using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Microsoft.CognitiveServices.Speech.Audio;

namespace WavRecorder.Utils
{
    internal class MessageSendingStream : IRandomAccessStream
    {
        private PushAudioInputStream pushStream;
        private InMemoryRandomAccessStream memoryBuffer;

        // Raised when the connection with the service is broken.
        public event EventHandler ConnectionBroken;

        public MessageSendingStream(PushAudioInputStream pushStream)
        {
            this.pushStream = pushStream;
            this.memoryBuffer = new InMemoryRandomAccessStream();
        }


        public bool CanRead { get { return true; } }
        public bool CanWrite { get { return true; } }

        public IRandomAccessStream CloneStream()
        {
            return memoryBuffer.CloneStream();
        }


        public IInputStream GetInputStreamAt(ulong position)
        {
            return memoryBuffer.GetInputStreamAt(position);
        }

        public IOutputStream GetOutputStreamAt(ulong position)
        {
            return this.memoryBuffer.GetOutputStreamAt(position);
        }

        public ulong Position
        {
            get { return (ulong)this.memoryBuffer.Position; }
        }

        public void Seek(ulong position)
        {
            this.memoryBuffer.Seek(position);
        }

        public ulong Size
        {
            get { return (ulong)this.memoryBuffer.Size; }
            set { this.memoryBuffer.Size = value; }
        }

        public void Dispose()
        {
            this.memoryBuffer.Dispose();
        }

        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(
            IBuffer buffer, uint count, InputStreamOptions options)
        {
            return this.memoryBuffer.ReadAsync(buffer, count, options);
        }

        public IAsyncOperation<bool> FlushAsync()
        {
            return this.memoryBuffer.FlushAsync();
        }

        // Implements sending of video/audio data to the service.
        // The message is encoded the following way:
        // 4 bytes - position in MP4 file where data shall be put.
        // n bytes - video/audio data. 
        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            Task<uint> aTask = new Task<uint>( () =>
            {
                uint aVideoDataLength = buffer.Length;
                byte[] aMessage = new byte[aVideoDataLength + 4];

                // Put position within MP4 file to the message.
                byte[] aPosition = BitConverter.GetBytes((int) this.Position);
                Array.Copy(aPosition, aMessage, aPosition.Length);

                // Put video/audio data to the message.
                buffer.CopyTo(0, aMessage, 4, (int)aVideoDataLength);



                uint aTransferedSize = 0;
                try
                {
                    // Send the message to the service.
                    // myOutputChannel.SendMessage(aMessage);
                    this.pushStream.Write(aMessage);

                    memoryBuffer.WriteAsync(buffer);
                    aTransferedSize = (uint)aVideoDataLength;

                }
                catch
                {
                    // If sending fails then the connection is broken.
                    if (ConnectionBroken != null)
                    {
                        ConnectionBroken(this, new EventArgs());
                    }
                }

                return aTransferedSize;
            });
            aTask.RunSynchronously();

            Func<CancellationToken, IProgress<uint>, Task<uint>> aTaskProvider =
                (token, progress) => aTask;
            return AsyncInfo.Run<uint, uint>(aTaskProvider);
        }

        private static async Task<IRandomAccessStream> ConvertToRandomAccessStream(MemoryStream memoryStream)
    {
        var randomAccessStream = new InMemoryRandomAccessStream();
        var outputStream = randomAccessStream.GetOutputStreamAt(0);
        await RandomAccessStream.CopyAndCloseAsync(memoryStream.AsInputStream(), outputStream);
        var result = RandomAccessStreamReference.CreateFromStream(randomAccessStream);
        return randomAccessStream;
    }
    }
}
