using Frontier.Classes;
using Frontier.Database.GetQuery;
using Frontier.Methods.Invoices;
using Frontier.Variables;
using Frontier.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Frontier.Windows.Invoices_Window.Archive_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Archiwum.xaml
    /// </summary>
    public partial class Archive : Page, INotifyPropertyChanged
    {
        private decimal _TotalPrice_Netto { get; set; }
        public decimal TotalPrice_Netto
        {
            get { return _TotalPrice_Netto; }
            set
            {
                if (_TotalPrice_Netto == value) return;
                _TotalPrice_Netto = value;
                NotifyPropertyChanged("TotalPrice_Netto");
            }
        }
        private decimal _TotalPrice_Brutto { get; set; }
        public decimal TotalPrice_Brutto
        {
            get { return _TotalPrice_Brutto; }
            set
            {
                if (_TotalPrice_Brutto == value) return;
                _TotalPrice_Brutto = value;
                NotifyPropertyChanged("TotalPrice_Brutto");
            }
        }
        private int _InvoicesAmount { get; set; }
        public int InvoicesAmount
        {
            get { return _InvoicesAmount; }
            set
            {
                if (_InvoicesAmount == value) return;
                _InvoicesAmount = value;
                NotifyPropertyChanged("InvoicesAmount");
            }
        }

        public Archive()
        {
            InitializeComponent();
            LoadArchive();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private async void LoadArchive()
        {
            try
            {
                Archive_Grid.ItemsSource = Collections.Archive_Invoices;
                var now = DateTime.Now;
                var first = new DateTime(now.Year, now.Month, 1);
                var last = first.AddMonths(1).AddDays(-1);
                BeginDate.Text = first.ToShortDateString();
                EndDate.Text = last.ToShortDateString();
                await DownloadInvoices();
            }
            catch (Exception)
            {
                Collections.Archive_Invoices.Clear();
                MessageBox.Show("Nie można załadować archiwum");
            }
        }
        private void SelectionDate_Clicked(object sender, SelectionChangedEventArgs e)
        {
            var senderName = ((DatePicker)sender).Name;
            if (senderName == "From_Date")
            {
                if (From_Date.SelectedDate.Value.Date > Convert.ToDateTime(EndDate.Text))
                {
                    MessageBox.Show("Nie można ustawić początkowej daty pózniejszej niż końcowa");
                    return;
                }
            }
            else
            {
                if (Until_Date.SelectedDate.Value.Date < Convert.ToDateTime(BeginDate.Text))
                {
                    MessageBox.Show("Nie można ustawić końcowej daty wcześniejszej niż początkowa");
                    return;
                }
            }
            _ = senderName == "From_Date" ? BeginDate.Text = From_Date.SelectedDate.Value.Date.ToShortDateString() : EndDate.Text = Until_Date.SelectedDate.Value.Date.ToShortDateString();
        }
        private void Search_Invoice(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SearchBox.Text.Length == 0)
                {
                    Archive_Grid.ItemsSource = Collections.Archive_Invoices;
                }
                else
                {
                    if (SearchType.SelectedIndex == 0)
                    {
                        Archive_Grid.ItemsSource = Collections.Archive_Invoices.Where(x => x.InvoiceID.ToLower().Contains(SearchBox.Text.ToLower()));
                    }
                    else
                    {
                        Archive_Grid.ItemsSource = Collections.Archive_Invoices.Where(x => x.Contactor.ToLower().Contains(SearchBox.Text.ToLower()));
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas wyszukiwania");
            }
        }
        private async void OpenPreview(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
            await Task.Run(async () =>
            {
                await Dispatcher.BeginInvoke(new Action(async () =>
                {
                    try
                    {
                        Button button = sender as Button;
                        var item = (sender as FrameworkElement).DataContext;
                        var index = Archive_Grid.Items.IndexOf(item);
                        var id = Archive_Grid.Columns[0].GetCellContent(Archive_Grid.Items[index]) as TextBlock;
                        var invoicetype = Archive_Grid.Columns[1].GetCellContent(Archive_Grid.Items[index]) as TextBlock;

                        InvoiceData getInvoice = await Download_Invoice.GetData(invoicetype.Text, int.Parse(id.Text));
                        await Generate_Invoice.CreateInvoice(getInvoice, "InvoicePreview.pdf");
                        ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;

                        PdfViewer viewer = new PdfViewer();
                        viewer.Owner = Application.Current.MainWindow;
                        viewer.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        viewer.ShowDialog();
                    }
                    catch (Exception)
                    {
                        ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                        MessageBox.Show("Wystąpił błąd podczas operacji");
                    }
                }));
            });
        }
        private async void GetInvoices(object sender, RoutedEventArgs e)
        {
            try
            {
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                await DownloadInvoices();
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
            }
            catch (Exception)
            {
                MessageBox.Show("Błąd podczas filtrowania");
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
            }
        }
        private async Task DownloadInvoices()
        {
            TotalPrice_Brutto = 0;
            TotalPrice_Netto = 0;
            await Task.Run(async () =>
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    using (GetInvoices invoices = new GetInvoices())
                    {
                        Collections.Archive_Invoices.Clear();
                        var begindate = Convert.ToDateTime(BeginDate.Text);
                        var enddate = Convert.ToDateTime(EndDate.Text);
                        var sold = invoices.Invoice_Sold.Where(x => x.Date_Created >= begindate && x.Date_Created <= enddate);

                        using (GetInvoice_Products products = new GetInvoice_Products())
                        {
                            if (sold != null)
                            {
                                foreach (var data in sold)
                                {
                                    var soldproduct = products.Invoice_Products.Where(x => x.Invoice_ID == data.Invoice_ID && x.Invoice == data.idInvoice_Sold);
                                    decimal totalnetto = 0;
                                    decimal totalbrutto = 0;
                                    decimal totalvat = 0;

                                    foreach (var product in soldproduct)
                                    {
                                        totalnetto += decimal.Parse(product.Netto);
                                        totalbrutto += decimal.Parse(product.Brutto);
                                        totalvat += decimal.Parse(product.VAT_Price);
                                    }

                                    var item = new ArchiveGrid_ViewModel
                                    {
                                        ID = data.idInvoice_Sold,
                                        InvoiceID = data.Invoice_ID,
                                        InvoiceType = data.Invoice_Type,
                                        Contactor = Collections.ContactorsData.FirstOrDefault(x => x.ID == int.Parse(data.Receiver)).Name,
                                        ContactorNIP = Collections.ContactorsData.FirstOrDefault(x => x.ID == int.Parse(data.Receiver)).NIP,
                                        Created_Date = data.Date_Created,
                                        Netto = totalnetto.ToString(),
                                        VATAmount = totalvat.ToString(),
                                        Brutto = totalbrutto.ToString(),
                                        Currency = data.Currency
                                    };
                                    Collections.Archive_Invoices.Add(item);
                                    TotalPrice_Brutto += data.Currency == "PLN" ? totalbrutto : Math.Round(totalbrutto * decimal.Parse(data.ExchangeRate), 2);
                                    TotalPrice_Netto += data.Currency == "PLN" ? totalnetto : Math.Round(totalnetto * decimal.Parse(data.ExchangeRate), 2);

                                }
                            }
                        }
                        Collections.Archive_Invoices.OrderByDescending(x => x.Created_Date);
                        InvoicesAmount = Collections.Archive_Invoices.Count;
                    }
                }));
            });
        }
    }
}
