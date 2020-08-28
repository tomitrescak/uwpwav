using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using WavRecorder.Handlers;
using WavRecorder.Utils;

namespace WavRecorder.Recorders
{
    public class MediaCaptureRecorder: ProcessorBase, IRecorder
    {
        private InMemoryRandomAccessStream memoryBuffer;
        public MediaCapture Capture { get; private set; }
        public MediaEncodingProfile Profile { get; private set; }
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

        public string FileName { get; set; }

        public async Task Start()
        {
            Capture = await MediaCaptureHandler.Init(MediaCategory, AudioProcessingType);

            Profile = MediaEncodingProfile.CreateWav(Quality);
            Profile.Audio = AudioEncodingProperties.CreatePcm(SampleRate, ChannelCount, BitsPerSample);

            memoryBuffer = new InMemoryRandomAccessStream();
            await Capture.StartRecordToStreamAsync(Profile, memoryBuffer);
        }

        public async Task Stop()
        {
            await Capture.StopRecordAsync();
            await FileUtils.Save(memoryBuffer, this.FileName);
        }
    }
}
