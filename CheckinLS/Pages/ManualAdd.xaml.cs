using Xamarin.Forms.Xaml;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualAdd
    {
        public ManualAdd()
        {
            InitializeComponent();

            AddButton.Clicked += Add_button_Clicked;
        }

        private async void Add_button_Clicked(object sender, System.EventArgs e)
        {
            if (!CursToggle.IsToggled && !PregatireToggle.IsToggled && !RecuperareToggle.IsToggled)
                return;

            AddButton.IsEnabled = false;

            await Home.AddNewEntryExternalAsync(ObsManualEntry.Text, CursToggle.IsToggled, PregatireToggle.IsToggled, RecuperareToggle.IsToggled);

            ObsManualEntry.Text = "";

            AddButton.IsEnabled = true;
        }
    }
}