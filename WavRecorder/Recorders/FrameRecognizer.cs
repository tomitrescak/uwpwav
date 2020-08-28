using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Storage.Streams;
using WavRecorder.Handlers;
using WavRecorder.Utils;

namespace WavRecorder.Recorders
{
    class FrameRecognizer: RecognizerBase
    {
        private InMemoryRandomAccessStream messageStream;
        private MediaFrameReader frameReader;
        public FrameRecognizer(string fileName) : base(fileName) { }


        public override async Task Start()
        {
            MediaRecorder = await MediaCaptureHandler.Init(MediaCategory.Media, AudioProcessing.Raw);

            // Start Recognition
            await this.StartRecognition();

            // Start frame recognizer
            var frameSources = MediaRecorder.FrameSources.Where(x => x.Value.Info.MediaStreamType == MediaStreamType.Audio);
            var source = frameSources.First().Value;

            frameReader = await MediaRecorder.CreateFrameReaderAsync(source);
            frameReader.AcquisitionMode = MediaFrameReaderAcquisitionMode.Buffered;
            frameReader.FrameArrived += OnFrameArrived;

            messageStream = new InMemoryRandomAccessStream();
            await messageStream.WriteAsync(Utils.Audio.CreateWavHeader().AsBuffer());

            var status = await frameReader.StartAsync();
            if (status != MediaFrameReaderStartStatus.Success)
            {
                throw new Exception("The MediaFrameReader couldn't start.");
            }
        }

        public override async Task Stop()
        {
            await this.StopRecognition();
            await frameReader.StopAsync();
            await FileUtils.Save(messageStream, this.FileName);
        }

        private void OnFrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            using (MediaFrameReference reference = sender.TryAcquireLatestFrame())
            {
                if (reference != null)
                {
                    ProcessAudioFrame(reference.AudioMediaFrame);
                }
            }
        }

        private unsafe void ProcessAudioFrame(AudioMediaFrame audioMediaFrame)
        {
            using (AudioFrame audioFrame = audioMediaFrame.GetAudioFrame())
            using (AudioBuffer buffer = audioFrame.LockBuffer(AudioBufferAccessMode.Read))
            using (IMemoryBufferReference reference = buffer.CreateReference())
            {
                ((IMemoryBufferByteAccess)reference).GetBuffer(out var dataInBytes, out _);

                // The requested format was float
                var dataInFloat = (float*)dataInBytes;

                // Get the number of samples by multiplying the duration by sampling rate: 
                // duration [s] x sampling rate [samples/s] = # samples 

                // Duration can be gotten off the frame reference OR the audioFrame
                TimeSpan duration = audioMediaFrame.FrameReference.Duration;

                // frameDurMs is in milliseconds, while SampleRate is given per second.
                uint frameDurMs = (uint)duration.TotalMilliseconds;
                uint sampleRate = audioMediaFrame.AudioEncodingProperties.SampleRate;
                uint channelCount = audioMediaFrame.AudioEncodingProperties.ChannelCount;
                uint sampleCount = (frameDurMs * sampleRate) / 1000;

                // extract data and convert to mono
                float[] floats = new float[sampleCount];

                if (channelCount > 1)
                {
                    for (uint i = 0; i < sampleCount * channelCount; i += channelCount)
                    {
                        var sum = 0f;
                        for (var j = 0; j < channelCount; j++)
                        {
                            sum += dataInFloat[i + j];
                        }
                        floats[i / channelCount] = sum / channelCount;
                    }
                }
                else
                {
                    for (var i = 0; i < sampleCount; i += 1)
                    {
                        floats[i] = dataInFloat[i];
                    }
                }

                // downsample
                if (sampleRate > 16000)
                {
                    floats = Utils.Audio.DownsampleBuffer(floats, sampleRate, 16000);
                }

                //if (!this.initialised)
                //{
                //    var storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder; 
                //    p = Path.Combine(storageFolder.Path, "out-2.wav");
                //    Debug.WriteLine(storageFolder.Path);

                //    // pushStream.Write(this.CreateWavHeader());
                //    this.initialised = true;

                //    wav.AddRange(this.CreateWavHeader());
                //}

                // TOMAS's WAY
                var newBuffer = Utils.Audio.ToByteArray(floats);
                messageStream.WriteAsync(newBuffer.AsBuffer());

                pushStream.Write(newBuffer);
            }
        }
    }
}

[ComImport]
[Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
unsafe interface IMemoryBufferByteAccess
{
    void GetBuffer(out byte* buffer, out uint capacity);
}
