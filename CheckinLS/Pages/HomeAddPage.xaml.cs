using CheckinLS.API.Misc;
using CheckinLS.API.Standard;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms.Xaml;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualAdd
    {
        private readonly StandardElements _elements;
        private readonly Home _home;

        public ManualAdd(StandardElements elements, in Home home)
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

            if (!CursToggle.IsToggled && !PregatireToggle.IsToggled && !RecuperareToggle.IsToggled)
                return;

            AddButton.IsEnabled = false;

            Analytics.TrackEvent("Manual entry added");
            await _elements.AddNewEntryAsync(ObsManualEntry.Text, CursToggle.IsToggled, PregatireToggle.IsToggled, RecuperareToggle.IsToggled);
            HelperFunctions.ShowToast("New entry added!");
            _home.RefreshPage();
            ObsManualEntry.Text = "";

            AddButton.IsEnabled = true;
        }
    }
}