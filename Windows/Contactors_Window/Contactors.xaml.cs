using Frontier.ViewModels;
using Frontier.Windows.Confirmation_Window;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Frontier.Windows.Contactors_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Kontrahenci.xaml
    /// </summary>
    public partial class Contactors : Page
    {
        ObservableCollection<Contactors_ViewModel> ContactorsData = new ObservableCollection<Contactors_ViewModel>();

        public Contactors()
        {
            InitializeComponent();
            Contactors_Grid.ItemsSource = ContactorsData;
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
