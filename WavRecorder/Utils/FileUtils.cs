using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace WavRecorder.Utils
{
    static class FileUtils
    {
        public static async Task Save(IRandomAccessStream messageStream, string fileName)
        {
            IRandomAccessStream audioStream = messageStream.CloneStream();
            StorageFolder storageFolder = KnownFolders.MusicLibrary;
            StorageFile storageFile = await storageFolder.CreateFileAsync(
                fileName, CreationCollisionOption.ReplaceExisting);

            using (IRandomAccessStream fileStream =
                await storageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                await RandomAccessStream.CopyAndCloseAsync(
                    audioStream.GetInputStreamAt(0), fileStream.GetOutputStreamAt(0));
                await audioStream.FlushAsync();
                audioStream.Dispose();
            }
        }
    }
}
