using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;

namespace WavRecorder
{
    public class MediaCaptureRecorder: IRecorder
    {
        private InMemoryRandomAccessStream memoryBuffer;
        private MediaCapture mediaCapture;

        public MediaCaptureRecorder()
        {
            Quality = AudioEncodingQuality.Low;
            SampleRate = 16000;
            ChannelCount = 1;
            BitsPerSample = 16;
            MediaCategory = MediaCategory.Speech;
            AudioProcessingType = AudioProcessing.Default;
        }

        public MediaCaptureRecorder(string fileName)
        {
            this.FileName = fileName;
        }

        public AudioEncodingQuality Quality { get; set; }
        public uint SampleRate { get; set; }
        public uint ChannelCount { get; set; }
        public uint BitsPerSample { get; set; }
        public MediaCategory MediaCategory { get; set; }

        public AudioProcessing AudioProcessingType { get; set; }

        public string FileName { get; set; }

        

        public async Task Start()
        {
            await MicrophoneHandler.EnableMicrophone();

            mediaCapture = new MediaCapture();
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.AudioCapture);
            var device = devices[0];
            var microphoneId = device.Id;

            var outProfile = MediaEncodingProfile.CreateWav(Quality);

            outProfile.Audio = AudioEncodingProperties.CreatePcm(SampleRate, ChannelCount, BitsPerSample);
            

            await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                MediaCategory = MediaCategory,
                StreamingCaptureMode = StreamingCaptureMode.Audio,
                AudioDeviceId = microphoneId,
                AudioProcessing = AudioProcessingType
            });

            memoryBuffer = new InMemoryRandomAccessStream();
            await mediaCapture.StartRecordToStreamAsync(outProfile, memoryBuffer);
        }

        public async Task Stop()
        {
            await mediaCapture.StopRecordAsync();

            IRandomAccessStream audioStream = memoryBuffer.CloneStream();
            StorageFolder storageFolder = KnownFolders.MusicLibrary;
            StorageFile storageFile = await storageFolder.CreateFileAsync(
                this.FileName, CreationCollisionOption.ReplaceExisting);

            using (IRandomAccessStream fileStream =
                await storageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                await RandomAccessStream.CopyAndCloseAsync(
                    audioStream.GetInputStreamAt(0), fileStream.GetOutputStreamAt(0));
                await audioStream.FlushAsync();
                audioStream.Dispose();
            }
        }
    }
}
