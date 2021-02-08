using CheckinLS.API.Misc;
using CheckinLS.API.Sql;
using CheckinLS.API.Standard;
using CheckinLS.InterfacesAndClasses.Date;
using Microsoft.AppCenter.Analytics;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static CheckinLS.API.Misc.TimerHelper;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Home
    {
        private StandardElements _elements;

        public Home() =>
            InitializeComponent();

        public async Task CreateElementsAsync()
        {
            if (MainSql.UserHasOffice())
                OreOfficeButton.IsVisible = true;

            _elements = await StandardElements.CreateAsync(new GetDate());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ButtonTimer.Elapsed += ButtonTimer_Elapsed;

            PretTotalVechi.Text = Preferences.Get("totalVechi", 0.0).ToString(CultureInfo.InvariantCulture);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            ButtonTimer.Elapsed -= ButtonTimer_Elapsed;
            ButtonTimer.Stop();
        }

        protected override bool OnBackButtonPressed()
        {
            Device.InvokeOnMainThreadAsync(async () =>
            {
                var result = await DisplayAlert("Alert!", "Do you really want to exit the application?", "Yes", "No");

                if (result)
                    App.Close();
            });

            return true;
        }

        private void LeftButton_Pressed(object sender, EventArgs e)
        {
            ButtonTimer.Start();
            StartTime = DateTime.Now;
            LeftRightButton = false;
        }

        private void LeftButton_Released(object sender, EventArgs e)
        {
            ButtonTimer.Stop();

            if (DateTime.Now - StartTime >= TimeSpan.FromMilliseconds(TimerInternal))
                return;
            if (_elements == null || _elements.Index < 1)
                return;

            --_elements.Index;
            RefreshPage(false);
        }

        private void RightButton_Pressed(object sender, EventArgs e)
        {
            ButtonTimer.Start();
            StartTime = DateTime.Now;
            LeftRightButton = true;
        }

        private void RightButton_Released(object sender, EventArgs e)
        {
            ButtonTimer.Stop();

            if (DateTime.Now - StartTime >= TimeSpan.FromMilliseconds(TimerInternal))
                return;
            if (_elements == null || _elements.Index > _elements.MaxElement() - 1)
                return;

            ++_elements.Index;
            RefreshPage(false);
        }

        private async void ButtonTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_elements == null)
                return;

            _elements.Index = LeftRightButton ? _elements.MaxElement() : 0;

            try
            {
                HapticFeedback.Perform(HapticFeedbackType.LongPress);
            }
            catch (FeatureNotSupportedException)
            {
                Vibration.Vibrate(100);
            }

            await Device.InvokeOnMainThreadAsync(() => RefreshPage(false)).ConfigureAwait(false);
        }

        private async void DeleteButton_Clicked(object sender, EventArgs e)
        {
            if (_elements == null || _elements.MaxElement() < 0)
                return;

            var result = await DisplayAlert("Alert!", "Are you sure you want to delete the entry?", "Yes", "No");

            if (!result)
            {
                Analytics.TrackEvent("Delete entry cancelled");
                return;
            }

            Analytics.TrackEvent("Entry deleted");

            DeleteButton.IsEnabled = false;

            await _elements.DeleteEntryAsync(Convert.ToInt32(IdLabel.Text));

            if (_elements.Index > _elements.MaxElement())
                --_elements.Index;
            RefreshPage(true);

            await HelperFunctions.ShowToastAsync("Entry deleted!");

            DeleteButton.IsEnabled = true;
        }

        private void ManualAddButton_Clicked(object sender, EventArgs e) =>
            Navigation.PushModalAsync(new HomeAddPage(_elements, this));

        private async void OreOfficeButton_Clicked(object sender, EventArgs e)
        {
            var officeClass = new OfficeHomePage();
            await Navigation.PushModalAsync(officeClass);
            await officeClass.CreateElementsAsync();
            officeClass.RefreshPage();
        }

        public void RefreshPage(bool price)
        {
            if (_elements == null || _elements.MaxElement() < 0)
            {
                IdLabel.Text = Observatii.Text = Date.Text = OraIncepere.Text = OraSfarsit.Text =
                    CursAlocat.Text = PregatireAlocat.Text = RecuperareAlocat.Text =
                        Total.Text = "Not found!";
                PretTotal.Text = "0";
            }
            else
            {
                (IdLabel.Text, Observatii.Text, Date.Text, OraIncepere.Text, OraSfarsit.Text, CursAlocat.Text,
                        PregatireAlocat.Text, RecuperareAlocat.Text, Total.Text) =
                    (HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].Id),
                        HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].Observatii),
                        HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].Date),
                        HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].OraIncepere),
                        HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].OraFinal),
                        HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].CursAlocat),
                        HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].PregatireAlocat),
                        HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].RecuperareAlocat),
                        HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].Total));

                if (price)
                    SetPrice();
            }
        }

        private void SetPrice()
        {
            if (_elements == null)
                return;

            var valoare = new double[2];
            foreach (var entry in _elements.Entries)
            {
                valoare[entry.Date.Month == DateTime.Today.SubstractMonths(1).Month ? 0 : 1] += entry.CursAlocat.TotalHours * Constants.PretCurs;
                valoare[entry.Date.Month == DateTime.Today.SubstractMonths(1).Month ? 0 : 1] += entry.PregatireAlocat.TotalHours * Constants.PretPregatire;
                valoare[entry.Date.Month == DateTime.Today.SubstractMonths(1).Month ? 0 : 1] += entry.RecuperareAlocat.TotalHours * Constants.PretRecuperare;
            }

            if (!Preferences.ContainsKey("totalVechi") || valoare[0] != 0.0)
            {
                Preferences.Set("totalVechi", valoare[0]);
                PretTotalVechi.Text = valoare[0].ToString(CultureInfo.InvariantCulture);
            }

            PretTotal.Text = valoare[1].ToString(CultureInfo.InvariantCulture);
        }
    }
}