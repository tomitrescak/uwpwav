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
    class Scenario2
    {
        public static Scenario Init()
        {
            var mediaCatureSpeechDefaultScenario = new MediaCaptureRecorder("mediaCaptureMediaDefaultScenario.wav")
            {
                Quality = AudioEncodingQuality.Low,
                MediaCategory = MediaCategory.Media,
                AudioProcessingType = AudioProcessing.Default,
                ChannelCount = 1,
                SampleRate = 16000
            };
            var scenario = new Scenario(mediaCatureSpeechDefaultScenario, "Media / Default");

            return scenario;
        }
    }
}
