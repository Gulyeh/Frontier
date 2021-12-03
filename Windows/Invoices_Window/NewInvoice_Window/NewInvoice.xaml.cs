using Frontier.Classes;
using Frontier.Database.GetQuery;
using Frontier.Database.TableClasses;
using Frontier.Methods;
using Frontier.Methods.Invoices;
using Frontier.Methods.Numerics;
using Frontier.Variables;
using Frontier.ViewModels;
using Frontier.Windows.Confirmation_Window;
using Frontier.Windows.Invoices_Window.AddProduct_Window;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Frontier.Windows.Invoices_Window.NewInvoice_Window
{
    /// <summary>
    /// Logika interakcji dla klasy NowFaktura.xaml
    /// </summary>
    public partial class NewInvoice : Page, INotifyPropertyChanged
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
        private Dictionary<string, decimal> CurrencyData { get; set; }
        InvoiceData _InvoiceData { get; set; }
        private bool SavedInvoice = false;

        public NewInvoice()
        {
            InitializeComponent();
            LoadInvoice();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void LoadInvoice()
        {
            ContactorsList.ItemsSource = Collections.ContactorsData;
            Products_Grid.ItemsSource = Collections.ProductsSold;
            ProductsValue = 0.00m;
            GetLastInvoice();
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

                    if (decimal.Parse(Proform_Paid.Text) > ProductsValue) { Proform_Paid.Text = ProductsValue.ToString(); }
                }));
            });
        }
        private void GetLastInvoice()
        {
            try
            {
                using (GetInvoice_Sold getLastID = new GetInvoice_Sold())
                {
                    var year = DateTime.Now.Year;
                    var Invoices = getLastID.Invoice_Sold.Where(x => x.Date_Created.Year == year).OrderByDescending(x => x.idInvoice_Sold).FirstOrDefault() != null ? getLastID.Invoice_Sold.Where(x => x.Date_Created.Year == year).OrderByDescending(x => x.idInvoice_Sold).Count() : 0;
                    string newInvoiceID = (Invoices + 1).ToString("D6");
                    InvoiceNumber.Text = "F/" + newInvoiceID + "/" + DateTime.Now.Year.ToString();
                }
            }catch (Exception) { GetLastInvoice(); }
        }
        private void CheckNumeric(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex_Check.CheckNumbers(e.Text);
        }
        private void PurchaseType_Selection(object sender, RoutedEventArgs e)
        {
            switch (PurchaseType.SelectedIndex)
            {
                case 1:
                    Bankname.IsEnabled = true;
                    Accountnumber.IsEnabled = true;
                    break;
                default:
                    Bankname.IsEnabled = false;
                    Accountnumber.IsEnabled = false;
                    Bankname.Text = string.Empty;
                    Accountnumber.Text = string.Empty;
                    break;
            }
        }
        private async void Change_InvoiceType(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                await Dispatcher.BeginInvoke(new Action(async () => 
                {
                    if (Proform_Text != null && Proform_Value != null)
                    {
                        await RestoreProducts();
                        Collections.ProductsSold.Clear();
                        Proform_Paid.Text = "0";
                        if (InvoiceType.SelectedIndex == 0 || InvoiceType.SelectedIndex == 2)
                        {
                            Sell_Date.IsHitTestVisible = true;
                        }
                        else
                        {
                            SellDate_Text.Text = string.Empty;
                            Sell_Date.IsHitTestVisible = false;
                        }
                    }
                }));
            });
        }
        private void SelectionDate_Clicked(object sender, SelectionChangedEventArgs e)
        {
            var senderName = ((DatePicker)sender).Name;
            if (senderName == "Created_Date" && SellDate_Text.Text != string.Empty)
            {
                if (Convert.ToDateTime(SellDate_Text.Text) > Created_Date.SelectedDate.Value.Date)
                {
                    MessageBox.Show("Data wystawienia faktury nie może być wcześniejsza od daty sprzedaży");
                    return;
                }
            }
            else if (senderName == "Sell_Date" && CreatedDate_Text.Text != string.Empty)
            {
                if (Convert.ToDateTime(CreatedDate_Text.Text) < Sell_Date.SelectedDate.Value.Date)
                {
                    MessageBox.Show("Data sprzedaży nie może być późniejsza od daty wystawienia faktury");
                    return;
                }
            }
            _ = senderName == "Sell_Date" ? SellDate_Text.Text = Sell_Date.SelectedDate.Value.Date.ToShortDateString() : CreatedDate_Text.Text = Created_Date.SelectedDate.Value.Date.ToShortDateString();
        }
        private void SaveInvoice_Clicked(object sender, RoutedEventArgs e)
        {
            Confirmation confirm = new Confirmation("Save");
            confirm.Owner = Application.Current.MainWindow;
            confirm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            bool? data = confirm.ShowDialog();
            if (data.HasValue && data.Value)
            {
                if (SavedInvoice)
                {
                    MessageBox.Show("Faktura została już zapisana");
                }
                else
                {
                    SaveInvoice();
                }
            }
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
        private void AddProduct_Clicked(object sender, RoutedEventArgs e)
        {
            decimal exchangerate = 0;
            if (CurrencyData != null)
            {
                exchangerate = CurrencyData[CurrencyList.Text];
            }

            AddProduct products = new AddProduct(InvoiceType.Text, CurrencyList.Text, exchangerate, "NewInvoice");
            products.Owner = Application.Current.MainWindow;
            products.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            products.ShowDialog();
            ReCalculateValue(null, null);
        }
        private async void DeleteRows()
        {
            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
            await Task.Run(async () =>
            {
                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    List<decimal[]> ids = new List<decimal[]>();
                    foreach (ProductsSold_ViewModel data in Products_Grid.SelectedItems)
                    {
                        ids.Add(new decimal[] { data.ID, data.Amount, data.PieceBrutto, data.PieceNetto });
                    }

                    foreach (decimal[] data in ids)
                    {
                        if (Collections.WarehouseData.FirstOrDefault(x => x.ID == (int)data[0]).GroupType != "Usługa")
                        {
                            Collections.WarehouseData.FirstOrDefault(x => x.ID == (int)data[0]).Amount += (int)data[1];
                        }
                        Collections.ProductsSold.Remove(Collections.ProductsSold.Where(x => x.ID == (int)data[0] && x.PieceNetto == data[3] && x.PieceBrutto == data[2]).FirstOrDefault());
                    }
                }));
            });
            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
        }
        private async void SaveInvoice()
        {
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                await Task.Run(async () =>
                {
                    await Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        try
                        {
                            if (ContactorsList.SelectedIndex > -1 && InvoiceNumber.Text != string.Empty && (InvoiceType.SelectedIndex == 1 ? DaysAmount.Text != string.Empty : SellDate_Text.Text != string.Empty) && PurchaseType.SelectedIndex > -1 && CreatedDate_Text.Text != string.Empty && Collections.ProductsSold.Count() > 0)
                            {
                                SavedInvoice = true;
                                _InvoiceData = new InvoiceData
                                {
                                    InvoiceType = InvoiceType.Text,
                                    InvoiceNumber = InvoiceNumber.Text,
                                    BuyerID = Collections.ContactorsData[ContactorsList.SelectedIndex].ID,
                                    Payment = PurchaseType.Text,
                                    SellDate = SellDate_Text.Text,
                                    InvoiceDate = CreatedDate_Text.Text,
                                    Description = Description.Text,
                                    PaymentDays = DaysAmount.Text,
                                    BankName = Bankname.Text,
                                    AccountNumber = Accountnumber.Text,
                                    TotalPrice = String.Format("{0:0.00}", ProductsValue),
                                    PaidPrice = String.Format("{0:0.00}", decimal.Parse(Proform_Paid.Text)),
                                    Currency = CurrencyList.Text
                                };
                                using (GetInvoice_Sold invoice = new GetInvoice_Sold())
                                {
                                    DateTimeFormatInfo format = new DateTimeFormatInfo();
                                    format.ShortDatePattern = "dd.MM.yyyy";

                                    var item = new Invoice_Sold
                                    {
                                        Receiver = Collections.ContactorsData[ContactorsList.SelectedIndex].ID.ToString(),
                                        Invoice_ID = InvoiceNumber.Text,
                                        Invoice_Type = InvoiceType.Text,
                                        Date_Sold = SellDate_Text.Text,
                                        Date_Created = Convert.ToDateTime(CreatedDate_Text.Text, format),
                                        Purchase_type = PurchaseType.Text,
                                        Day_Limit = DaysAmount.Text,
                                        Currency = CurrencyList.Text,
                                        PricePaid = String.Format("{0:0.00}", decimal.Parse(Proform_Paid.Text)),
                                        Description = Description.Text,
                                        AccountNumber = Accountnumber.Text,
                                        BankName = Bankname.Text,
                                        ExchangeRate = "0"
                                    };

                                    if (CurrencyList.Text != "PLN")
                                    {
                                        item.ExchangeRate = CurrencyData[CurrencyList.Text].ToString();
                                    }

                                    var updated = await invoice.AddInvoice(item);
                                    if (updated)
                                    {
                                        using (GetInvoice_Products query = new GetInvoice_Products())
                                        {
                                            using (GetWarehouse sold_item = new GetWarehouse())
                                            {
                                                int LastInvoice = invoice.Invoice_Sold.OrderByDescending(x => x.idInvoice_Sold).FirstOrDefault() != null ? invoice.Invoice_Sold.OrderByDescending(x => x.idInvoice_Sold).FirstOrDefault().idInvoice_Sold + 1 : 1;
                                                foreach (ProductsSold_ViewModel product in Products_Grid.Items)
                                                {
                                                    var new_product = new Invoice_Products
                                                    {
                                                        Invoice_ID = InvoiceNumber.Text,
                                                        Invoice = LastInvoice,
                                                        Product_ID = product.ID.ToString(),
                                                        Name = product.Name,
                                                        Amount = product.Amount.ToString(),
                                                        Each_Netto = product.PieceNetto.ToString(),
                                                        VAT = product.VAT,
                                                        Each_Brutto = product.PieceBrutto.ToString(),
                                                        Netto = product.Netto.ToString(),
                                                        VAT_Price = product.VATAmount.ToString(),
                                                        Brutto = product.Brutto.ToString(),
                                                        GroupType = product.GroupType,
                                                        GTU = Collections.GroupsData.FirstOrDefault(x => x.ID == Collections.WarehouseData.FirstOrDefault(Z => Z.ID == product.ID).GroupID).GTU,
                                                        BoughtNetto = product.GroupType != "Usługa" ? Collections.WarehouseData.FirstOrDefault(x => x.ID == product.ID).Netto.ToString() : "0",
                                                        BoughtVAT = product.GroupType != "Usługa" ? Collections.WarehouseData.FirstOrDefault(x => x.ID == product.ID).VAT.ToString() : "0",
                                                        BoughtBrutto = product.GroupType != "Usługa" ? Collections.WarehouseData.FirstOrDefault(x => x.ID == product.ID).Brutto.ToString() : "0"
                                                    };
                                                    await query.AddProduct(new_product);

                                                    if (product.GroupType != "Usługa")
                                                    {
                                                        var solditem_warehouse = new Warehouse
                                                        {
                                                            idWarehouse = product.ID,
                                                            Name = product.Name,
                                                            Amount = product.Amount,
                                                        };
                                                        await sold_item.SoldWarehouse_Item(solditem_warehouse);
                                                    }
                                                }
                                                await sold_item.SaveChangesAsync();
                                                await query.SaveChangesAsync();
                                                await invoice.SaveChangesAsync();
                                            }
                                        }
                                        ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                                        MessageBox.Show("Pomyślnie zapisano fakturę");
                                    }
                                    else
                                    {
                                        throw new ArgumentNullException();
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Proszę wypełnić wymagane dane");
                            }
                        }
                        catch (Exception)
                        {
                            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                            MessageBox.Show("Wystąpił błąd podczas zapisu faktury");
                        }
                    }));
                });
        }
        private async void NewInvoice_Clicked(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                await Dispatcher.BeginInvoke(new Action(async () =>
                {
                    ContactorsList.SelectedIndex = -1;
                    PurchaseType.SelectedIndex = -1;
                    CurrencyList.SelectedIndex = 0;
                    InvoiceType.SelectedIndex = 0;
                    InvoiceNumber.Text = string.Empty;
                    SellDate_Text.Text = string.Empty;
                    CreatedDate_Text.Text = string.Empty;
                    Description.Text = string.Empty;
                    DaysAmount.Text = string.Empty;
                    if (!SavedInvoice)
                    {
                        await RestoreProducts();
                    }
                    Collections.ProductsSold.Clear();
                    GetLastInvoice();
                }));
            });
        }
        private Task RestoreProducts()
        {
            List<int[]> ids = new List<int[]>();
            foreach (ProductsSold_ViewModel data in Products_Grid.Items)
            {
                ids.Add(new int[] { data.ID, data.Amount });
            }

            foreach (int[] data in ids)
            {
                if (Collections.WarehouseData.FirstOrDefault(x => x.ID == data[0]).GroupType != "Usługa")
                {
                    Collections.WarehouseData.FirstOrDefault(x => x.ID == data[0]).Amount += data[1];
                }
            }
            return Task.CompletedTask;
        }
        private async void SavePDF_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!SavedInvoice)
                {
                    MessageBox.Show("Proszę zapisać najpierw zapisać fakturę");
                    return;
                }
                await Save_Invoice.SavePDF(_InvoiceData);
            }
            catch (Exception)
            {
                MessageBox.Show("Błąd podczas zapisu");
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
            }
        }
        private async void SaveWord_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!SavedInvoice)
                {
                    MessageBox.Show("Proszę zapisać najpierw zapisać fakturę");
                    return;
                }
                await Save_Invoice.SaveWord(_InvoiceData);
            }
            catch (Exception)
            {
                MessageBox.Show("Błąd podczas zapisu");
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
            }
        }
        private async void Print_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!SavedInvoice)
                {
                    MessageBox.Show("Proszę zapisać najpierw zapisać fakturę");
                    return;
                }
                await Save_Invoice.Print(_InvoiceData);
            }
            catch (Exception)
            {
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                MessageBox.Show("Błąd podczas drukowania");
            }
        }
        private void Check_PaidValue(object sender, TextCompositionEventArgs e)
        {
            if (Regex_Check.CheckNumbers(e.Text))
            {
                if (((TextBox)sender).Text.Length != 0)
                {
                    if (e.Text == ",")
                    {
                        if (((TextBox)sender).Text.Contains(","))
                        {
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
                else
                {
                    e.Handled = true;
                }
            }
            else
            {
                if (((TextBox)sender).Text.Length == 0 && e.Text == "0")
                {
                    e.Handled = true;
                }

                if (((TextBox)sender).Text.Length > 0 && e.Text != ",")
                {
                    if (Regex.IsMatch(Proform_Paid.Text.Insert(Proform_Paid.SelectionStart, e.Text), @"\,\d\d\d"))
                    {
                        e.Handled = true;
                        MessageBox.Show("Nie można dodać wiecej niz dwie cyfry po przecinku");
                    }
                    else if (decimal.Parse(Proform_Paid.Text.Insert(Proform_Paid.SelectionStart, e.Text)) > ProductsValue)
                    {
                        e.Handled = true;
                        MessageBox.Show("Wartość zapłacona nie może być większa niż wartość produktów");
                    }
                }
                else if (((TextBox)sender).Text.Length == 0 && int.Parse(e.Text) > 0)
                {
                    if (int.Parse(e.Text) > ProductsValue)
                    {
                        e.Handled = true;
                        ((TextBox)sender).Text = "0";
                        MessageBox.Show("Wartość zapłacona nie może być większa niż wartość produktów");
                    }
                }
            }
        }
        private void ResetValue(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(Proform_Paid.Text, out decimal paid))
            {
                Proform_Paid.Text = "0";
            }
        }
    }
}
