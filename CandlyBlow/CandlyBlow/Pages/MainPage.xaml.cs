using System;
using Xamarin.Forms;
using CandleBlow.Pages;

namespace CandleBlow.Pages

{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Oscillator_Clicked(object sender, EventArgs e)
        {
            SoundGenerator page = new SoundGenerator();
            GenerateSoundButton.IsEnabled = false;
            await Navigation.PushAsync(page);
            GenerateSoundButton.IsEnabled = true;
        }
    }
}
