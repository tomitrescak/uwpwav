using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WavRecorder.Recorders;
using WavRecorder.Utils;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace WavRecorder
{
    public sealed partial class Scenario : UserControl
    {
        private bool _recording;

        public string ScenarioHeader
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public IRecorder Recorder { get; set; }

        public Scenario()
        {
            this.InitializeComponent();
        }

        public Scenario(IRecorder recorder, string header)
        {

            this.Recorder = recorder;
            this.InitializeComponent();
            this.CheckFile();
            this.ScenarioHeader = header;
        }

        public async void CheckFile()
        {
            StorageFolder storageFolder = KnownFolders.MusicLibrary;
            var checkFile = await storageFolder.TryGetItemAsync(this.Recorder.FileName);

            if (checkFile == null)
            {
                this.Info.Text = "Wav not recorded ...";
            }
            else
            {

                StorageFile storageFile = await storageFolder.GetFileAsync(this.Recorder.FileName);
                var parser = new RiffParser();

                // read file
                await Windows.System.Threading.ThreadPool.RunAsync(
                    async (workItem) =>
                    {
                        IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.Read);
                        Stream sstream = stream.AsStream();
                        parser.OpenFile(sstream);
                        // Define the processing delegates
                        RiffParser.ProcessChunkElement pc = ProcessChunk;

                        // Read all top level elements and chunks
                        int size = parser.DataSize;
                        while (size > 0)
                        {
                            // Prefix the line with the current top level type
                            var info = (RiffParser.FromFourCC(parser.FileType) + " (" + size.ToString() + "): ");
                            // Get the next element (if there is one)
                            if (false == parser.ReadElement(ref size, pc, null)) break;
                        }

                        // Close the stream
                        parser.CloseFile();
                    });
            }
        }


        // Process a RIFF chunk element (skip the data)
        public void ProcessChunk(RiffParser rp, int FourCC, int length, int paddedLength)
        {
            string type = RiffParser.FromFourCC(FourCC);
            Debug.WriteLine("Found chunk element of type \"" +
                            type + "\" and length " + length.ToString());

            if (type.Trim() == "fmt")
            {
                BinaryReader br = new BinaryReader(rp.m_stream);


                var format = br.ReadUInt16();
                var channels = br.ReadUInt16();
                var sampleRate = br.ReadUInt32();
                var bytePerSec = br.ReadUInt32();
                var blockSize = br.ReadUInt16();
                var bit = br.ReadUInt16();

                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.Info.Text = $"{channels} channels / {sampleRate} hz / {bit} bit / {bytePerSec} bytes/sec";
                });

                rp.SkipData(paddedLength - 16);
            }
            else
            {

                // Skip data and update bytesleft
                rp.SkipData(paddedLength);
            }
        }

        private async void RecordClicked(object sender, RoutedEventArgs e)
        {
            if (_recording)
            {
                await this.Recorder.Stop();
                _recording = false;
                this.RecordButton.Content = "Start Recording";
                this.Info.Text = "Will get info ...";
                this.CheckFile();
            }
            else
            {
                await this.Recorder.Start();
                _recording = true;
                this.RecordButton.Content = "Stop Recording";
                this.Info.Text = "Recording ...";
            }

        }

        private async void PlayClicked(object sender, RoutedEventArgs e)
        {
            await this.PlayFromDisk();
        }

        public async Task PlayFromDisk()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                MediaElement playbackMediaElement = new MediaElement();
                StorageFolder storageFolder = KnownFolders.MusicLibrary;
                StorageFile storageFile = await storageFolder.GetFileAsync(this.Recorder.FileName);
                IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.Read);
                playbackMediaElement.SetSource(stream, storageFile.FileType);
                playbackMediaElement.Play();
            });
        }


        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("ScenarioHeader", typeof(string), typeof(Scenario), new PropertyMetadata("Your Header", HeaderPropertyChangedCallback));

        public static void HeaderPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                (d as Scenario).Group.Header = e.NewValue?.ToString();
            }
        }
    }
}
