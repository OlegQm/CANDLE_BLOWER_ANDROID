/*
          Add packages:

 * SkiaSharp.Views.Forms (1.68.3)
 * SkiaSharp.Waveform (0.2.0)
 * JaybirdLabs.Chrip (0.7.1)
 * Xam.Plugin.SimpleAudioPlayer (1.6.0)
   
 */

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System;
using JaybirdLabs.Chirp;
using Plugin.SimpleAudioPlayer;
using SkiaSharp.Waveform;
using SkiaSharp;
using Android.Media;
using CandlyBlow;

namespace CandleBlow.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SoundGenerator : ContentPage
    {
        private ISimpleAudioPlayer soundPlayer = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
        private System.IO.Stream stream;
        private Waveform _waveForm;
        private AudioManager setSound = ExternData.soundLevel;
        private float[] normalizedAmplitudes;
        private bool isHoldingPlus = false;
        private bool isHoldingMinus = false;

        static float[] ToNormalizedFloat(short[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var result = new float[data.Length];

            for (var i = 0; i < data.Length; i++)
            {
                result[i] = (float)data[i] / short.MaxValue;
            }

            return result;
        }
        private System.IO.Stream GenerateAudioStream(int frequency, int durationInSeconds)
        {
            var streamResult = new StreamGenerator().GenerateStream(SignalGeneratorType.Sin, frequency, durationInSeconds);
            short[] amplitudes = streamResult.Amplitudes;
            normalizedAmplitudes = ToNormalizedFloat(amplitudes);
            return streamResult.WaveStream;
        }
        private void PlaySound(int frequency, int durationInSeconds)
        {
            soundPlayer = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            stream = GenerateAudioStream(frequency, durationInSeconds);
            soundPlayer.Load(stream);
            soundPlayer.Loop = true;
            soundPlayer.Play();
        }
        public SoundGenerator()
        {
            InitializeComponent();
        }
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            if (setSound.GetStreamVolume(Stream.Music) != setSound.GetStreamMaxVolume(Stream.Music))
            {
                bool chooseSoundMax = await DisplayAlert("Warning",
                            "Can we turn up the maximum volume on your device?",
                            "Yes", "No");
                if (chooseSoundMax)
                    setSound.SetStreamVolume(Stream.Music, setSound.GetStreamMaxVolume(Stream.Music), 0);
            }
        }

        protected async override void OnDisappearing()
        {
            soundPlayer.Stop();
            soundPlayer.Dispose();
            base.OnDisappearing();
            int maxVolume = setSound.GetStreamMaxVolume(Stream.Music);
            if (setSound.GetStreamVolume(Stream.Music) >= (maxVolume - (int)(maxVolume / 4)))
                await DisplayAlert("Warning",
                    "Your volume is set at a high level, we recommend turning it down",
                    "OK");
        }

        private void frequencyChanger_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            soundPlayer.Stop();
            frequencyValue.Text = "FREQUENCY: " + Convert.ToString((int)frequencyChanger.Value) + " HZ";
            PlaySound(Convert.ToInt32(frequencyChanger.Value), 3);

            _waveForm = new Waveform.Builder()
                    .WithAmplitudes(normalizedAmplitudes)
                    .WithSpacing(5f)
                    .WithColor(new SKColor(0x00, 0x00, 0xff))
                    .Build();

            WaveformCanvas.InvalidateSurface();
        }

        private void EnablingAndDisabling(bool TurnOnOFF)
        {
            frequencyChanger.IsEnabled = TurnOnOFF;
            plusFrequence.IsEnabled = TurnOnOFF;
            minusFrequence.IsEnabled = TurnOnOFF;
        }

        private void Start_btn_Clicked(object sender, EventArgs e)
        {

            if (Start_btn.Text == "START")
            {
                soundPlayer.Volume = 1;
                EnablingAndDisabling(true);
                Pause_btn.IsEnabled = true;
                frequencyChanger.Value = 0;
                frequencyChanger.Value = 865;
                Start_btn.Text = "STOP";
            }
            else
            {
                soundPlayer.Stop();
                soundPlayer.Dispose();
                EnablingAndDisabling(false);
                Pause_btn.IsEnabled = false;
                Start_btn.Text = "START";
            }
            Pause_btn.Text = "PAUSE";
            _waveForm = new Waveform.Builder()
                .WithAmplitudes(normalizedAmplitudes)
                .WithSpacing(5f)
                .WithColor(new SKColor(0x00, 0x00, 0xff))
                .Build();
        }
        private void Pause_btn_Clicked(object sender, EventArgs e)
        {
            if (Pause_btn.Text == "PAUSE")
            {
                soundPlayer.Pause();
                EnablingAndDisabling(false);
                Pause_btn.Text = "CONTINUE";
            }
            else
            {
                soundPlayer.Play();
                EnablingAndDisabling(true);
                Pause_btn.Text = "PAUSE";
            }
        }

        private void plusFrequence_Pressed(object sender, EventArgs e)
        {
            isHoldingPlus = true;
            Device.StartTimer(TimeSpan.FromMilliseconds(5), () =>
            {
                if (isHoldingPlus)
                {
                    frequencyChanger.Value += 5;
                    return true;
                }
                else return false;
            });
        }

        private void plusFrequence_Released(object sender, EventArgs e)
        {
            isHoldingPlus = false;
        }

        private void minusFrequence_Pressed(object sender, EventArgs e)
        {
            isHoldingMinus = true;
            Device.StartTimer(TimeSpan.FromMilliseconds(5), () =>
            {
                if (isHoldingMinus)
                {
                    frequencyChanger.Value -= 5;
                    return true;
                }
                else return false;
            });
        }

        private void minusFrequence_Released(object sender, EventArgs e)
        {
            isHoldingMinus = false;
        }

        private void WaveformCanvas_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            if (_waveForm != null)
                _waveForm.DrawOnCanvas(e.Surface.Canvas);
        }
    }
}