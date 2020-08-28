using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using WavRecorder.Handlers;
using WavRecorder.Recorders;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace WavRecorder
{
    public sealed partial class SpeechRecognizerControl : UserControl
    {
        private RecognizerBase handler;
        private bool started;
        private string fromLanguage;
        private string toLanguage;




        public SpeechRecognizerControl()
        {
            this.InitializeComponent();
        }

        public SpeechRecognizerControl(string title, RecognizerBase recognizer) : this()
        {
            handler = recognizer;

            handler.RecognisedSpeech += SpeechHandler;
            handler.RecognisedTranslation += TranslationHandler;

            handler.RecognisionStarted += SessionStartedHandler;
            handler.RecognisionStopped += SessionStoppedHandler;

            this.Header.Text = title;

        }

        private async void ToggleRecognition(object sender, RoutedEventArgs e)
        {
            if (this.started)
            {
                await this.handler.Stop();
                this.started = false;
                this.ToggleButton.Content = "Start";
                return;

            }

            fromLanguage = FromLanguage.SelectedValue?.ToString();
            if (fromLanguage == null)
            {
                fromLanguage = "en-US";
            }
            toLanguage = ToLanguage.SelectedValue?.ToString();
            if (toLanguage == null)
            {
                toLanguage = "en-US";
            }

            this.ToggleButton.Content = "Stop";
            await handler.Start(fromLanguage, toLanguage);
            this.started = true;
        }

        #region Speech Recognition Event Handlers
        private async void SessionStartedHandler()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Circle.Visibility = Visibility.Visible;
                StatusText.Text = "Session Started ...";
            });
        }

        private async void SessionStoppedHandler()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Circle.Visibility = Visibility.Collapsed;
                StatusText.Text = "Session Stopped ...";
            });
        }

        private async void SpeechHandler(string speech)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {

                RecognisedText.Text = speech;
                Scroller1.ChangeView(null, Scroller1.ExtentHeight, null, false);

            });
        }

        private async void TranslationHandler(string translation)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {

                TranslatedText.Text = translation;
                Scroller2.ChangeView(null, Scroller2.ExtentHeight, null, false);

            });
        }

        //private void SpeechStartDetected(object sender, RecognitionEventArgs e)
        //{
        //}

        //private void SpeechEndDetectedHandler(object sender, RecognitionEventArgs e)
        //{
        //}

        //private void CancelHandler(object sender, RecognitionEventArgs e)
        //{
        //}
        #endregion
    }
}
