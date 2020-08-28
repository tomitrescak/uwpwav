using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;

namespace WavRecorder.Recorders
{
    public interface IRecorder
    {
        string FileName { get; set; }
        Task Start();
        Task Stop();
    }

    public interface IRecognizer: IRecorder
    {
        Task Start(string from, string to);
    }
}
