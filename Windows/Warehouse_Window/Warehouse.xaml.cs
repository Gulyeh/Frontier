using Frontier.ViewModels;
using Frontier.Windows.Confirmation_Window;
using Frontier.Windows.Groups_Window;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Frontier.Windows.Warehouse_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Magazyn.xaml
    /// </summary>
    public partial class Warehouse : Page
    {
        ObservableCollection<Warehouse_ViewModel> WarehouseData = new ObservableCollection<Warehouse_ViewModel>();

        public Warehouse()
        {
            InitializeComponent();
            Warehouse_Grid.ItemsSource = WarehouseData;
        }

        private void Delete_Clicked(object sender, RoutedEventArgs e)
        {
            Confirmation confirm = new Confirmation("Warehouse");
            confirm.Owner = Application.Current.MainWindow;
            confirm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            confirm.ShowDialog();
        }

        private void Groups_Clicked(object sender, RoutedEventArgs e)
        {
            Groups groups = new Groups();
            groups.Owner = Application.Current.MainWindow;
            groups.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            groups.ShowDialog();
        }
    }
}
