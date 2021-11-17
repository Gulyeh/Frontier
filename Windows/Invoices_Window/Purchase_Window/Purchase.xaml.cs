using Frontier.Methods;
using Frontier.Variables;
using Frontier.ViewModels;
using Frontier.Windows.Confirmation_Window;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Frontier.Windows.Invoices_Window.Purchase_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Kupno.xaml
    /// </summary>
    public partial class Purchase : Page, INotifyPropertyChanged
    {
        private decimal productsvalue;
        public decimal ProductsValue
        {
            get { return productsvalue; }
            set
            {
                if (productsvalue == value) return;
                productsvalue = value;
                NotifyPropertyChanged("ProductsValue");
            }
        }
        private Dictionary<string, decimal> CurrencyData;

        public Purchase()
        {
            InitializeComponent();
            LoadAddInvoice();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void LoadAddInvoice()
        {
            ProductsValue = 0;
            SellersList.ItemsSource = Collections.ContactorsData;
            Products_Grid.ItemsSource = Collections.ProductsBought;
        }
        private async void DownloadCurrencies_Values(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
            CurrencyData = await Currencies.DownloadNBP();
            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
            if (CurrencyData != null)
            {
                DownloadCurrency_Button.IsEnabled = false;
                CurrencyList.IsEnabled = true;
                MessageBox.Show("Zaktualizowano dane:\nEUR - " + CurrencyData["EUR"] + "\nUSD - " + CurrencyData["USD"] + "\nGBP - " + CurrencyData["GBP"]);
            }
            else
            {
                MessageBox.Show("Nie udało się pobrać kursów");
            }
        }
        private async void ReCalculateValue(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ProductsValue = 0;
                    foreach (ProductsSold_ViewModel data in Products_Grid.Items)
                    {
                        ProductsValue += data.Brutto;
                    }
                }));
            });
        }
        private void Delete_Clicked(object sender, RoutedEventArgs e)
        {
            Confirmation confirm = new Confirmation("Delete");
            confirm.Owner = Application.Current.MainWindow;
            confirm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            bool? data = confirm.ShowDialog();
            if (data.HasValue && data.Value)
            {
                DeleteRows();
            }
        }
        private async void DeleteRows()
        {
            await Task.Run(async () =>
            {
                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    List<int> ids = new List<int>();
                    foreach (ProductsSold_ViewModel data in Products_Grid.SelectedItems)
                    {
                        ids.Add(data.ID);
                    }

                    foreach (int data in ids)
                    {
                        Collections.ProductsBought.Remove(Collections.ProductsBought.FirstOrDefault(x => x.ID == data));
                    }
                }));
            });
        }
    }
}
