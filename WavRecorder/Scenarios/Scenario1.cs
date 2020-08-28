using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using WavRecorder.Recorders;

namespace WavRecorder.Scenarios
{
    static class Scenario1
    {
        public static Scenario Init()
        {
            var mediaCatureSpeechDefaultScenario = new MediaCaptureRecorder("mediaCaptureSpeechDefaultScenario.wav")
            {
                Quality = AudioEncodingQuality.Low,
                MediaCategory = MediaCategory.Speech,
                AudioProcessingType = AudioProcessing.Default,
                ChannelCount = 1,
                SampleRate = 16000
            };
            var scenario = new Scenario(mediaCatureSpeechDefaultScenario, "1 Channel / Speech / Default");

            return scenario;
        }
    }
}
