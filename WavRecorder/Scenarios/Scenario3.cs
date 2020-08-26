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
    class Scenario3
    {
        public static Scenario Init()
        {
            var mediaCatureSpeechDefaultScenario = new MediaCaptureRecorder("mediaCaptureMediaRawScenario.wav")
            {
                Quality = AudioEncodingQuality.Low,
                MediaCategory = MediaCategory.Media,
                AudioProcessingType = AudioProcessing.Raw
            };
            var scenario = new Scenario(mediaCatureSpeechDefaultScenario, "Media / Raw");

            return scenario;
        }
    }
}
