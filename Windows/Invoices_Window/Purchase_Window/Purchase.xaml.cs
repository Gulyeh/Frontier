using Frontier.Database.GetQuery;
using Frontier.Database.TableClasses;
using Frontier.Methods;
using Frontier.Variables;
using Frontier.ViewModels;
using Frontier.Windows.Confirmation_Window;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
            CurrencyData = await CurrenciesRate.DownloadNBP();
            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
            if (CurrencyData != null)
            {
                DownloadCurrency_Button.IsEnabled = false;
                if (Products_Grid.Items.Count == 0)
                {
                    CurrencyList.IsEnabled = true;
                }
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
                    if (Products_Grid.Items.Count > 0 && CurrencyData != null)
                    {
                        CurrencyList.IsEnabled = false;
                    }
                    else if (Products_Grid.Items.Count == 0 && CurrencyData != null)
                    {
                        CurrencyList.IsEnabled = true;
                    }

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
            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
            await Task.Run(async () =>
            {
                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    List <decimal[]> ids = new List<decimal[]>();
                    foreach (ProductsSold_ViewModel data in Products_Grid.SelectedItems)
                    {
                        ids.Add(new decimal[] { data.ID, data.PieceNetto, data.PieceBrutto });
                    }

                    foreach (decimal[] data in ids)
                    {
                        Collections.ProductsBought.Remove(Collections.ProductsBought.FirstOrDefault(x => x.ID == (int)data[0] && x.PieceBrutto == data[2] && x.PieceNetto == data[1]));
                    }
                }));
            });
            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
        }
        private void AddProduct_Clicked(object sender, RoutedEventArgs e)
        {
            AddBought window = new AddBought();
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }
        private void ChangeDate(object sender, SelectionChangedEventArgs e)
        {
            var senderName = ((DatePicker)sender).Name;
            if (senderName == "Buy_Date" && InvoiceDate.Text != string.Empty)
            {
                if (Convert.ToDateTime(InvoiceDate.Text) < Buy_Date.SelectedDate.Value)
                {
                    MessageBox.Show("Data wystawienia faktury nie może być wcześniejsza od daty kupna");
                    return;
                }
            }
            else if (senderName == "Created_Date" && BoughtDate.Text != string.Empty)
            {
                if (Convert.ToDateTime(BoughtDate.Text) > Created_Date.SelectedDate.Value)
                {
                    MessageBox.Show("Data kupna nie może być późniejsza od daty wystawienia faktury");
                    return;
                }
            }
            _ = senderName == "Buy_Date" ? BoughtDate.Text = Buy_Date.SelectedDate.Value.ToShortDateString() : InvoiceDate.Text = Created_Date.SelectedDate.Value.ToShortDateString();
        }
        private void SaveInvoice_Clicked(object sender, RoutedEventArgs e)
        {
            Confirmation confirm = new Confirmation("Save");
            confirm.Owner = Application.Current.MainWindow;
            confirm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            bool? data = confirm.ShowDialog();
            if (data.HasValue && data.Value)
            {
                SaveInvoice();
            }
        }
        private async void SaveInvoice()
        {
            if (SellersList.SelectedIndex > -1 && InvoiceNumber.Text != string.Empty && BoughtDate.Text != string.Empty && InvoiceDate.Text != string.Empty && Products_Grid.Items.Count > 0)
            {
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                await Task.Run(async () =>
                {
                    await Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        using (GetInvoice_Bought invoice = new GetInvoice_Bought())
                        {
                            using (GetWarehouse product = new GetWarehouse())
                            {
                                using (GetInvoice_Products new_item = new GetInvoice_Products())
                                {
                                    int LastInvoice = 0;
                                    try
                                    {
                                        DateTimeFormatInfo format = new DateTimeFormatInfo();
                                        format.ShortDatePattern = "dd.MM.yyyy";

                                        var data = new Invoice_Bought
                                        {
                                            Invoice_ID = InvoiceNumber.Text,
                                            Seller_ID = Collections.ContactorsData[SellersList.SelectedIndex].ID,
                                            Purchase_type = PurchaseType.Text,
                                            Date_Created = Convert.ToDateTime(InvoiceDate.Text, format),
                                            Date_Bought = BoughtDate.Text,
                                            Currency = CurrencyList.Text,
                                            ExchangeRate = CurrencyList.Text != "PLN" ? CurrencyData[CurrencyList.Text] : 0
                                        };
                                        var updated = await invoice.AddInvoice(data);
                                        if (updated)
                                        {
                                            await invoice.SaveChangesAsync();
                                            LastInvoice = invoice.Invoice_Bought.OrderByDescending(x => x.idinvoice_bought).FirstOrDefault() != null ? invoice.Invoice_Bought.OrderByDescending(x => x.idinvoice_bought).FirstOrDefault().idinvoice_bought : 1;
                                            foreach (ProductsSold_ViewModel item in Products_Grid.Items)
                                            {
                                                var exists = product.Warehouse.FirstOrDefault(x => x.Name == item.Name && x.Netto == item.PieceNetto && x.Brutto == item.PieceBrutto);
                                                if (exists != null)
                                                {
                                                    exists.Amount += item.Amount;
                                                }
                                                else
                                                {
                                                    var findDefault = Collections.WarehouseData.FirstOrDefault(x => x.Name == item.Name);
                                                    exists = product.Warehouse.FirstOrDefault(x => x.Name == item.Name && x.Amount == 0);
                                                    if (exists != null)
                                                    {
                                                        exists.Amount = item.Amount;
                                                        exists.Brutto = item.Brutto;
                                                        exists.Netto = item.Netto;
                                                        exists.VAT = item.VAT;
                                                    }
                                                    else
                                                    {
                                                        var new_warehouseitem = new Warehouse
                                                        {
                                                            Name = item.Name,
                                                            Group = findDefault.GroupID.ToString(),
                                                            Amount = item.Amount,
                                                            Brutto = CurrencyList.Text != "PLN" ? item.PieceBrutto * CurrencyData[CurrencyList.Text] : item.PieceBrutto,
                                                            Netto = CurrencyList.Text != "PLN" ? item.PieceNetto * CurrencyData[CurrencyList.Text] : item.PieceNetto,
                                                            VAT = item.VAT,
                                                            Margin = findDefault.Margin
                                                        };
                                                        updated = await product.AddItem(new_warehouseitem);
                                                        if (!updated) { throw new ArgumentNullException(); }
                                                    }
                                                }

                                                var new_product = new Invoice_Products
                                                {
                                                    Invoice = LastInvoice,
                                                    Invoice_ID = data.Invoice_ID,
                                                    Product_ID = string.Empty,
                                                    Name = item.Name,
                                                    Amount = item.Amount.ToString(),
                                                    Each_Netto = item.PieceNetto.ToString(),
                                                    VAT = item.VAT,
                                                    Each_Brutto = item.PieceBrutto.ToString(),
                                                    Netto = item.Netto.ToString(),
                                                    VAT_Price = item.VATAmount.ToString(),
                                                    Brutto = item.Brutto.ToString(),
                                                    GTU = item.GTU,
                                                    GroupType = item.GroupType,
                                                    BoughtNetto = "0",
                                                    BoughtBrutto = "0",
                                                    BoughtVAT = "0",
                                                };
                                                updated = await new_item.AddProduct(new_product);
                                                if (!updated) { throw new ArgumentNullException(); }
                                            }
                                            await new_item.SaveChangesAsync();
                                            await product.SaveChangesAsync();
                                            foreach (ProductsSold_ViewModel item in Products_Grid.Items)
                                            {
                                                var LastID = product.Warehouse.FirstOrDefault(x => x.Name == item.Name && x.Netto == item.PieceNetto && x.Brutto == item.PieceBrutto && x.VAT == item.VAT).idWarehouse;
                                                new_item.Invoice_Products.FirstOrDefault(x => x.Invoice == LastInvoice && x.Invoice_ID == data.Invoice_ID && x.Name == item.Name && x.Each_Netto == item.PieceNetto.ToString() && x.Each_Brutto == item.PieceBrutto.ToString() && x.VAT == item.VAT).Product_ID = LastID.ToString();
                                            }
                                            await new_item.SaveChangesAsync();
                                            await Download_WarehouseItems.Download();
                                            ResetValues();
                                            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                                            MessageBox.Show("Pomyślnie dodano fakturę");
                                        }
                                        else
                                        {
                                            throw new ArgumentNullException();
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        var items = new_item.Invoice_Products.Where(x => x.Invoice_ID == InvoiceNumber.Text && x.Invoice == LastInvoice);
                                        foreach(var item in items) { new_item.DeleteItem(item.id); }
                                        invoice.Invoice_Bought.Remove(invoice.Invoice_Bought.FirstOrDefault(x => x.idinvoice_bought == LastInvoice));
                                        await invoice.SaveChangesAsync();
                                        await new_item.SaveChangesAsync();
                                        ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                                        MessageBox.Show("Wystąpił błąd podczas zapisu");
                                    }

                                }
                            }
                        }
                    }));
                });
            }
            else
            {
                MessageBox.Show("Proszę uzupełnić wymagane dane");
            }
        }
        private void ResetValues()
        {
            SellersList.SelectedIndex = -1;
            InvoiceNumber.Text = string.Empty;
            BoughtDate.Text = string.Empty;
            InvoiceDate.Text = string.Empty;
            Collections.ProductsBought.Clear();
        }
    }
}
