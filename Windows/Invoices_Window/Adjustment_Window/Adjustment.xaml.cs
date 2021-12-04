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
using Frontier.Windows.Invoices_Window.Adjustment_Window.Edit_Window;
using Frontier.Windows.Invoices_Window.Purchase_Window;
using Microsoft.EntityFrameworkCore;
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

namespace Frontier.Windows.Invoices_Window.Adjustment_Window
{
    public partial class Adjustment : Page, INotifyPropertyChanged
    {
        private bool IsSaved = false;
        private bool IsDownloaded = false;
        private int SelectedSearchType { get; set; }
        private int InvoiceID { get; set; }
        private decimal ExchangeRate { get; set; }
        private decimal productsvalue;
        public decimal ProductsValue
        {
            get { return productsvalue; }
            set
            {
                if (productsvalue == value) return;
                productsvalue = Math.Round(decimal.Parse(value.ToString("F2")), 2);
                NotifyPropertyChanged("ProductsValue");
            }
        }
        InvoiceData _InvoiceData { get; set; }
        private List<ProductsSold_ViewModel> SourceList = new List<ProductsSold_ViewModel>();

        public Adjustment()
        {
            InitializeComponent();
            Products_Grid.ItemsSource = Collections.Products_Correction;
            ContactorsList.ItemsSource = Collections.ContactorsData;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private async void ReCalculateValue(object sender, EventArgs e)
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

                    if (decimal.Parse(Proform_Paid.Text) > ProductsValue) { Proform_Paid.Text = ProductsValue.ToString(); }
                    if (SelectedSearchType == 1) { Proform_Paid.Text = ProductsValue.ToString(); }
                }));
            });
        }
        private async void Delete_Clicked(object sender, RoutedEventArgs e)
        {
            Confirmation confirm = new Confirmation("Delete");
            confirm.Owner = Application.Current.MainWindow;
            confirm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            bool? data = confirm.ShowDialog();
            if (data.HasValue && data.Value)
            {
                await DeleteRows("Delete");
            }
        }
        private async Task DeleteRows(string type)
        {
            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
            await Task.Run(async () =>
            {
                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    List<decimal[]> numericsData = new List<decimal[]>();
                    List<string[]> stringsData = new List<string[]>();
                    if (type == "Delete")
                    {
                        foreach (ProductsSold_ViewModel data in Products_Grid.SelectedItems)
                        {
                            numericsData.Add(new decimal[] { data.ID, data.Amount, data.PieceBrutto, data.PieceNetto });
                            stringsData.Add(new string[] { data.Name, data.GroupType });
                        }
                    }
                    else
                    {
                        foreach (ProductsSold_ViewModel data in Products_Grid.Items)
                        {
                            numericsData.Add(new decimal[] { data.ID, data.Amount, data.PieceBrutto, data.PieceNetto });
                            stringsData.Add(new string[] { data.Name, data.GroupType });
                        }
                    }

                    int i = 0;
                    foreach (decimal[] data in numericsData)
                    {
                        if (stringsData[i][1] != "Usługa")
                        {
                            var listitem = SourceList.FirstOrDefault(x => x.ID == (int)data[0] && x.PieceNetto == data[3] && x.PieceBrutto == data[2]);
                            var whID_item = Collections.WarehouseData.FirstOrDefault(x => x.ID == (int)data[0] && x.Brutto == data[2] && x.Netto == data[3]);
                            var whName_item = Collections.WarehouseData.FirstOrDefault(x => x.Name == stringsData[i][0] && x.Brutto == data[2] && x.Netto == data[3]);
                            var grid_item = Collections.Products_Correction.FirstOrDefault(x => x.ID == (int)data[0] && x.PieceNetto == data[3] && x.PieceBrutto == data[2]);

                            if (listitem == null)
                            {
                                if (SelectedSearchType == 0)
                                {
                                    whID_item.Amount += (int)data[1];
                                }
                                Collections.Products_Correction.Remove(grid_item);
                            }
                            else if (SelectedSearchType == 1)
                            {

                                if (whID_item == null && whName_item == null)
                                {
                                    grid_item.Amount = listitem.Amount;
                                    ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                                    MessageBox.Show("Brak zakupionego przedmiotu - " + stringsData[i][0] + " - w magazynie. Nie można usunąć.");
                                }
                                else if ((whID_item?.Amount < listitem.Amount) || (whName_item?.Amount < listitem.Amount))
                                {
                                    grid_item.Amount = listitem.Amount;
                                    ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                                    MessageBox.Show("Brak ilości przedmiotu - " + stringsData[i][0] + " - w magazynie. Nie można usunąć.");
                                }
                                else
                                {
                                    Collections.Products_Correction.Remove(grid_item);
                                }
                            }
                        }
                        i++;
                    }
                }));
            });
            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
        }
        private void CheckNumeric(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex_Check.CheckNumbers(e.Text);
        }
        private void SelectionDate_Clicked(object sender, SelectionChangedEventArgs e)
        {
            var senderName = ((DatePicker)sender).Name;
            if (senderName == "Created_Date" && SellDate.Text != string.Empty)
            {
                if (Convert.ToDateTime(SellDate.Text) > Created_Date.SelectedDate.Value.Date)
                {
                    MessageBox.Show("Data wystawienia faktury nie może być wcześniejsza od daty sprzedaży");
                    return;
                }
            }
            else if (senderName == "Sell_Date" && CreatedDate.Text != string.Empty)
            {
                if (Convert.ToDateTime(CreatedDate.Text) < Sell_Date.SelectedDate.Value.Date)
                {
                    MessageBox.Show("Data sprzedaży nie może być późniejsza od daty wystawienia faktury");
                    return;
                }
            }
            _ = senderName == "Sell_Date" ? SellDate.Text = Sell_Date.SelectedDate.Value.Date.ToShortDateString() : CreatedDate.Text = Created_Date.SelectedDate.Value.Date.ToShortDateString();
        }
        private async void FindInvoice_Clicked(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                await Dispatcher.BeginInvoke(new Action(async () =>
                {
                    try
                    {
                        ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                        SelectedSearchType = InvoiceType.SelectedIndex;
                        IsDownloaded = false;
                        if (Products_Grid.Items.Count > 0 && IsSaved == false && SelectedSearchType == 0) { await DeleteRows("Restore"); }
                        ResetInputs();
                        BlockInputs();
                        if (InvoiceType.SelectedIndex == 0)
                        {
                            Invoice_Sold sold = await Find_SoldInvoices(SearchBox.Text.ToLower());
                            if (sold != null) { FillData(sold); } else { return; }
                        }
                        else
                        {
                            Invoice_Bought bought = await Find_BoughtInvoices(SearchBox.Text.ToLower());
                            if (bought != null) { FillData(bought); } else { return; }
                        }
                        if (ExchangeRate > 0) { CurrencyList.IsEnabled = true; } else { CurrencyList.IsEnabled = false; }
                        IsDownloaded = true;
                        ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                    }
                    catch (Exception)
                    {
                        ResetInputs();
                        ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                        MessageBox.Show("Wystąpił błąd podczas wyszukiwania");
                    }
                }));
            });
        }
        private async Task<Invoice_Sold> Find_SoldInvoices(string Invoice_Number)
        {
            Invoice_Sold sold = new Invoice_Sold();

            using (GetInvoice_Sold Invoicesold = new GetInvoice_Sold())
            {
                sold = await Invoicesold.Invoice_Sold.FirstOrDefaultAsync(x => x.Invoice_ID.ToLower() == Invoice_Number);
            }

            if (sold != null)
            {
                using (GetInvoice_Products Invoice_Products = new GetInvoice_Products())
                {
                    var products = await Invoice_Products.Invoice_Products.Where(x => x.Invoice_ID == sold.Invoice_ID && x.Invoice == sold.idInvoice_Sold).ToListAsync();
                    foreach (var item in products)
                    {
                        SourceList.Add(new ProductsSold_ViewModel
                        {
                            ID_SoldProduct = item.id,
                            ID = int.Parse(item.Product_ID),
                            Name = item.Name,
                            GroupType = Collections.GroupsData.FirstOrDefault(x => x.ID == Collections.WarehouseData.FirstOrDefault(z => z.ID == int.Parse(item.Product_ID)).GroupID).Type == 0 ? "Towar" : "Usługa",
                            Amount = int.Parse(item.Amount),
                            PieceNetto = decimal.Parse(item.Each_Netto),
                            PieceBrutto = decimal.Parse(item.Each_Brutto),
                            VAT = item.VAT,
                            Netto = decimal.Parse(item.Netto),
                            VATAmount = decimal.Parse(item.VAT_Price),
                            Brutto = decimal.Parse(item.Brutto),
                            GTU = item.GTU
                        });
                        Collections.Products_Correction.Add(new ProductsSold_ViewModel
                        {
                            ID_SoldProduct = item.id,
                            ID = int.Parse(item.Product_ID),
                            Name = item.Name,
                            GroupType = Collections.GroupsData.FirstOrDefault(x => x.ID == Collections.WarehouseData.FirstOrDefault(z => z.ID == int.Parse(item.Product_ID)).GroupID).Type == 0 ? "Towar" : "Usługa",
                            Amount = int.Parse(item.Amount),
                            PieceNetto = decimal.Parse(item.Each_Netto),
                            PieceBrutto = decimal.Parse(item.Each_Brutto),
                            VAT = item.VAT,
                            Netto = decimal.Parse(item.Netto),
                            VATAmount = decimal.Parse(item.VAT_Price),
                            Brutto = decimal.Parse(item.Brutto),
                            GTU = item.GTU
                        });
                    }
                    InvoiceID = sold.idInvoice_Sold;
                }
            }
            else
            {
                ResetInputs();
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                MessageBox.Show("Nie znaleziono faktury o podanym numerze");
            }
            return sold;
        }
        private async Task<Invoice_Bought> Find_BoughtInvoices(string Invoice_Number)
        {
            Invoice_Bought bought = new Invoice_Bought();

            using (GetInvoice_Bought InvoiceBought = new GetInvoice_Bought())
            {
                bought = await InvoiceBought.Invoice_Bought.FirstOrDefaultAsync(x => x.Invoice_ID.ToLower() == Invoice_Number);
            }

            if (bought != null)
            {
                using (GetInvoice_Products Invoice_Products = new GetInvoice_Products())
                {
                    var products = await Invoice_Products.Invoice_Products.Where(x => x.Invoice_ID == bought.Invoice_ID && x.Invoice == bought.idinvoice_bought).ToListAsync();
                    foreach (var item in products)
                    {
                        SourceList.Add(new ProductsSold_ViewModel
                        {
                            ID_SoldProduct = item.id,
                            ID = int.Parse(item.Product_ID),
                            Name = item.Name,
                            GroupType = item.GroupType,
                            Amount = int.Parse(item.Amount),
                            PieceNetto = decimal.Parse(item.Each_Netto),
                            PieceBrutto = decimal.Parse(item.Each_Brutto),
                            VAT = item.VAT,
                            Netto = decimal.Parse(item.Netto),
                            VATAmount = decimal.Parse(item.VAT_Price),
                            Brutto = decimal.Parse(item.Brutto),
                            GTU = item.GTU
                        });
                        Collections.Products_Correction.Add(new ProductsSold_ViewModel
                        {
                            ID_SoldProduct = item.id,
                            ID = int.Parse(item.Product_ID),
                            Name = item.Name,
                            GroupType = item.GroupType,
                            Amount = int.Parse(item.Amount),
                            PieceNetto = decimal.Parse(item.Each_Netto),
                            PieceBrutto = decimal.Parse(item.Each_Brutto),
                            VAT = item.VAT,
                            Netto = decimal.Parse(item.Netto),
                            VATAmount = decimal.Parse(item.VAT_Price),
                            Brutto = decimal.Parse(item.Brutto),
                            GTU = item.GTU
                        });
                    }
                    InvoiceID = bought.idinvoice_bought;
                }
            }
            else
            {
                ResetInputs();
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                MessageBox.Show("Nie znaleziono faktury o podanym numerze");
            }
            return bought;
        }
        private async void ReCalculate_Products(object sender, RoutedEventArgs e)
        {
            if (Products_Grid != null && IsDownloaded)
            {
                await Task.Run(async () =>
                {
                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            ProductsValue = 0;
                            foreach (ProductsSold_ViewModel data in Products_Grid.Items)
                            {
                                data.Brutto = CurrencyList.SelectedIndex == 0 ? data.Brutto *= ExchangeRate : data.Brutto /= ExchangeRate;
                                data.Netto = CurrencyList.SelectedIndex == 0 ? data.Netto *= ExchangeRate : data.Netto /= ExchangeRate;
                                data.PieceBrutto = CurrencyList.SelectedIndex == 0 ? data.PieceBrutto *= ExchangeRate : data.PieceBrutto /= ExchangeRate;
                                data.PieceNetto = CurrencyList.SelectedIndex == 0 ? data.PieceNetto *= ExchangeRate : data.PieceNetto /= ExchangeRate;
                                data.VATAmount = data.Brutto - data.Netto;
                                ProductsValue += data.Brutto;
                            }
                            var paidvalue = CurrencyList.SelectedIndex == 0 ? decimal.Parse(Proform_Paid.Text) * ExchangeRate : decimal.Parse(Proform_Paid.Text) / ExchangeRate;
                            if (paidvalue > ProductsValue)
                            {
                                paidvalue = ProductsValue;
                            }
                            Proform_Paid.Text = paidvalue.ToString("0.00");
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Wystąpił błąd podczas przeliczania");
                            _ = CurrencyList.SelectedIndex == 0 ? CurrencyList.SelectedIndex = 1 : CurrencyList.SelectedIndex = 0;
                        }
                    }));
                });
            }
        }
        private void FillData(object sold)
        {
            if (sold != null)
            {
                var FindType = Main_InvoiceType.Items.OfType<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == (sold.GetType() == typeof(Invoice_Sold) == true ? (sold as Invoice_Sold).Invoice_Type : "Sprzedaż"));
                var FindPurchase = PurchaseType.Items.OfType<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == (sold.GetType() == typeof(Invoice_Sold) == true ? (sold as Invoice_Sold).Purchase_type : (sold as Invoice_Bought).Purchase_type));
                var FindCurrency = CurrencyList.Items.OfType<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == (sold.GetType() == typeof(Invoice_Sold) == true ? (sold as Invoice_Sold).Currency : (sold as Invoice_Bought).Currency));
                Main_InvoiceType.SelectedIndex = Main_InvoiceType.Items.IndexOf(FindType);
                ContactorsList.SelectedIndex = Collections.ContactorsData.IndexOf(Collections.ContactorsData.FirstOrDefault(x => x.ID.ToString() == (sold.GetType() == typeof(Invoice_Sold) == true ? (sold as Invoice_Sold).Receiver : (sold as Invoice_Bought).Seller_ID.ToString())));
                PurchaseType.SelectedIndex = PurchaseType.Items.IndexOf(FindPurchase);
                CurrencyList.SelectedIndex = CurrencyList.Items.IndexOf(FindCurrency);
                InvoiceNumber.Text = sold.GetType() == typeof(Invoice_Sold) == true ? (sold as Invoice_Sold).Invoice_ID : (sold as Invoice_Bought).Invoice_ID;
                Days_Amount.Text = sold.GetType() == typeof(Invoice_Sold) == true ? (sold as Invoice_Sold).Day_Limit : string.Empty;
                SellDate.Text = sold.GetType() == typeof(Invoice_Sold) == true ? (sold as Invoice_Sold).Date_Sold : (sold as Invoice_Bought).Date_Bought;
                CreatedDate.Text = sold.GetType() == typeof(Invoice_Sold) == true ? (sold as Invoice_Sold).Date_Created.ToShortDateString() : (sold as Invoice_Bought).Date_Created.ToShortDateString();
                Description.Text = sold.GetType() == typeof(Invoice_Sold) == true ? (sold as Invoice_Sold).Description : string.Empty;
                BankName.Text = sold.GetType() == typeof(Invoice_Sold) == true ? (sold as Invoice_Sold).BankName : string.Empty;
                BankAccount.Text = sold.GetType() == typeof(Invoice_Sold) == true ? (sold as Invoice_Sold).AccountNumber : string.Empty;
                Proform_Paid.Text = sold.GetType() == typeof(Invoice_Sold) == true ? (sold as Invoice_Sold).PricePaid : ProductsValue.ToString();
                if ((sold.GetType() == typeof(Invoice_Sold) == true ? (sold as Invoice_Sold).Currency : (sold as Invoice_Bought).Currency) != "PLN")
                {
                    ExchangeRate = decimal.Parse(sold.GetType() == typeof(Invoice_Sold) == true ? (sold as Invoice_Sold).ExchangeRate : "0");
                    if (CurrencyList.Items.Count > 1)
                    {
                        CurrencyList.Items.RemoveAt(1);
                    }
                    CurrencyList.Items.Add(sold.GetType() == typeof(Invoice_Sold) == true ? (sold as Invoice_Sold).Currency : (sold as Invoice_Bought).Currency);
                    CurrencyList.SelectedIndex = 1;
                }
                else { ExchangeRate = 0; }
            }
        }
        private void PurchaseType_Selection(object sender, RoutedEventArgs e)
        {
            if (SelectedSearchType == 0 && PurchaseType != null && BankName != null && BankAccount != null)
            {
                switch (PurchaseType.SelectedIndex)
                {
                    case 1:
                        BankName.IsEnabled = true;
                        BankAccount.IsEnabled = true;
                        break;
                    default:
                        BankName.IsEnabled = false;
                        BankAccount.IsEnabled = false;
                        BankName.Text = string.Empty;
                        BankAccount.Text = string.Empty;
                        break;
                }
            }
        }
        private void Edit_SelectedProduct(object sender, RoutedEventArgs e)
        {
            if (Products_Grid.SelectedItem == null) { return; }
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var item = (DataGrid)contextMenu.PlacementTarget;
            var Product = (ProductsSold_ViewModel)item.SelectedItem;
            if (Product == null) { return; }
            var ListItem = SourceList.FirstOrDefault(x => x.ID_SoldProduct == Product.ID_SoldProduct);
            _ = ListItem != null ? Product.Amount = ListItem.Amount : Product.Amount;
            Edit window = new Edit(Product, SelectedSearchType);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
            if (ListItem != null)
            {
                ListItem.PieceBrutto = Product.PieceBrutto;
                ListItem.PieceNetto = Product.PieceNetto;
                ListItem.VAT = Product.VAT;
                ListItem.Netto = Product.Netto;
                ListItem.Brutto = Product.Brutto;
                ListItem.VATAmount = Product.VATAmount;
            }
            ReCalculateValue(null, null);
        }
        private void BlockInputs()
        {
            Collections.Products_Correction.Clear();
            AddProduct_Button.IsEnabled = true;
            PurchaseType.IsEnabled = true;
            ContactorsList.IsEnabled = true;
            Created_Date.IsEnabled = true;
            Sell_Date.IsEnabled = true;
            if (InvoiceType.SelectedIndex == 0)
            {
                Description.IsEnabled = true;
                InvoiceNumber.IsEnabled = false;
                Days_Amount.IsEnabled = true;
                Proform_Paid.IsHitTestVisible = true;
                Signature.IsEnabled = true;
            }
            else
            {
                Description.IsEnabled = false;
                Proform_Paid.IsHitTestVisible = false;
                InvoiceNumber.IsEnabled = true;
                Days_Amount.IsEnabled = false;
                Signature.IsEnabled = false;
            }
        }
        private void ResetInputs()
        {
            ContactorsList.IsEnabled = false;
            PurchaseType.IsEnabled = false;
            InvoiceNumber.IsEnabled = false;
            Days_Amount.IsEnabled = false;
            Description.IsEnabled = false;
            Created_Date.IsEnabled = false;
            CurrencyList.IsEnabled = false;
            Signature.IsEnabled = false;
            Sell_Date.IsEnabled = false;
            AddProduct_Button.IsEnabled = false;
            Proform_Paid.IsHitTestVisible = false;
            InvoiceNumber.Text = string.Empty;
            Days_Amount.Text = string.Empty;
            Description.Text = string.Empty;
            SellDate.Text = string.Empty;
            CreatedDate.Text = string.Empty;
            Signature.Text = string.Empty;
            Proform_Paid.Text = "0";
            PurchaseType.SelectedIndex = 0;
            ContactorsList.SelectedIndex = -1;
            CurrencyList.SelectedIndex = 0;
            ExchangeRate = 0;
            InvoiceID = 0;
            IsSaved = false;
            Collections.Products_Correction.Clear();
            SourceList.Clear();
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
                else if (((TextBox)sender).Text.Length > 0 && e.Text != ",")
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
        private async void SavePDF_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedSearchType == 1)
                {
                    MessageBox.Show("Brak możlwości zapisu faktury kupna");
                    return;
                }
                if (!IsSaved)
                {
                    MessageBox.Show("Proszę zapisać najpierw zapisać fakturę");
                    return;
                }

                await Save_Invoice.SavePDF(_InvoiceData);
            }
            catch (Exception)
            {
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                MessageBox.Show("Błąd podczas zapisu");
            }
        }
        private async void SaveWord_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedSearchType == 1)
                {
                    MessageBox.Show("Brak możlwości zapisu faktury kupna");
                    return;
                }
                if (!IsSaved)
                {
                    MessageBox.Show("Proszę zapisać najpierw zapisać fakturę");
                    return;
                }
                await Save_Invoice.SaveWord(_InvoiceData);
            }
            catch (Exception)
            {
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                MessageBox.Show("Błąd podczas zapisu");
            }
        }
        private async void Print_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedSearchType == 1)
                {
                    MessageBox.Show("Brak możlwości wyrduku faktury kupna");
                    return;
                }
                if (!IsSaved)
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
        private async void Clear_Clicked(object sender, RoutedEventArgs e)
        {
            if (Products_Grid.Items.Count > 0 && IsSaved == false && SelectedSearchType == 0) { await DeleteRows("Restore"); }
            BlockInputs();
            ResetInputs();
        }
        private void AddProduct_Clicked(object sender, RoutedEventArgs e)
        {
            if (SelectedSearchType == 0)
            {
                AddProduct products = new AddProduct(InvoiceType.Text, CurrencyList.Text, ExchangeRate, "Adjustment");
                products.Owner = Application.Current.MainWindow;
                products.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                products.ShowDialog();
            }
            else
            {
                AddBought window = new AddBought();
                window.Owner = Application.Current.MainWindow;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.ShowDialog();
            }
            ReCalculateValue(null, null);
        }
        private async void SaveCorrection_Clicked(object sender, RoutedEventArgs e)
        {
            if (PurchaseType.SelectedIndex == 1 && SelectedSearchType == 0 && (BankName.Text == string.Empty || BankAccount.Text == string.Empty)) { MessageBox.Show("Proszę wypełnić dane bankowe"); return; }
            if (Products_Grid.Items.Count == 0 || ContactorsList.SelectedIndex == -1 || InvoiceNumber.Text == string.Empty || SellDate.Text == string.Empty || CreatedDate.Text == string.Empty) { MessageBox.Show("Proszę wypełnić wymagane dane"); return; }

            Confirmation confirm = new Confirmation("Save");
            confirm.Owner = Application.Current.MainWindow;
            confirm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            bool? data = confirm.ShowDialog();
            if (data.HasValue && data.Value)
            {
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                await Task.Run(async () =>
                {
                    await Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        try
                        {
                            if (SelectedSearchType == 0)
                            {
                                await SaveSold_Correction();
                            }
                            else
                            {
                                await SaveBought_Correction();
                            }
                            GenerateInvoice_Data();
                            IsSaved = true;
                            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                            MessageBox.Show("Pomyślnie zapisano korekte faktury");
                        }
                        catch (Exception d)
                        {
                            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                            MessageBox.Show(d+"Wystąpił błąd podczas zapisywania");
                        }
                    }));
                });
            }
        }
        private async Task SaveBought_Correction()
        {
            using (GetInvoice_Bought invoice = new GetInvoice_Bought())
            {
                using (GetInvoice_Products product = new GetInvoice_Products())
                {
                    using (GetWarehouse warehouseitem = new GetWarehouse())
                    {
                        DateTimeFormatInfo format = new DateTimeFormatInfo();
                        format.ShortDatePattern = "dd.MM.yyyy";
                        var Corrected_Invoice = new Invoice_Bought
                        {
                            idinvoice_bought = InvoiceID,
                            Seller_ID = Collections.ContactorsData[ContactorsList.SelectedIndex].ID,
                            Invoice_ID = InvoiceNumber.Text,
                            Date_Bought = SellDate.Text,
                            Date_Created = Convert.ToDateTime(CreatedDate.Text, format),
                            Purchase_type = PurchaseType.Text,
                            Currency = CurrencyList.Text,
                            ExchangeRate = ExchangeRate
                        };
                        var updated = await invoice.EditProduct(Corrected_Invoice);
                        if (updated == true)
                        {
                            foreach (ProductsSold_ViewModel item in Products_Grid.Items)
                            {
                                var newproduct = new Invoice_Products
                                {
                                    id = item.ID_SoldProduct,
                                    Invoice_ID = Corrected_Invoice.Invoice_ID,
                                    Invoice = Corrected_Invoice.idinvoice_bought,
                                    Product_ID = item.ID.ToString(),
                                    Name = item.Name,
                                    Amount = item.Amount.ToString(),
                                    Each_Netto = item.PieceNetto.ToString(),
                                    VAT = item.VAT,
                                    Each_Brutto = item.PieceBrutto.ToString(),
                                    Netto = item.Netto.ToString(),
                                    VAT_Price = (item.Brutto - item.Netto).ToString(),
                                    Brutto = item.Brutto.ToString(),
                                    GTU = item.GTU,
                                    BoughtBrutto = "0",
                                    BoughtNetto = "0",
                                    BoughtVAT = "0",
                                    GroupType = item.GroupType
                                };
                                var findProduct = product.Invoice_Products.FirstOrDefault(x => x.id == item.ID_SoldProduct);
                                var query = SourceList.FirstOrDefault(x => x.ID == item.ID && x.Name == item.Name && x.PieceNetto == item.PieceNetto && x.PieceBrutto == item.PieceBrutto);
                                if (query == null)
                                {
                                    var WH_Item = Collections.WarehouseData.FirstOrDefault(x => x.Name == item.Name && x.Netto == item.PieceNetto && x.Brutto == item.PieceBrutto);
                                    if (WH_Item?.Amount == 0)
                                    {
                                        WH_Item.Amount = item.Amount;
                                        WH_Item.Brutto = item.Brutto;
                                        WH_Item.Netto = item.Netto;
                                        WH_Item.VAT = item.VAT;
                                    }
                                    else if (WH_Item?.Amount > 0)
                                    {
                                        WH_Item.Amount += item.Amount;
                                    }
                                    else
                                    {
                                        await warehouseitem.AddItem(new Warehouse
                                        {
                                            Name = item.Name,
                                            Amount = item.Amount,
                                            Netto = item.PieceNetto,
                                            VAT = item.VAT,
                                            Brutto = item.PieceBrutto,
                                            Margin = 0,
                                            Group = Collections.GroupsData.FirstOrDefault(x => x.Type == 0).ID.ToString()
                                        });
                                    }
                                    await product.AddProduct(newproduct);
                                }
                                else
                                {
                                    var findID = warehouseitem.Warehouse.FirstOrDefault(x => x.idWarehouse == item.ID && x.Netto == decimal.Parse(findProduct.Each_Netto) && x.Brutto == decimal.Parse(findProduct.Each_Brutto) && x.VAT == findProduct.VAT);
                                    var findName = warehouseitem.Warehouse.FirstOrDefault(x => x.Name == item.Name && x.Netto == decimal.Parse(findProduct.Each_Netto) && x.Brutto == decimal.Parse(findProduct.Each_Brutto) && x.VAT == findProduct.VAT);
                                    if (query.Amount > item.Amount)
                                    {
                                        if (findID != null)
                                        {
                                            if (findID.Amount == query.Amount - item.Amount)
                                            {
                                                var countItems = warehouseitem.Warehouse.Where(x => x.Name == item.Name);
                                                if (countItems.Count() > 1)
                                                {
                                                    warehouseitem.DeleteItem(item.ID);
                                                }
                                                else
                                                {
                                                    countItems.FirstOrDefault().Amount = 0;
                                                    countItems.FirstOrDefault().Brutto = 0;
                                                    countItems.FirstOrDefault().Netto = 0;
                                                    countItems.FirstOrDefault().VAT = "0";
                                                }
                                            }
                                            else
                                            {
                                                if (findProduct.Each_Brutto != item.PieceBrutto.ToString() || findProduct.Each_Netto != item.PieceNetto.ToString() || findProduct.VAT != item.VAT)
                                                {
                                                    findID.Amount -= query.Amount - item.Amount;
                                                    var newProduct = warehouseitem.Warehouse.FirstOrDefault(x => x.Name == item.Name && x.Netto == item.PieceNetto && x.Brutto == item.PieceBrutto && x.VAT == item.VAT);
                                                    if (newProduct != null)
                                                    {
                                                        newProduct.Amount += item.Amount;
                                                        warehouseitem.DeleteItem(item.ID);
                                                    }
                                                    else
                                                    {
                                                        findID.VAT = item.VAT;
                                                        findID.Brutto = item.PieceBrutto;
                                                        findID.Netto = item.PieceNetto;
                                                    }
                                                }
                                                else
                                                {
                                                    findID.Amount -= query.Amount - item.Amount;
                                                }
                                            }
                                        }
                                        else if (findName != null)
                                        {
                                            if (findName.Amount == query.Amount - item.Amount)
                                            {
                                                warehouseitem.DeleteItem(findName.idWarehouse);
                                            }
                                            else
                                            {
                                                if (findProduct.Each_Brutto != item.PieceBrutto.ToString() || findProduct.Each_Netto != item.PieceNetto.ToString() || findProduct.VAT != item.VAT)
                                                {
                                                    findName.Amount -= query.Amount - item.Amount;
                                                    var newProduct = warehouseitem.Warehouse.FirstOrDefault(x => x.Name == item.Name && x.Netto == item.PieceNetto && x.Brutto == item.PieceBrutto && x.VAT == item.VAT);
                                                    if (newProduct != null)
                                                    {
                                                        newProduct.Amount += item.Amount;
                                                        warehouseitem.DeleteItem(item.ID);
                                                    }
                                                    else
                                                    {
                                                        findName.VAT = item.VAT;
                                                        findName.Brutto = item.PieceBrutto;
                                                        findName.Netto = item.PieceNetto;
                                                    }
                                                }
                                                else
                                                {
                                                    findName.Amount -= query.Amount - item.Amount;
                                                }
                                            }
                                        }
                                    }
                                    else if (query.Amount < item.Amount)
                                    {
                                        if (findID != null)
                                        {
                                            if (findProduct.Each_Brutto != item.PieceBrutto.ToString() || findProduct.Each_Netto != item.PieceNetto.ToString() || findProduct.VAT != item.VAT)
                                            {
                                                findID.Amount -= query.Amount;
                                                var newProduct = warehouseitem.Warehouse.FirstOrDefault(x => x.Name == item.Name && x.Netto == item.PieceNetto && x.Brutto == item.PieceBrutto && x.VAT == item.VAT);
                                                if (newProduct != null)
                                                {
                                                    newProduct.Amount += item.Amount;
                                                }
                                                else
                                                {
                                                    await warehouseitem.AddItem(new Warehouse
                                                    {
                                                        Group = findID.Group,
                                                        Name = item.Name,
                                                        Amount = item.Amount,
                                                        Netto = item.PieceNetto,
                                                        Brutto = item.PieceBrutto,
                                                        Margin = 0,
                                                        VAT = item.VAT
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                findID.Amount += item.Amount - query.Amount;
                                            }
                                        }
                                        else if (findName != null)
                                        {
                                            if (findProduct.Each_Brutto != item.PieceBrutto.ToString() || findProduct.Each_Netto != item.PieceNetto.ToString() || findProduct.VAT != item.VAT)
                                            {
                                                findName.Amount -= query.Amount;
                                                var newProduct = warehouseitem.Warehouse.FirstOrDefault(x => x.Name == item.Name && x.Netto == item.PieceNetto && x.Brutto == item.PieceBrutto && x.VAT == item.VAT);
                                                if (newProduct != null)
                                                {
                                                    newProduct.Amount += item.Amount;
                                                }
                                                else
                                                {
                                                    await warehouseitem.AddItem(new Warehouse
                                                    {
                                                        Group = findName.Group,
                                                        Name = item.Name,
                                                        Amount = item.Amount,
                                                        Netto = item.PieceNetto,
                                                        Brutto = item.PieceBrutto,
                                                        Margin = 0,
                                                        VAT = item.VAT
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                findName.Amount += item.Amount - query.Amount;
                                            }
                                        }
                                    }
                                    else if (query.Amount == item.Amount)
                                    {
                                        if (findProduct.Each_Brutto != item.PieceBrutto.ToString() || findProduct.Each_Netto != item.PieceNetto.ToString() || findProduct.VAT != item.VAT)
                                        {
                                            if (findID != null)
                                            {
                                                var newProduct = warehouseitem.Warehouse.FirstOrDefault(x => x.Name == item.Name && x.Netto == item.PieceNetto && x.Brutto == item.PieceBrutto && x.VAT == item.VAT);
                                                if (newProduct != null)
                                                {
                                                    if (findID.Amount == item.Amount)
                                                    {
                                                        warehouseitem.DeleteItem(item.ID);
                                                    }
                                                    else
                                                    {
                                                        findID.Amount -= item.Amount;
                                                    }
                                                    newProduct.Amount += item.Amount;
                                                }
                                                else
                                                {
                                                    findID.VAT = item.VAT;
                                                    findID.Brutto = item.PieceBrutto;
                                                    findID.Netto = item.PieceNetto;
                                                }
                                            }
                                            else if (findName != null)
                                            {
                                                var newProduct = warehouseitem.Warehouse.FirstOrDefault(x => x.Name == item.Name && x.Netto == item.PieceNetto && x.Brutto == item.PieceBrutto && x.VAT == item.VAT);
                                                if (newProduct != null)
                                                {
                                                    if (findName.Amount == item.Amount)
                                                    {
                                                        warehouseitem.DeleteItem(item.ID);
                                                    }
                                                    else
                                                    {
                                                        findName.Amount -= item.Amount;
                                                    }
                                                    newProduct.Amount += item.Amount;
                                                }
                                                else
                                                {
                                                    findName.VAT = item.VAT;
                                                    findName.Brutto = item.PieceBrutto;
                                                    findName.Netto = item.PieceNetto;
                                                }
                                            }
                                        }
                                    }
                                    await product.EditProduct(newproduct);
                                }
                            }

                            //Delete removed items from Warehouse
                            foreach (ProductsSold_ViewModel item in SourceList)
                            {
                                if (Collections.Products_Correction.FirstOrDefault(x => x.ID_SoldProduct == item.ID_SoldProduct) == null)
                                {
                                    if (item.GroupType != "Usługa")
                                    {
                                        var query = SourceList.FirstOrDefault(x => x.ID_SoldProduct == item.ID_SoldProduct);
                                        var findItemID = warehouseitem.Warehouse.FirstOrDefault(x => x.idWarehouse == item.ID && x.Netto == item.PieceNetto && x.Brutto == item.PieceBrutto);
                                        var findItemName = warehouseitem.Warehouse.FirstOrDefault(x => x.Name == item.Name && x.Netto == item.PieceNetto && x.Brutto == item.PieceBrutto);
                                        if (findItemID != null)
                                        {
                                            if (findItemID.Amount == query.Amount)
                                            {
                                                var countItems = warehouseitem.Warehouse.Where(x => x.Name == item.Name);
                                                if (countItems.Count() > 1)
                                                {
                                                    warehouseitem.DeleteItem(item.ID);
                                                }
                                                else
                                                {
                                                    countItems.FirstOrDefault().Amount = 0;
                                                    countItems.FirstOrDefault().Brutto = 0;
                                                    countItems.FirstOrDefault().Netto = 0;
                                                    countItems.FirstOrDefault().VAT = "0";
                                                }
                                            }
                                            else
                                            {
                                                findItemID.Amount -= query.Amount;
                                            }

                                        }
                                        else if (findItemName != null)
                                        {
                                            if (findItemName.Amount == query.Amount)
                                            {
                                                var countItems = warehouseitem.Warehouse.Where(x => x.Name == item.Name);
                                                if (countItems.Count() > 1)
                                                {
                                                    warehouseitem.DeleteItem(item.ID);
                                                }
                                                else
                                                {
                                                    countItems.FirstOrDefault().Amount = 0;
                                                    countItems.FirstOrDefault().Brutto = 0;
                                                    countItems.FirstOrDefault().Netto = 0;
                                                    countItems.FirstOrDefault().VAT = "0";
                                                }
                                            }
                                            else
                                            {
                                                findItemName.Amount -= query.Amount;
                                            }
                                        }
                                    }
                                    product.DeleteItem(item.ID_SoldProduct);
                                }
                            }

                            await warehouseitem.SaveChangesAsync();
                            await product.SaveChangesAsync();
                            await invoice.SaveChangesAsync();

                            //Fill ids to newly added items
                            foreach (ProductsSold_ViewModel item in Products_Grid.Items)
                            {
                                if (SourceList.FirstOrDefault(x => x.ID == item.ID && x.Name == item.Name && x.PieceBrutto == item.PieceBrutto && x.PieceNetto == item.PieceNetto && x.VAT == item.VAT) == null)
                                {
                                    var LastID = warehouseitem.Warehouse.FirstOrDefault(x => x.Name == item.Name && x.Netto == item.PieceNetto && x.Brutto == item.PieceBrutto && x.VAT == item.VAT).idWarehouse;
                                    product.Invoice_Products.FirstOrDefault(x => x.Invoice == InvoiceID && x.Invoice_ID == InvoiceNumber.Text && x.Name == item.Name && x.Each_Netto == item.PieceNetto.ToString() && x.Each_Brutto == item.PieceBrutto.ToString() && x.VAT == item.VAT).Product_ID = LastID.ToString();
                                }
                            }
                            await product.SaveChangesAsync();
                            await Download_WarehouseItems.Download();
                            SourceList.Clear();
                            Collections.Products_Correction.Clear();
                            await Find_BoughtInvoices(InvoiceNumber.Text.ToLower());
                        }
                        else
                        {
                            throw new ArgumentNullException();
                        }
                    }
                }
            }
        }
        private async Task SaveSold_Correction()
        {
            using (GetInvoice_Sold invoice = new GetInvoice_Sold())
            {
                using (GetInvoice_Products product = new GetInvoice_Products())
                {
                    using (GetWarehouse warehouseitem = new GetWarehouse())
                    {
                        DateTimeFormatInfo format = new DateTimeFormatInfo();
                        format.ShortDatePattern = "dd.MM.yyyy";

                        var Corrected_Invoice = new Invoice_Sold
                        {
                            idInvoice_Sold = InvoiceID,
                            Receiver = Collections.ContactorsData[ContactorsList.SelectedIndex].ID.ToString(),
                            Invoice_ID = InvoiceNumber.Text,
                            Invoice_Type = InvoiceType.Text,
                            Date_Sold = SellDate.Text,
                            Date_Created = Convert.ToDateTime(CreatedDate.Text, format),
                            Purchase_type = PurchaseType.Text,
                            Day_Limit = Days_Amount.Text,
                            Currency = CurrencyList.Text,
                            Description = Description.Text,
                            AccountNumber = BankAccount.Text,
                            BankName = BankName.Text,
                            PricePaid = Proform_Paid.Text,
                            ExchangeRate = ExchangeRate.ToString(),
                        };
                        var updated = await invoice.EditInvoice(Corrected_Invoice);
                        if (updated == true)
                        {
                            foreach (ProductsSold_ViewModel item in Products_Grid.Items)
                            {
                                var findBoughtPrice = product.Invoice_Products.FirstOrDefault(x => x.Invoice_ID == Corrected_Invoice.Invoice_ID && x.Name == item.Name && x.Product_ID == item.ID.ToString());
                                var newproduct = new Invoice_Products
                                {
                                    id = item.ID_SoldProduct,
                                    Invoice_ID = Corrected_Invoice.Invoice_ID,
                                    Invoice = Corrected_Invoice.idInvoice_Sold,
                                    Product_ID = item.ID.ToString(),
                                    Name = item.Name,
                                    Amount = item.Amount.ToString(),
                                    Each_Netto = item.PieceNetto.ToString(),
                                    VAT = item.VAT,
                                    Each_Brutto = item.PieceBrutto.ToString(),
                                    Netto = item.Netto.ToString(),
                                    VAT_Price = (item.Brutto - item.Netto).ToString(),
                                    Brutto = item.Brutto.ToString(),
                                    GTU = item.GTU,
                                    BoughtBrutto = findBoughtPrice != null ? findBoughtPrice.BoughtBrutto : Collections.WarehouseData.FirstOrDefault(x => x.ID == item.ID).Brutto.ToString(),
                                    BoughtNetto = findBoughtPrice != null ? findBoughtPrice.BoughtNetto : Collections.WarehouseData.FirstOrDefault(x => x.ID == item.ID).Netto.ToString(),
                                    BoughtVAT = findBoughtPrice != null ? findBoughtPrice.BoughtVAT : Collections.WarehouseData.FirstOrDefault(x => x.ID == item.ID).VAT,
                                    GroupType = item.GroupType
                                };
                                var query = SourceList.FirstOrDefault(x => x.ID == item.ID && x.Name == item.Name && x.PieceNetto == item.PieceNetto && x.PieceBrutto == item.PieceBrutto);
                                var wh_item = Collections.WarehouseData.FirstOrDefault(x => x.ID == item.ID);
                                if (query == null)
                                {
                                    await product.AddProduct(newproduct);
                                    if (item.GroupType != "Usługa")
                                    {
                                        await warehouseitem.SoldWarehouse_Item(new Warehouse
                                        {
                                            idWarehouse = item.ID,
                                            Name = item.Name,
                                            Amount = item.Amount
                                        });
                                    }
                                }
                                else
                                {
                                    await product.EditProduct(newproduct);
                                    if (item.GroupType != "Usługa")
                                    {
                                        if (query.Amount > item.Amount)
                                        {
                                            await RestoreToWarehouse(item, query, warehouseitem, findBoughtPrice);
                                        }
                                        else if (query.Amount < item.Amount)
                                        {
                                            await warehouseitem.SoldWarehouse_Item(new Warehouse
                                            {
                                                idWarehouse = item.ID,
                                                Name = item.Name,
                                                Amount = item.Amount - query.Amount
                                            });
                                        }
                                    }
                                }
                            }
                            foreach (ProductsSold_ViewModel item in SourceList)
                            {
                                if (Collections.Products_Correction.FirstOrDefault(x => x.ID_SoldProduct == item.ID_SoldProduct) == null)
                                {
                                    if (item.GroupType != "Usługa")
                                    {
                                        var findBoughtPrice = product.Invoice_Products.FirstOrDefault(x => x.Invoice_ID == Corrected_Invoice.Invoice_ID && x.Name == item.Name && x.Product_ID == item.ID.ToString());
                                        var query = SourceList.FirstOrDefault(x => x.ID == item.ID && x.Name == item.Name && x.PieceNetto == item.PieceNetto && x.PieceBrutto == item.PieceBrutto && x.VAT == item.VAT);
                                        await RestoreToWarehouse(item, query, warehouseitem, findBoughtPrice);
                                    }
                                    product.DeleteItem(item.ID_SoldProduct);
                                }
                            }

                            await invoice.SaveChangesAsync();
                            await product.SaveChangesAsync();
                            await warehouseitem.SaveChangesAsync();
                            await Download_WarehouseItems.Download();
                            SourceList.Clear();
                            Collections.Products_Correction.Clear();
                            await Find_SoldInvoices(InvoiceNumber.Text.ToLower());
                        }
                        else
                        {
                            throw new ArgumentNullException();
                        }
                    }
                }
            }
        }
        private async Task RestoreToWarehouse(ProductsSold_ViewModel item, ProductsSold_ViewModel query, GetWarehouse warehouseitem, Invoice_Products findBoughtPrice)
        {
            var id_exists = Collections.WarehouseData.FirstOrDefault(x => x.ID == item.ID);
            var name_exists = Collections.WarehouseData.FirstOrDefault(x => x.Name == item.Name);
            if (id_exists != null)
            {
                if (id_exists.Amount == 0)
                {
                    id_exists.Amount = query.Amount - item.Amount;
                    id_exists.Netto = decimal.Parse(findBoughtPrice.BoughtNetto);
                    id_exists.Brutto = decimal.Parse(findBoughtPrice.BoughtBrutto);
                    id_exists.VAT = findBoughtPrice.BoughtVAT;
                }
                else
                {
                    if (Collections.WarehouseData.FirstOrDefault(x => x.ID == item.ID && x.Brutto == decimal.Parse(findBoughtPrice.BoughtBrutto) && x.Netto == decimal.Parse(findBoughtPrice.BoughtNetto)) != null)
                    {
                        if (query.Amount == item.Amount)
                        {
                            warehouseitem.Warehouse.FirstOrDefault(x => x.idWarehouse == item.ID).Amount += item.Amount;
                        }
                        else
                        {
                            warehouseitem.Warehouse.FirstOrDefault(x => x.idWarehouse == item.ID).Amount += query.Amount - item.Amount;
                        }
                    }
                    else
                    {
                        await warehouseitem.AddItem(new Warehouse
                        {
                            Name = item.Name,
                            Amount = query.Amount != item.Amount ? query.Amount - item.Amount : item.Amount,
                            Netto = decimal.Parse(findBoughtPrice.BoughtNetto),
                            VAT = findBoughtPrice.BoughtVAT,
                            Brutto = decimal.Parse(findBoughtPrice.BoughtBrutto),
                            Margin = 0,
                            Group = id_exists.GroupID.ToString()
                        });
                    }
                }
            }
            else if (name_exists != null)
            {
                if (name_exists.Amount == 0)
                {
                    name_exists.Amount = query.Amount - item.Amount;
                    name_exists.Netto = decimal.Parse(findBoughtPrice.BoughtNetto);
                    name_exists.Brutto = decimal.Parse(findBoughtPrice.BoughtBrutto);
                    name_exists.VAT = findBoughtPrice.BoughtVAT;
                }
                else
                {
                    var findRecords = Collections.WarehouseData.Where(x => x.Name == item.Name);
                    if (findRecords.Any(x => x.Brutto == decimal.Parse(findBoughtPrice.BoughtBrutto) && x.Netto == decimal.Parse(findBoughtPrice.BoughtNetto)))
                    {
                        var wh_ID = findRecords.FirstOrDefault(x => x.Brutto == decimal.Parse(findBoughtPrice.BoughtBrutto) && x.Netto == decimal.Parse(findBoughtPrice.BoughtNetto)).ID;
                        if (query.Amount == item.Amount)
                        {
                            warehouseitem.Warehouse.FirstOrDefault(x => x.idWarehouse == wh_ID).Amount += item.Amount;
                        }
                        else
                        {
                            warehouseitem.Warehouse.FirstOrDefault(x => x.idWarehouse == wh_ID).Amount += query.Amount - item.Amount;
                        }
                    }
                    else
                    {
                        await warehouseitem.AddItem(new Warehouse
                        {
                            Name = item.Name,
                            Amount = query.Amount != item.Amount ? query.Amount - item.Amount : item.Amount,
                            Netto = decimal.Parse(findBoughtPrice.BoughtNetto),
                            VAT = findBoughtPrice.BoughtVAT,
                            Brutto = decimal.Parse(findBoughtPrice.BoughtBrutto),
                            Margin = 0,
                            Group = name_exists.GroupID.ToString()
                        });
                    }
                }
            }
            else
            {
                await warehouseitem.AddItem(new Warehouse
                {
                    Name = item.Name,
                    Amount = query.Amount - item.Amount,
                    Netto = decimal.Parse(findBoughtPrice.BoughtNetto),
                    VAT = findBoughtPrice.BoughtVAT,
                    Brutto = decimal.Parse(findBoughtPrice.BoughtBrutto),
                    Margin = 0,
                    Group = Collections.GroupsData.FirstOrDefault(x => x.Type == 0).ID.ToString()
                });
            }
        }
        private void GenerateInvoice_Data()
        {
            _InvoiceData = new InvoiceData
            {
                InvoiceType = InvoiceType.Text,
                InvoiceNumber = InvoiceNumber.Text,
                BuyerID = Collections.ContactorsData[ContactorsList.SelectedIndex].ID,
                Payment = PurchaseType.Text,
                SellDate = SellDate.Text,
                InvoiceDate = CreatedDate.Text,
                Description = Description.Text,
                PaymentDays = Days_Amount.Text,
                BankName = BankName.Text,
                AccountNumber = BankAccount.Text,
                TotalPrice = ProductsValue.ToString(),
                PaidPrice = String.Format("{0:0.00}", decimal.Parse(Proform_Paid.Text)),
                Currency = CurrencyList.Text,
                Signature = Signature.Text
            };
        }
    }
}
