using Frontier.Database.GetQuery;
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
        ObservableCollection<Warehouse_ViewModel> WarehouseData { get; set; } = new ObservableCollection<Warehouse_ViewModel>();

        public Warehouse()
        {
            InitializeComponent();
            Warehouse_Grid.ItemsSource = WarehouseData;
            LoadWarehouse();
        }

        private void LoadWarehouse()
        {
            GetWarehouse warehouse = new GetWarehouse();
            var query = warehouse.Warehouse;
            foreach(var data in query)
            {
                WarehouseData.Add(new Warehouse_ViewModel { ID = data.idWarehouse, Group = data.Group, Name = data.Name, Count = data.Amount, Brutto = data.Brutto, Netto = data.Netto, VAT = data.VAT, Margin = data.Margin });
            }
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
