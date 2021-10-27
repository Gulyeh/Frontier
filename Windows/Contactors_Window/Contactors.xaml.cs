using Frontier.Database.GetQuery;
using Frontier.Variables;
using Frontier.ViewModels;
using Frontier.Windows.Confirmation_Window;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Windows;
using System.Windows.Controls;

namespace Frontier.Windows.Contactors_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Kontrahenci.xaml
    /// </summary>
    public partial class Contactors : Page
    {
        public ObservableCollection<Contactors_ViewModel> ContactorsData { get; set; } = new ObservableCollection<Contactors_ViewModel>();

        public Contactors()
        {
            InitializeComponent();
            Contactors_Grid.ItemsSource = ContactorsData;
            LoadContactors();
        }

        private void LoadContactors()
        {
            GetContactors Contactors = new GetContactors();
            var query = Contactors.Contactors;
            foreach(var data in query)
            {
                ContactorsData.Add(new Contactors_ViewModel { ID = data.idContactors, Address = data.Street, Country = data.Country, Name = data.Name, State = data.State, NIP = data.NIP, PostCode = data.PostCode, Regon = data.REGON });
            }
        }

        private void Delete_Clicked(object sender, RoutedEventArgs e)
        {
            Confirmation confirm = new Confirmation("Contactors");
            confirm.Owner = Application.Current.MainWindow;
            confirm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            confirm.ShowDialog();
        }
    }
}
