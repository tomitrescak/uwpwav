using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WavRecorder.Scenarios;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WavRecorder
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // init scenarios

            // #1 MediaCapture / Speech / Default
            //var mediaCatureSpeechDefaultScenario = new MediaCaptureRecorder("mediaCatureSpeechDefaultScenario.wav");
            //this.MediaCatureSpeechDefaultScenario.Recorder = mediaCatureSpeechDefaultScenario;
            //this.MediaCatureSpeechDefaultScenario.CheckFile();

            this.Stack.Children.Add(Scenario1.Init());
            this.Stack.Children.Add(Scenario2.Init());
            this.Stack.Children.Add(Scenario3.Init());
            this.Stack.Children.Add(Scenario4.Init());

        }
    }
}
