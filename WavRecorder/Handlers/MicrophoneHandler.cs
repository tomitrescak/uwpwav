using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavRecorder.Handlers
{
    static class MicrophoneHandler
    {
        private static bool IsMicAvailable;
        
        public static async Task EnableMicrophone()
        {
            if (IsMicAvailable)
            {
                return;
            }

            IsMicAvailable = true;
            try
            {
                var mediaCapture = new Windows.Media.Capture.MediaCapture();
                var settings = new Windows.Media.Capture.MediaCaptureInitializationSettings();
                settings.StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.Audio;
                await mediaCapture.InitializeAsync(settings);
            }
            catch (Exception)
            {
                IsMicAvailable = false;
            }

            if (!IsMicAvailable)
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-microphone"));
            }
            else
            {
                Debug.WriteLine("Microphone was enabled");
            }
        }
    }
}
