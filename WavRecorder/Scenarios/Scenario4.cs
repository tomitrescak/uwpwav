using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;

namespace WavRecorder.Scenarios
{
    class Scenario4
    {
        public static Scenario Init()
        {
            var mediaCatureSpeechDefaultScenario = new MediaCaptureRecorder("mediaCaptureMediaRaw11Scenario.wav")
            {
                Quality = AudioEncodingQuality.Low,
                MediaCategory = MediaCategory.Media,
                AudioProcessingType = AudioProcessing.Raw,
                ChannelCount = 11,
                SampleRate = 16000,
            };
            var scenario = new Scenario(mediaCatureSpeechDefaultScenario, "Media / Raw / Forced 11 Channels");

            return scenario;
        }
    }
}
