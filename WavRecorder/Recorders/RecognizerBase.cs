using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;

namespace WavRecorder.Recorders
{
    public abstract class RecognizerBase: IRecognizer
    {
        private string apiKey = "a73728b86dd048c4b65d9043e9c1918a";
        private string region = "australiaeast";

        protected PushAudioInputStream pushStream { get; set; }
        protected MediaCapture MediaRecorder { get; set; }
        protected MediaEncodingProfile Profile { get; set; }

        public event Action RecognisionStarted;
        public event Action RecognisionStopped;
        public event Action<string> RecognisedSpeech;
        public event Action<string> RecognisedTranslation;

        public RecognizerBase(string fileName)
        {
            this.FileName = fileName;
        }

        public string FileName { get; set; }
        public abstract Task Start();

        private SpeechRecognizer speechRecognizer;
        private TranslationRecognizer translationRecognizer;
        private string toLanguage;

        private void Init(string from, string to)
        {
            this.toLanguage = to;

            Profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Low);
            Profile.Audio = AudioEncodingProperties.CreatePcm(16000, 1, 16);

            byte channels = 1;
            byte bitsPerSample = 16;
            uint samplesPerSecond = 16000; // or 8000
            var audioFormat = AudioStreamFormat.GetWaveFormatPCM(samplesPerSecond, bitsPerSample, channels);

            // Init Push Stream 

            pushStream = AudioInputStream.CreatePushStream(audioFormat);

            if (from == to)
            {
                var config = SpeechConfig.FromSubscription(apiKey, region);
                config.SpeechRecognitionLanguage = from;

                speechRecognizer = new SpeechRecognizer(config, AudioConfig.FromStreamInput(pushStream));

                speechRecognizer.Recognizing += RecognisingSpeechHandler;
                speechRecognizer.Recognized += RecognisingSpeechHandler;

                speechRecognizer.SessionStarted += (sender, args) => this.RecognisionStarted?.Invoke();
                speechRecognizer.SessionStopped += (sender, args) => this.RecognisionStopped?.Invoke();
            }
            else
            {
                var config = SpeechTranslationConfig.FromSubscription(apiKey, region);
                config.SpeechRecognitionLanguage = from;
                config.AddTargetLanguage(to);

                translationRecognizer = new TranslationRecognizer(config, AudioConfig.FromStreamInput(pushStream));
                
                translationRecognizer.SessionStarted += (sender, args) => this.RecognisionStarted?.Invoke();
                translationRecognizer.SessionStopped += (sender, args) => this.RecognisionStopped?.Invoke();

                translationRecognizer.Recognizing += RecognisingTranslationHandler;
                translationRecognizer.Recognized += RecognisingTranslationHandler;
            }
            
        }

        private void RecognisingSpeechHandler(object sender, SpeechRecognitionEventArgs e)
        {
            if (e.Result.Text != null)
            {
                this.RecognisedSpeech?.Invoke(e.Result.Text);
            }
        }

        private void RecognisingTranslationHandler(object sender, TranslationRecognitionEventArgs e)
        {
            
            if (e.Result.Text != null)
            {
                this.RecognisedSpeech?.Invoke(e.Result.Text);
            }

            if (e.Result.Translations[toLanguage] != null)
            {
                this.RecognisedTranslation?.Invoke(e.Result.Translations[toLanguage]);
            }
        }

        protected async Task StartRecognition()
        {
           
            if (speechRecognizer != null)
            {
                await speechRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            }
            if (translationRecognizer != null)
            {
                await translationRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            }
        }

        protected async Task StopRecognition()
        {
            if (speechRecognizer != null)
            {
                await speechRecognizer.StopContinuousRecognitionAsync();
            }
            if (translationRecognizer != null)
            {
                await translationRecognizer.StopContinuousRecognitionAsync();
            }

            speechRecognizer = null;
            translationRecognizer = null;
        }

        public async Task Start(string from, string to)
        {
            this.Init(from, to);
            await this.Start();
        }
        public abstract Task Stop();
    }
}
