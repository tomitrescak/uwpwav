using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;

namespace WavRecorder.Handlers
{
    class MediaCaptureHandler
    {
        public static async Task<MediaCapture> Init(MediaCategory mediaCategory, AudioProcessing audioProcessingType)
        {
            await MicrophoneHandler.EnableMicrophone();

            var mediaCapture = new MediaCapture();
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.AudioCapture);
            var device = devices[0];
            var microphoneId = device.Id;

            await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                MediaCategory = mediaCategory,
                StreamingCaptureMode = StreamingCaptureMode.Audio,
                AudioDeviceId = microphoneId,
                AudioProcessing = audioProcessingType
            });

            return mediaCapture;
        }
    }
}
