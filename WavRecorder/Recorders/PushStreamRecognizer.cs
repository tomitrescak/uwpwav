using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using WavRecorder.Handlers;
using WavRecorder.Utils;

namespace WavRecorder.Recorders
{
    class PushStreamRecognizer: RecognizerBase
    {
        
        private MessageSendingStream messageStream;

        public PushStreamRecognizer(string fileName) : base(fileName) { }


        public override async Task Start()
        {
            MediaRecorder = await MediaCaptureHandler.Init(MediaCategory.Media, AudioProcessing.Default);

            // Start Recognition
            await this.StartRecognition();

            // Start Pushing to Recognition 
            messageStream = new MessageSendingStream(pushStream);
            await MediaRecorder.StartRecordToStreamAsync(Profile, messageStream);
        }

        public override async Task Stop()
        {
            await this.StopRecognition();
            await MediaRecorder.StopRecordAsync();
            await FileUtils.Save(messageStream, this.FileName);
        }

    }
}
