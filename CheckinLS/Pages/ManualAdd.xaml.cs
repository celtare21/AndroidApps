using CheckinLS.API;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms.Xaml;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualAdd
    {
        private readonly Elements _elements;
        private readonly Home _home;

        public ManualAdd(Elements elements, Home home)
        {
            InitializeComponent();

            _elements = elements;
            _home = home;

            AddButton.Clicked += Add_button_Clicked;
        }

        private async void Add_button_Clicked(object sender, System.EventArgs e)
        {
            if (!CursToggle.IsToggled && !PregatireToggle.IsToggled && !RecuperareToggle.IsToggled)
                return;

            AddButton.IsEnabled = false;

            Analytics.TrackEvent("Manual entry added");

            await _elements.AddNewEntryAsync(ObsManualEntry.Text, CursToggle.IsToggled, PregatireToggle.IsToggled, RecuperareToggle.IsToggled);

            _home.ShowToast("New entry added!");

            _home.RefreshPage();

            ObsManualEntry.Text = "";

            AddButton.IsEnabled = true;
        }
    }
}