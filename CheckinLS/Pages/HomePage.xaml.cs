using Acr.UserDialogs;
using CheckinLS.API.Misc;
using CheckinLS.API.Sql;
using CheckinLS.API.Standard;
using CheckinLS.InterfacesAndClasses.Date;
using Microsoft.AppCenter.Analytics;
using Plugin.NFC;
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
        private bool _fakeListener, _busy, _disableNfcError, _startup = true;
        private (bool curs, bool pregatire, bool recuperare) _ora = (false, false, false);

        public Home() =>
            InitializeComponent();

        public async Task CreateElementsAsync()
        {
            if (MainSql.UserHasOffice())
                OreOfficeButton.IsVisible = true;

            _elements = await StandardElements.CreateAsync(new GetDate());
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await AddEventsAsync().ConfigureAwait(false);
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            await RemoveEventsAsync();
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

        private Task AddEventsAsync()
        {
            ButtonTimer.Elapsed += ButtonTimer_Elapsed;
            CrossNFC.Current.OnNfcStatusChanged += Current_OnNfcStatusChanged;
            return StartListeningAsync();
        }

        private Task RemoveEventsAsync()
        {
            ButtonTimer.Elapsed -= ButtonTimer_Elapsed;
            CrossNFC.Current.OnNfcStatusChanged -= Current_OnNfcStatusChanged;
            return StopListeningAsync();
        }

        public async Task CheckNfcStatusAsync()
        {
            if (!CrossNFC.Current.IsEnabled)
            {
                Indicator.Color = Color.Gray;

                if (!_disableNfcError)
                {
                    await DisplayAlert("Error", "NFC is disabled", "OK");
                    _disableNfcError = true;
                }
            }
            else
            {
                Indicator.Color = Color.Green;
            }
        }

        private async void Current_OnNfcStatusChanged(bool isEnabled)
        {
            if (!isEnabled)
            {
                Indicator.Color = Color.Gray;
                if (_disableNfcError)
                    return;

                await DisplayAlert("Error", "NFC is disabled", "OK");
                _disableNfcError = true;
            }
            else
            {
                Indicator.Color = Color.Green;
            }
        }

        private Task StartListeningAsync() =>
            Device.InvokeOnMainThreadAsync(() =>
            {
                SubscribeEventsReal();

                if (!_startup)
                    return;

                try
                {
                    CrossNFC.Current.StartListening();
                }
                catch
                {
                    App.Close();
                }

                _startup = false;
            });

        private Task StopListeningAsync() =>
            Device.InvokeOnMainThreadAsync(SubscribeFake);

        private void SubscribeEventsReal()
        {
            if (_fakeListener)
                CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceivedFake;
            CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
            _fakeListener = false;
        }

        private void SubscribeFake()
        {
            CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;
            CrossNFC.Current.OnMessageReceived += Current_OnMessageReceivedFake;
            _fakeListener = true;
        }

        private async void Current_OnMessageReceived(ITagInfo tagInfo)
        {
            if (_elements == null)
                return;

            await StopListeningAsync();

            if (tagInfo == null)
            {
                await DisplayAlert("Error", "No tag found", "OK");
                return;
            }

            if (!tagInfo.IsSupported || tagInfo.IsEmpty)
            {
                await DisplayAlert("Error", "Unsupported/Empty tag", "OK");
                return;
            }

            _ = FlashColorAsync();

            var message = GetMessage(tagInfo.Records[0]);

            if (message.Contains("adauga_ora_curs"))
                _ora.curs = true;
            else if (message.Contains("adauga_ora_pregatire"))
                _ora.pregatire = true;
            else if (message.Contains("adauga_ora_recuperare"))
                _ora.recuperare = true;

            Analytics.TrackEvent("NFC tag read");

            if (!_busy)
                _ = WaitAndAddAsync();

            await StartListeningAsync().ConfigureAwait(false);
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
                Preferences.Set("totalVechi", valoare[0]);

            PretTotal.Text = valoare[1].ToString(CultureInfo.InvariantCulture);
            PretTotalVechi.Text = (valoare[0] == 0.0 ? Preferences.Get("totalVechi", 0.0) : valoare[0]).ToString(CultureInfo.InvariantCulture);
        }

        private async Task WaitAndAddAsync()
        {
            _busy = true;

            await CountdownAsync();

            await _elements.AddNewEntryAsync(ObsEntry.Text, _ora.curs, _ora.pregatire, _ora.recuperare, null, null);
            await HelperFunctions.ShowToastAsync("New entry added!");
            ObsEntry.Text = string.Empty;
            RefreshPage(true);

            _ora = (false, false, false);

            _busy = false;
        }

        private async Task CountdownAsync()
        {
            UserDialogs.Instance.ShowLoading("Waiting...");
            for (var i = 0; i < 12 && _ora != (true, true, true); i++)
                await Task.Delay(500);
            UserDialogs.Instance.HideLoading();
            await Task.Delay(100).ConfigureAwait(false);
        }

        private async Task FlashColorAsync()
        {
            Indicator.Color = Color.Red;
            await Task.Delay(500);
            Indicator.Color = Color.Green;
        }

        private static void Current_OnMessageReceivedFake(ITagInfo tagInfo)
        {
            // Do nothing;
        }

        private static string GetMessage(NFCNdefRecord record)
        {
            if (record.TypeFormat != NFCNdefTypeFormat.WellKnown ||
                string.IsNullOrWhiteSpace(record.MimeType) && string.CompareOrdinal(record.MimeType, "text/plain") != 0)
                return null;

            return record.Message;
        }
    }
}