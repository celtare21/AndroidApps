using System;
using System.ComponentModel;
using CheckinLS.API.Misc;
using CheckinLS.API.Standard;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomeAddPage
    {
        private readonly StandardElements _elements;
        private readonly Home _home;
        private bool _timeSet, _dateSet;

        public HomeAddPage(StandardElements elements, in Home home)
        {
            InitializeComponent();

            _elements = elements;
            _home = home;

            AddButton.Clicked += Add_button_Clicked;
        }

        private async void Add_button_Clicked(object sender, EventArgs e)
        {
            if (_elements == null)
                return;

            AddButton.IsEnabled = false;

            try
            {
                await _elements.AddNewEntryAsync(ObsManualEntry.Text, CursToggle.IsToggled, PregatireToggle.IsToggled,
                    RecuperareToggle.IsToggled, GetCustomTime(), GetCustomDate());
            }
            catch (AllParametersFalse)
            {
                HelperFunctions.ShowToast("Please select at least one item.");
                AddButton.IsEnabled = true;
                return;
            }
            catch (HoursOutOfBounds)
            {
                HelperFunctions.ShowToast("Too many entries in a day!");
                AddButton.IsEnabled = true;
                return;
            }

            Analytics.TrackEvent("Manual entry added");
            HelperFunctions.ShowToast("New entry added!");
            _home.RefreshPage();

            ResetElements();

            AddButton.IsEnabled = true;
        }

        private void OnTimePickerPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName.Equals("Time"))
                _timeSet = true;
        }

        private void OnDateSelected(object sender, DateChangedEventArgs args)
        {
            if (args.NewDate != args.OldDate)
                _dateSet = true;
        }

        private void ResetElements()
        {
            CursToggle.IsToggled = PregatireToggle.IsToggled = RecuperareToggle.IsToggled = false;
            ObsManualEntry.Text = string.Empty;
            OraIncepereTime.Time = TimeSpan.FromHours(8);
            StartDatePicker.Date = DateTime.Today;
            _timeSet = _dateSet = false;
        }

        private TimeSpan? GetCustomTime()
        {
            if (_timeSet)
                return OraIncepereTime.Time;

            return null;
        }

        private DateTime? GetCustomDate()
        {
            if (_dateSet)
                return StartDatePicker.Date;

            return null;
        }
    }
}