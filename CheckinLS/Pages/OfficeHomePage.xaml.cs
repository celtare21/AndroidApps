using CheckinLS.API.Misc;
using CheckinLS.API.Office;
using CheckinLS.InterfacesAndClasses.Date;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static CheckinLS.API.Misc.TimerHelper;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OfficeHomePage
    {
        private OfficeElements _officeElements;

        public async Task CreateElementsAsync() =>
            _officeElements = await OfficeElements.CreateAsync(new GetDate());

        public OfficeHomePage() =>
            InitializeComponent();

        protected override void OnAppearing()
        {
            base.OnAppearing();

            AddEvents();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            RemoveEvents();
            ButtonTimer.Stop();
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
            if (_officeElements == null || _officeElements.Index <= 0)
                return;

            --_officeElements.Index;
            RefreshPage();
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
            if (_officeElements == null || _officeElements.Index > _officeElements.MaxElement() - 1)
                return;

            ++_officeElements.Index;
            RefreshPage();
        }

        private async void ButtonTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_officeElements == null)
                return;

            _officeElements.Index = LeftRightButton ? _officeElements.MaxElement() : 0;

            try
            {
                HapticFeedback.Perform(HapticFeedbackType.LongPress);
            }
            catch (FeatureNotSupportedException)
            {
                Vibration.Vibrate(100);
            }

            await Device.InvokeOnMainThreadAsync(RefreshPage).ConfigureAwait(false);
        }

        private async void DeleteButton_Clicked(object sender, EventArgs e)
        {
            if (_officeElements == null || _officeElements.MaxElement() < 0)
                return;

            var result = await DisplayAlert("Alert!", "Are you sure you want to delete the entry?", "Yes", "No");

            if (!result)
                return;

            DeleteButton.IsEnabled = false;

            await _officeElements.DeleteEntryAsync(Convert.ToInt32(IdLabel.Text));
            if (_officeElements.Index > _officeElements.MaxElement())
                --_officeElements.Index;
            RefreshPage();

            await HelperFunctions.ShowToastAsync("Entry deleted!");

            DeleteButton.IsEnabled = true;
        }

        private async void AddButton_Clicked(object sender, EventArgs e)
        {
            TimeSpan start = OraIncepereTime.Time;
            TimeSpan finish = OraFinalTime.Time;

            if (_officeElements == null)
                return;

            AddButton.IsEnabled = false;

            try
            {
                await _officeElements.AddNewEntryAsync(start, finish, ObservatiiEntry.Text);
            }
            catch (HoursCantBeEqual)
            {
                await HelperFunctions.ShowToastAsync("Can't have the start time equal to finish time!");
                AddButton.IsEnabled = true;
                return;
            }
            catch (StartCantBeBigger)
            {
                await HelperFunctions.ShowToastAsync("Start time can't be bigger than finish time");
                AddButton.IsEnabled = true;
                return;
            }

            RefreshPage();
            await HelperFunctions.ShowToastAsync("New entry added!");

            OraIncepereTime.Time = TimeSpan.FromHours(8);
            OraFinalTime.Time = TimeSpan.FromHours(8);
            ObservatiiEntry.Text = string.Empty;

            AddButton.IsEnabled = true;
        }

        public void RefreshPage()
        {
            if (_officeElements == null || _officeElements.MaxElement() < 0)
            {
                IdLabel.Text = DateLabel.Text = OraIncepereLabel.Text = OraFinalLabel.Text = ObservatiiLabel.Text = TotalLabel.Text = "Not found!";
                PretTotal.Text = "0";
            }
            else
            {
                (IdLabel.Text, DateLabel.Text, OraIncepereLabel.Text, OraFinalLabel.Text, TotalLabel.Text, ObservatiiLabel.Text) =
                    (HelperFunctions.ConversionWrapper(_officeElements.Entries[_officeElements.Index].Id),
                        HelperFunctions.ConversionWrapper(_officeElements.Entries[_officeElements.Index].Date),
                        HelperFunctions.ConversionWrapper(_officeElements.Entries[_officeElements.Index].OraIncepere),
                        HelperFunctions.ConversionWrapper(_officeElements.Entries[_officeElements.Index].OraFinal),
                        HelperFunctions.ConversionWrapper(_officeElements.Entries[_officeElements.Index].Total),
                        HelperFunctions.ConversionWrapper(_officeElements.Entries[_officeElements.Index].Observatii));

                SetPrice();
            }
        }

        private void SetPrice()
        {
            if (_officeElements == null)
                return;

            var valoare = _officeElements.Entries.Sum(hours => hours.Total.TotalHours) * Constants.PretOffice;

            PretTotal.Text = valoare.ToString("0.##");
        }

        private void AddEvents() =>
            ButtonTimer.Elapsed += ButtonTimer_Elapsed;

        private void RemoveEvents() =>
            ButtonTimer.Elapsed -= ButtonTimer_Elapsed;
    }
}