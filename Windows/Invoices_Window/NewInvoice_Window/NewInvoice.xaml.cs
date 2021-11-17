using Frontier.Database.GetQuery;
using Frontier.Methods;
using Frontier.Variables;
using Frontier.ViewModels;
using Frontier.Windows.Confirmation_Window;
using Frontier.Windows.Invoices_Window.AddProduct_Window;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System.Drawing;
using Frontier.Database.TableClasses;
using System.Globalization;

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
        public IEnumerable<Warehouse_ViewModel> InvoiceProducts { get; set; }
        public class InvoiceData
        {
            public string InvoiceType { get; set; }
            public string InvoiceNumber { get; set; }
            public int BuyerID { get; set; }
            public string Payment { get; set; }
            public string SellDate { get; set; }
            public string InvoiceDate { get; set; }
            public string Description { get; set; }
            public string PaymentDays { get; set; }
            public string AccountNumber { get; set; }
            public string BankName { get; set; }
            public string TotalPrice { get; set; }
            public string PaidPrice { get; set; }
            public string Currency { get; set; }
        }
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
        private async void LoadInvoice()
        {
            await Task.Run(async () =>
            {
                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    InvoiceProducts = Collections.WarehouseData;
                    ContactorsList.ItemsSource = Collections.ContactorsData;
                    Products_Grid.ItemsSource = Collections.ProductsSold;
                    ProductsValue = 0.00m;
                    GetLastInvoice();
                }));
            });
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
        private void GetLastInvoice()
        {
            using (GetInvoice_Sold getLastID = new GetInvoice_Sold())
            {
                var year = DateTime.Now.Year;
                var Invoices = getLastID.Invoice_Sold.Where(x => x.Date_Created.Year == year).OrderByDescending(x => x.idInvoice_Sold).FirstOrDefault() != null ? getLastID.Invoice_Sold.Where(x => x.Date_Created.Year == year).OrderByDescending(x => x.idInvoice_Sold).Count() : 0;
                string newInvoiceID = (Invoices + 1).ToString("D6");
                InvoiceNumber.Text = "F/" + newInvoiceID + "/" + DateTime.Now.Year.ToString();
            }
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
        }
        private void SelectionDate_Clicked(object sender, SelectionChangedEventArgs e)
        {
            var senderName = ((DatePicker)sender).Name;
            if(senderName == "Created_Date" && SellDate_Text.Text != string.Empty)
            {
                if (Convert.ToDateTime(SellDate_Text.Text) > Created_Date.SelectedDate.Value.Date)
                {
                    MessageBox.Show("Data wystawienia faktury nie może być wcześniejsza od daty sprzedaży");
                    return;
                }
            }
            else if(senderName == "Sell_Date" && CreatedDate_Text.Text != string.Empty)
            {
                if (Convert.ToDateTime(CreatedDate_Text.Text) < Sell_Date.SelectedDate.Value.Date)
                {
                    MessageBox.Show("Data wystawienia faktury nie może być wcześniejsza od daty sprzedaży");
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
                if(SavedInvoice)
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
        private void AddProduct_Clicked(object sender, RoutedEventArgs e)
        {
            AddProduct products = new AddProduct(InvoiceProducts, InvoiceType.Text);
            products.Owner = Application.Current.MainWindow;
            products.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            products.ShowDialog();
        }
        private async void DeleteRows()
        {
            await Task.Run(async () =>
            {
                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    List<int[]> ids = new List<int[]>();
                    foreach (ProductsSold_ViewModel data in Products_Grid.SelectedItems)
                    {
                        ids.Add(new int[] { data.ID, data.Amount });
                    }

                    foreach (int[] data in ids)
                    {
                        if (InvoiceProducts.FirstOrDefault(x => x.ID == data[0]).GroupType != "Usługa")
                        {
                            InvoiceProducts.FirstOrDefault(x => x.ID == data[0]).Amount += data[1];
                        }
                        Collections.ProductsSold.Remove(Collections.ProductsSold.Where(x => x.ID == data[0]).FirstOrDefault());
                    }
                }));
            });
        }
        private async void SaveInvoice()
        {
            try
            {
                await Task.Run(async () =>
                {
                    await Dispatcher.BeginInvoke(new Action(async () =>
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
                                    BankName = Bankname.Text
                                };
                                var updated = await invoice.AddInvoice(item);
                                if (updated)
                                {
                                    using (GetInvoice_Products query = new GetInvoice_Products())
                                    {
                                        using (GetWarehouse sold_item = new GetWarehouse())
                                        {
                                            foreach (ProductsSold_ViewModel product in Products_Grid.Items)
                                            {
                                                var new_product = new Invoice_Products
                                                {
                                                    Invoice_ID = InvoiceNumber.Text,
                                                    Product_ID = product.ID.ToString(),
                                                    Name = product.Name,
                                                    Amount = product.Amount.ToString(),
                                                    Each_Netto = product.PieceNetto.ToString(),
                                                    VAT = product.VAT,
                                                    Each_Brutto = product.PieceBrutto.ToString(),
                                                    Netto = product.Netto.ToString(),
                                                    VAT_Price = product.VATAmount.ToString(),
                                                    Brutto = product.Brutto.ToString(),
                                                };
                                                await query.AddProduct(new_product);

                                                if (product.GroupType != "Usługa")
                                                {
                                                    var solditem_warehouse = new Warehouse
                                                    {
                                                        idWarehouse = product.ID,
                                                        Name = product.Name,
                                                        Amount = product.Amount,
                                                        Netto = product.PieceNetto,
                                                        Brutto = product.PieceBrutto
                                                    };
                                                    await sold_item.SoldWarehouse_Item(solditem_warehouse);
                                                    await sold_item.SaveChangesAsync();
                                                }
                                            }
                                            await query.SaveChangesAsync();
                                            await invoice.SaveChangesAsync();
                                        }
                                    }
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
                    }));
                });
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas zapisu faktury");
            }
        }
        private async void NewInvoice_Clicked(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                await Dispatcher.BeginInvoke(new Action(async() => 
                {
                    ContactorsList.SelectedIndex = -1;
                    PurchaseType.SelectedIndex = -1;
                    CurrencyList.SelectedIndex = 0;
                    InvoiceType.SelectedIndex = 0;
                    InvoiceNumber.Text = string.Empty;
                    SellDate_Text.Text = string.Empty;
                    CreatedDate_Text.Text = string.Empty;
                    Description.Text = string.Empty;
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
                if (InvoiceProducts.FirstOrDefault(x => x.ID == data[0]).GroupType != "Usługa")
                {
                    InvoiceProducts.FirstOrDefault(x => x.ID == data[0]).Amount += data[1];
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

                System.Windows.Forms.SaveFileDialog exportSaveFileDialog = new System.Windows.Forms.SaveFileDialog();

                exportSaveFileDialog.Title = "Select Destination";
                exportSaveFileDialog.Filter = "PDF(*.pdf)|*.pdf";

                if (System.Windows.Forms.DialogResult.OK == exportSaveFileDialog.ShowDialog())
                {
                    ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                    await Generate_Invoice.CreateInvoice(_InvoiceData, exportSaveFileDialog.FileName);
                    ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                }
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

                System.Windows.Forms.SaveFileDialog exportSaveFileDialog = new System.Windows.Forms.SaveFileDialog();

                exportSaveFileDialog.Title = "Select Destination";
                exportSaveFileDialog.Filter = "Word(*.docx)|*.docx";

                if (System.Windows.Forms.DialogResult.OK == exportSaveFileDialog.ShowDialog())
                {
                    ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                    await Generate_Invoice.CreateInvoice(_InvoiceData, AppDomain.CurrentDomain.BaseDirectory + "\\Invoice.pdf");
                    await Generate_Invoice.ConvertWord(AppDomain.CurrentDomain.BaseDirectory, exportSaveFileDialog.FileName);
                    ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                }
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

                using (System.Windows.Forms.PrintDialog dialogPrint = new System.Windows.Forms.PrintDialog())
                {
                    if (System.Windows.Forms.DialogResult.OK == dialogPrint.ShowDialog())
                    {
                        ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                        await Task.Run(async () =>
                        {
                            await Generate_Invoice.CreateInvoice(_InvoiceData, AppDomain.CurrentDomain.BaseDirectory + "\\Invoice.pdf");

                            PdfDocument pdf = new PdfDocument();
                            pdf.LoadFromFile(AppDomain.CurrentDomain.BaseDirectory + "\\Invoice.pdf");

                            PdfMargins pdfMargin = new PdfMargins();
                            pdfMargin.Left = 0;
                            pdfMargin.Right = 0;
                            pdfMargin.Top = 0;
                            pdfMargin.Bottom = 0;

                            PdfDocument doc1 = new PdfDocument();
                            foreach (PdfPageBase page in pdf.Pages)
                            {
                                SizeF size = page.Size;
                                PdfPageBase p = doc1.Pages.Add(new SizeF(size.Width + 50F, size.Height + 100F), pdfMargin);
                                page.CreateTemplate().Draw(p, 15, 0);
                            }
                            doc1.PrintSettings.PrinterName = dialogPrint.PrinterSettings.PrinterName;
                            doc1.Print();

                            await PermaDelete.Delete(AppDomain.CurrentDomain.BaseDirectory, "Invoice.pdf");
                        });
                        ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Błąd podczas drukowania");
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
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
                    if (decimal.Parse(Proform_Paid.Text + e.Text) > ProductsValue)
                    {
                        e.Handled = true;
                        MessageBox.Show("Wartość zapłacona nie może być większa niż wartość produktów");
                    }
                }
                else if(((TextBox)sender).Text.Length == 0 && int.Parse(e.Text) > 0)
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
            if(!decimal.TryParse(Proform_Paid.Text, out decimal paid))
            {
                Proform_Paid.Text = "0";
            }
        }
    }
}
