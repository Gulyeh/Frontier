using Frontier.Database.GetQuery;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Frontier.Windows.Analyze_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Analiza.xaml
    /// </summary>
    public partial class Analyze : Page, INotifyPropertyChanged
    {
        private ObservableCollection<KeyValuePair<string, int>> InvoiceSold_Data;
        private ObservableCollection<KeyValuePair<string, decimal>> SoldValue_Data;
        private ObservableCollection<KeyValuePair<string, int>> InvoiceBought_Data;
        private ObservableCollection<KeyValuePair<string, decimal>> BoughtValue_Data;
        private List<string> MonthsNames = new List<string>() { "Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec", "Lipiec", "Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień" };
        private int year;
        public int Year
        {
            get { return year; }
            set
            {
                if (year == value) return;
                year = value;
                NotifyPropertyChanged("Year");
            }
        }
        private int _InvoicesSold;
        public int InvoicesSold
        {
            get { return _InvoicesSold; }
            set
            {
                if (_InvoicesSold == value) return;
                _InvoicesSold = value;
                NotifyPropertyChanged("InvoicesSold");
            }
        }
        private decimal _SoldValue;
        public decimal SoldValue
        {
            get { return _SoldValue; }
            set
            {
                if (_SoldValue == value) return;
                _SoldValue = Math.Round(decimal.Parse(value.ToString("F2")), 2);
                NotifyPropertyChanged("SoldValue");
            }
        }
        private int _InvoicesBought;
        public int InvoicesBought
        {
            get { return _InvoicesBought; }
            set
            {
                if (_InvoicesBought == value) return;
                _InvoicesBought = value;
                NotifyPropertyChanged("InvoicesBought");
            }
        }
        private decimal _BoughtValue;
        public decimal BoughtValue
        {
            get { return _BoughtValue; }
            set
            {
                if (_BoughtValue == value) return;
                _BoughtValue = Math.Round(decimal.Parse(value.ToString("F2")), 2);
                NotifyPropertyChanged("BoughtValue");
            }
        }

        public Analyze()
        {
            InitializeComponent();
            LoadData();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private async void LoadData()
        {
            InvoiceSold_Data = new ObservableCollection<KeyValuePair<string, int>>();
            SoldValue_Data = new ObservableCollection<KeyValuePair<string, decimal>>();
            InvoiceBought_Data = new ObservableCollection<KeyValuePair<string, int>>();
            BoughtValue_Data = new ObservableCollection<KeyValuePair<string, decimal>>();
            Year = DateTime.Now.Year;
            Chart1.ItemsSource = InvoiceSold_Data;
            Chart2.ItemsSource = SoldValue_Data;
            Chart3.ItemsSource = InvoiceBought_Data;
            Chart4.ItemsSource = BoughtValue_Data;
            await FillChart();
        }
        private void DownNumber_Clicked(object sender, RoutedEventArgs e)
        {
            using (GetInvoice_Sold soldInvoices = new GetInvoice_Sold())
            {
                if (int.Parse(YearText.Text) - 1 >= soldInvoices.Invoice_Sold.FirstOrDefault().Date_Created.Year)
                {
                    Year = int.Parse(YearText.Text) - 1;
                }
            }
        }
        private void UpNumber_Clicked(object sender, RoutedEventArgs e)
        {
            if (int.Parse(YearText.Text) + 1 <= DateTime.Now.Year)
            {
                Year = int.Parse(YearText.Text) + 1;
            }
        }
        private async void UpdateData_Clicked(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
            await FillChart();
            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
        }
        private void ResetValues()
        {
            InvoiceSold_Data.Clear();
            InvoiceBought_Data.Clear();
            SoldValue_Data.Clear();
            BoughtValue_Data.Clear();
            SoldValue = 0;
            BoughtValue = 0;
            InvoicesSold = 0;
            InvoicesBought = 0;
        }
        private async Task FillChart()
        {
            await Task.Run(async () =>
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        using (GetInvoice_Sold soldInvoices = new GetInvoice_Sold())
                        {
                            using (GetInvoice_Bought boughtInvoices = new GetInvoice_Bought())
                            {
                                using (GetInvoice_Products products = new GetInvoice_Products())
                                {
                                    ResetValues();

                                    var sold = soldInvoices.Invoice_Sold.Where(x => x.Date_Created.Year == Year);
                                    var bought = boughtInvoices.Invoice_Bought.Where(x => x.Date_Created.Year == Year);
                                    InvoicesSold = sold.Count();
                                    InvoicesBought = bought.Count();

                                    for (int i = 1; i < 13; i++)
                                    {
                                        decimal tempSold = 0;
                                        decimal tempBought = 0;
                                        var soldMonth = sold.Where(x => x.Date_Created.Month == i);
                                        var boughtMonth = bought.Where(x => x.Date_Created.Month == i);
                                        InvoiceSold_Data.Add(new KeyValuePair<string, int>(MonthsNames[i - 1], soldMonth.Count()));
                                        InvoiceBought_Data.Add(new KeyValuePair<string, int>(MonthsNames[i - 1], boughtMonth.Count()));

                                        foreach (var invoice in soldMonth)
                                        {
                                            var invoiceProduct = products.Invoice_Products.Where(x => x.Invoice_ID == invoice.Invoice_ID);
                                            foreach (var product in invoiceProduct)
                                            {
                                                SoldValue += invoice.Currency == "PLN" ? decimal.Parse(product.Brutto) : decimal.Parse(product.Brutto) * decimal.Parse(invoice.ExchangeRate);
                                                tempSold += invoice.Currency == "PLN" ? decimal.Parse(product.Brutto) : Math.Round(decimal.Parse((decimal.Parse(product.Brutto) * decimal.Parse(invoice.ExchangeRate)).ToString("F2")), 2);
                                            }
                                        }

                                        foreach (var invoice in boughtMonth)
                                        {
                                            var invoiceProduct = products.Invoice_Products.Where(x => x.Invoice_ID == invoice.Invoice_ID);
                                            foreach (var product in invoiceProduct)
                                            {
                                                BoughtValue += invoice.Currency == "PLN" ? decimal.Parse(product.Brutto) : decimal.Parse(product.Brutto) * invoice.ExchangeRate;
                                                tempBought += invoice.Currency == "PLN" ? decimal.Parse(product.Brutto) : Math.Round(decimal.Parse((decimal.Parse(product.Brutto) * invoice.ExchangeRate).ToString("F2")), 2);
                                            }
                                        }
                                        BoughtValue_Data.Add(new KeyValuePair<string, decimal>(MonthsNames[i - 1], tempBought));
                                        SoldValue_Data.Add(new KeyValuePair<string, decimal>(MonthsNames[i - 1], tempSold));
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        ResetValues();
                        ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                        MessageBox.Show("Wystąpił błąd podczas pobierania danych");
                    }
                }));
            });
        }
    }
}
