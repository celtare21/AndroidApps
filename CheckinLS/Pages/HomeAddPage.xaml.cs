using System;
using System.ComponentModel;
using System.Threading.Tasks;
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
        }

        private async void AddButton_Clicked(object sender, EventArgs e)
        {
            await AddNewEntry(AddButton, new EntryInfo(ObsManualEntry.Text, CursToggle.IsToggled,
                PregatireToggle.IsToggled,
                RecuperareToggle.IsToggled));
            ResetElements();
        }

        private async void TabaraButton_OnClickedButton_Clicked(object sender, EventArgs e)
        {
            await AddNewEntry(TabaraButton, new EntryInfo("TABARA", true, true, true));
            await AddNewEntry(TabaraButton, new EntryInfo("TABARA", true, false, false));
            ResetElements();
        }

        private async Task AddNewEntry(VisualElement button, EntryInfo info)
        {
            if (_elements == null)
                return;

            button.IsEnabled = false;

            try
            {
                await _elements.AddNewEntryAsync(info.Obs, info.Curs, info.Pregatire,
                    info.Recuperare, GetCustomTime(), GetCustomDate());
            }
            catch (AllParametersFalse)
            {
                await HelperFunctions.ShowToastAsync("Please select at least one item.");
                AddButton.IsEnabled = true;
                return;
            }
            catch (Exception ex) when (ex is HoursOutOfBounds || ex is OverflowException)
            {
                await HelperFunctions.ShowToastAsync("Too many entries in a day!");
                AddButton.IsEnabled = true;
                return;
            }

            Analytics.TrackEvent("Entry added");
            await HelperFunctions.ShowToastAsync("New entry added!");
            _home.RefreshPage(true);

            button.IsEnabled = true;
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