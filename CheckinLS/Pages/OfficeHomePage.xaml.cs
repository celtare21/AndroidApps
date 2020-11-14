using CheckinLS.API.Misc;
using CheckinLS.API.Office;
using CheckinLS.InterfacesAndClasses.Date;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;
using MainSql = CheckinLS.API.Sql.MainSql;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OreOffice
    {
        private OfficeElements _officeElements;

        public async Task CreateElementsAsync(MainSql sqlClass)
        {
            _officeElements = await OfficeElements.CreateAsync(sqlClass, new GetDate());
        }

        public OreOffice()
        {
            InitializeComponent();

            LeftButton.Clicked += LeftButton_Clicked;
            RightButton.Clicked += RightButton_Clicked;

            DeleteButton.Clicked += DeleteButton_Clicked;
            AddButton.Clicked += AddButton_Clicked;
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

            if (start == finish)
            {
                HelperFunctions.ShowToast("Can't have the start time equal to finish time!");
                return;
            }

            if (start > finish)
            {
                HelperFunctions.ShowToast("Start time can't be bigger than finish time");
                return;
            }

            AddButton.IsEnabled = false;

            await _officeElements.AddNewEntryAsync(start, finish);
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

            var total = 0.0;

            foreach (var elem in _officeElements.Entries)
            {
                total += elem.Total.TotalHours;
            }

            var valoare = total * Constants.PretOffice;

            PretTotal.Text = valoare.ToString("0.##");
        }
    }
}