using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavRecorder.Utils
{
    static class Audio
    {
        public static byte[] ToByteArray(float[] samples)
        {
            Int16[] intData = new Int16[samples.Length];
            //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

            Byte[] bytesData = new Byte[samples.Length * 2];
            //bytesData array is twice the size of
            //dataSource array because a float converted in Int16 is 2 bytes.

            const float rescaleFactor = 32767; //to convert float to Int16

            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * rescaleFactor);
                Byte[] byteArr = new Byte[2];
                byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            return bytesData;
        }

        public static float[] DownsampleBuffer(float[] buffer, uint sampleRate, uint rate)
        {

            if (rate == sampleRate)
            {
                return buffer;
            }
            if (rate > sampleRate)
            {
                throw new Exception("downsampling rate show be smaller than original sample rate");
            }
            int sampleRateRatio = (int)(sampleRate / rate);
            int newLength = buffer.Length / sampleRateRatio;
            var result = new float[newLength];
            var offsetResult = 0;
            var offsetBuffer = 0;
            while (offsetResult < result.Length)
            {
                var nextOffsetBuffer = (int)((offsetResult + 1) * sampleRateRatio);
                // Use average value of skipped samples
                float accum = 0;
                var count = 0;
                for (var i = offsetBuffer; i < nextOffsetBuffer && i < buffer.Length; i++)
                {
                    accum += buffer[i];
                    count++;
                }
                result[offsetResult] = accum / count;
                // Or you can simply get rid of the skipped samples:
                // result[offsetResult] = buffer[nextOffsetBuffer];
                offsetResult++;
                offsetBuffer = nextOffsetBuffer;
            }
            return result;
        }

        public static byte[] CreateWavHeader()
        {
            var rate = 16000;
            var bits = 16;
            var channels = 1;
            var blockAlign = (short)(channels * (bits / 8));
            var averageBytesPerSecond = rate * blockAlign;
            byte[] binaryData;
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms, System.Text.Encoding.UTF8))
                {
                    var bs = System.Text.Encoding.UTF8.GetBytes("RIFF");
                    bw.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
                    bw.Write((byte)102);
                    bw.Write((byte)252);
                    bw.Write((byte)54);
                    bw.Write((byte)0); // placeholder
                    bw.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));

                    bw.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));


                    bw.Write((int)(18)); // wave format length
                    bw.Write((short) 1);
                    bw.Write((short) channels);
                    bw.Write((int) rate);
                    bw.Write((int) averageBytesPerSecond);
                    bw.Write((short) blockAlign);
                    bw.Write((short) bits);
                    bw.Write((short) 0);

                    bw.Write(System.Text.Encoding.UTF8.GetBytes("data"));
                    bw.Write((byte)64);
                    bw.Write((byte)243);
                    bw.Write((byte)102);
                    bw.Write((byte)0); // placeholder

                    binaryData = ms.ToArray();
                }
            }
            return binaryData;
        }
    }
}
