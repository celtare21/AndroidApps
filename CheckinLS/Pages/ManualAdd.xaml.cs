﻿using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualAdd : ContentPage
    {
        public ManualAdd()
        {
            InitializeComponent();

            add_button.Clicked += Add_button_Clicked;
        }

        private async void Add_button_Clicked(object sender, System.EventArgs e)
        {
            if (!CursToggle.IsToggled && !PregatireToggle.IsToggled && !RecuperareToggle.IsToggled)
                return;

            add_button.IsEnabled = false;

            await Home.AddNewEntryWrapper(CursToggle.IsToggled, PregatireToggle.IsToggled, RecuperareToggle.IsToggled);

            add_button.IsEnabled = true;
        }
    }
}