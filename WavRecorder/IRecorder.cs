using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavRecorder
{
    public interface IRecorder
    {
        string FileName { get; set; }
        Task Start();
        Task Stop();
    }
}
