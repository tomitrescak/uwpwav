using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;

namespace WavRecorder.Recorders
{
    public abstract class ProcessorBase
    {
        public AudioEncodingQuality Quality { get; set; }
        public uint SampleRate { get; set; }
        public uint ChannelCount { get; set; }
        public uint BitsPerSample { get; set; }
        public MediaCategory MediaCategory { get; set; }
        public AudioProcessing AudioProcessingType { get; set; }
    }
}
