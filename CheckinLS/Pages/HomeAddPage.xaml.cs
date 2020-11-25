using CheckinLS.API.Misc;
using CheckinLS.API.Standard;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms.Xaml;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomeAddPage
    {
        private readonly StandardElements _elements;
        private readonly Home _home;

        public HomeAddPage(StandardElements elements, in Home home)
        {
            InitializeComponent();

            _elements = elements;
            _home = home;

            AddButton.Clicked += Add_button_Clicked;
        }

        private async void Add_button_Clicked(object sender, System.EventArgs e)
        {
            if (_elements == null)
                return;

            AddButton.IsEnabled = false;

            try
            {
                await _elements.AddNewEntryAsync(ObsManualEntry.Text, CursToggle.IsToggled, PregatireToggle.IsToggled,
                    RecuperareToggle.IsToggled);
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
            ObsManualEntry.Text = string.Empty;

            AddButton.IsEnabled = true;
        }
    }
}