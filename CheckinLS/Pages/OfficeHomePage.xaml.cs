using CheckinLS.API.Misc;
using CheckinLS.API.Office;
using CheckinLS.InterfacesAndClasses.Date;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OfficeHomePage
    {
        private OfficeElements _officeElements;

        public async Task CreateElementsAsync()
        {
            _officeElements = await OfficeElements.CreateAsync(new GetDate());
        }

        public OfficeHomePage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            AddEvents();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            RemoveEvents();
        }

        private void LeftButton_Clicked(object sender, EventArgs e)
        {
            if (_officeElements == null || _officeElements.Index <= 0)
                return;

            --_officeElements.Index;
            RefreshPage();
        }

        private void RightButton_Clicked(object sender, EventArgs e)
        {
            if (_officeElements == null || _officeElements.Index >= _officeElements.MaxElement() - 1)
                return;

            ++_officeElements.Index;
            RefreshPage();
        }

        private async void DeleteButton_Clicked(object sender, EventArgs e)
        {
            if (_officeElements == null || _officeElements.MaxElement() == 0 || !IdLabel.Text.All(char.IsDigit))
                return;

            var result = await DisplayAlert("Alert!", "Are you sure you want to delete the entry?", "Yes", "No");

            if (!result)
                return;

            DeleteButton.IsEnabled = false;

            await _officeElements.DeleteEntryAsync(Convert.ToInt32(IdLabel.Text));
            if (_officeElements.Index > 0 && _officeElements.Index > _officeElements.MaxElement() - 1)
                --_officeElements.Index;
            RefreshPage();

            HelperFunctions.ShowToast("Entry deleted!");

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
                await _officeElements.AddNewEntryAsync(start, finish);
            }
            catch (HoursCantBeEqual)
            {
                HelperFunctions.ShowToast("Can't have the start time equal to finish time!");
                AddButton.IsEnabled = true;
                return;
            }
            catch (StartCantBeBigger)
            {
                HelperFunctions.ShowToast("Start time can't be bigger than finish time");
                AddButton.IsEnabled = true;
                return;
            }

            RefreshPage();
            HelperFunctions.ShowToast("New entry added!");

            AddButton.IsEnabled = true;
        }

        public void RefreshPage()
        {
            if (_officeElements == null || _officeElements.MaxElement() == 0)
            {
                IdLabel.Text = DateLabel.Text = OraIncepereLabel.Text = OraFinalLabel.Text = TotalLabel.Text = "Not found!";
                PretTotal.Text = "0";
                return;
            }

            (IdLabel.Text, DateLabel.Text, OraIncepereLabel.Text, OraFinalLabel.Text, TotalLabel.Text) =
                (HelperFunctions.ConversionWrapper(_officeElements.Entries[_officeElements.Index].Id),
                    HelperFunctions.ConversionWrapper(_officeElements.Entries[_officeElements.Index].Date),
                    HelperFunctions.ConversionWrapper(_officeElements.Entries[_officeElements.Index].OraIncepere),
                    HelperFunctions.ConversionWrapper(_officeElements.Entries[_officeElements.Index].OraFinal),
                    HelperFunctions.ConversionWrapper(_officeElements.Entries[_officeElements.Index].Total));

            SetPrice();
        }

        private void SetPrice()
        {
            if (_officeElements == null)
                return;

            var valoare = _officeElements.Entries.Sum(hours => hours.Total.TotalHours) * Constants.PretOffice;

            PretTotal.Text = valoare.ToString("0.##");
        }

        private void AddEvents()
        {
            LeftButton.Clicked += LeftButton_Clicked;
            RightButton.Clicked += RightButton_Clicked;

            DeleteButton.Clicked += DeleteButton_Clicked;
            AddButton.Clicked += AddButton_Clicked;
        }

        private void RemoveEvents()
        {
            LeftButton.Clicked -= LeftButton_Clicked;
            RightButton.Clicked -= RightButton_Clicked;

            DeleteButton.Clicked -= DeleteButton_Clicked;
            AddButton.Clicked -= AddButton_Clicked;
        }
    }
}