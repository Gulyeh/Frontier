using Frontier.Methods;
using Frontier.Variables;
using Frontier.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Frontier.Windows.Invoices_Window.AddProduct_Window
{
    public partial class AddProduct : Window
    {
        private int oldVAT { get; set; }
        private string oldBrutto { get; set; }
        public IEnumerable<Warehouse_ViewModel> InvoiceProducts { get; set; }
        private string _InvoiceType { get; set; }

        public AddProduct(IEnumerable<Warehouse_ViewModel> InvoiceProducts, string InvoiceType)
        {
            InitializeComponent();
            if(InvoiceType == "VAT Marża")
            {
                this.InvoiceProducts = InvoiceProducts.Where(x => x.Amount > 0 && x.GroupType != "Usługa");
            }
            else
            {
                this.InvoiceProducts = InvoiceProducts.Where(x => x.Amount > 0);
            }

            Products_Grid.ItemsSource = this.InvoiceProducts;
            _InvoiceType = InvoiceType;
        }

        private void ProductSelection_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (Products_Grid.SelectedItem != null)
            {
                itemvat.SelectedIndex = -1;
                UnlockFields();
                Warehouse_ViewModel data = (Warehouse_ViewModel)Products_Grid.SelectedItem;
                itemname.Text = data.Name;
                itemmargin.Text = data.Margin.ToString();
                itemnetto.Text = data.Netto.ToString();
                oldBrutto = data.Brutto.ToString();
                itembrutto.Text = oldBrutto;
                oldVAT = itemvat.Items.IndexOf(itemvat.Items.OfType<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString().Contains(data.VAT)));
                itemvat.SelectedIndex = itemvat.Items.IndexOf(itemvat.Items.OfType<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString().Contains(Collections.GroupsData.FirstOrDefault(z => z.ID == data.GroupID).VAT)));
            }
        }
        private void CheckPrice(object sender, TextCompositionEventArgs e)
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
        }
        private void CheckNumeric(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex_Check.CheckNumbers(e.Text.Replace(" ",""));
        }
        private void RecalculateNetto(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is ComboBox)
                {
                    if (itemvat.SelectedIndex != oldVAT && itemvat.SelectedIndex > -1 && itembrutto.Text.Length > 0 && itemnetto.Text.Length > 0)
                    {
                        if (decimal.TryParse(((ComboBoxItem)itemvat.SelectedItem).Content.ToString().Replace("%", ""), out decimal parsed))
                        {
                            itemnetto.Text = Calculate.GetNetto(parsed, decimal.Parse(itembrutto.Text)).ToString();
                        }
                        else
                        {
                            itemnetto.Text = itembrutto.Text;
                        }
                    }
                    oldVAT = itemvat.SelectedIndex;
                }
                else
                {
                    if (itembrutto.Text != oldBrutto && itembrutto.Text.Length > 0)
                    {
                        itemnetto.Text = Calculate.GetNetto(decimal.Parse(itemvat.Text.Replace("%", "")), decimal.Parse(itembrutto.Text)).ToString();
                    }
                    else if (itembrutto.Text.Length == 0)
                    {
                        itembrutto.Text = string.Empty;
                        itemnetto.Text = string.Empty;
                    }
                    oldBrutto = itembrutto.Text;
                }
            }
            catch (Exception) { }
        }
        private void FindProduct(object sender, RoutedEventArgs e)
        {
            if(SearchBox.Text.Length == 0)
            {
                Products_Grid.ItemsSource = InvoiceProducts;
            }
            else
            {
                Products_Grid.ItemsSource = InvoiceProducts.Where(x => x.Name.ToLower().Contains(SearchBox.Text));
            }
        }
        private void AddProduct_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (itemname.Text != string.Empty && int.Parse(itemcount.Text) > 0 && itembrutto.Text != string.Empty && itemnetto.Text != string.Empty && itemmargin.Text != string.Empty)
                {
                    if (_InvoiceType == "VAT Marża" && itemvat.Text != "23%" && itemvat.Text != "0%")
                    {
                        MessageBox.Show("VAT Marża nie przyjmuje takiego podatku VAT (23% lub 0%)");
                        return;
                    }

                    Warehouse_ViewModel data = (Warehouse_ViewModel)Products_Grid.SelectedItem;
                    if(decimal.Parse(itembrutto.Text) < Collections.WarehouseData.FirstOrDefault(x => x.ID == data.ID).Brutto && data.GroupType != "Usługa")
                    {
                        MessageBox.Show("UWAGA! Sprzedajesz towar za mniejszą wartość niż został zakupiony");
                    }

                    if (int.Parse(itemcount.Text) <= data.Amount || data.GroupType == "Usługa")
                    {
                        var piecenetto = Math.Round(int.Parse(itemmargin.Text) > 0 ?
                            decimal.Parse(itemnetto.Text) + (decimal.Parse(itemnetto.Text) * (decimal.Parse(itemmargin.Text) / 100)) :
                            decimal.Parse(itemnetto.Text), 2);
                        var piecebrutto = Math.Round(int.Parse(itemmargin.Text) > 0 ?
                            decimal.Parse(itembrutto.Text) + (decimal.Parse(itembrutto.Text) * (decimal.Parse(itemmargin.Text) / 100)) :
                            decimal.Parse(itembrutto.Text), 2);

                        var new_data = new ProductsSold_ViewModel()
                        {
                            ID = data.ID,
                            Name = data.Name,
                            GroupType = data.GroupType,
                            VAT = itemvat.Text.Replace("%", ""),
                            Amount = int.Parse(itemcount.Text),
                            PieceNetto = decimal.Parse(String.Format("{0:0.00}", piecenetto)),
                            PieceBrutto = decimal.Parse(String.Format("{0:0.00}", piecebrutto)),
                            Brutto = decimal.Parse(String.Format("{0:0.00}", Math.Round(int.Parse(itemcount.Text) * piecebrutto, 2))),
                            Netto = decimal.Parse(String.Format("{0:0.00}", Math.Round(int.Parse(itemcount.Text) * piecenetto, 2))),
                            VATAmount = _InvoiceType != "VAT Marża" ?
                            decimal.Parse(String.Format("{0:0.00}", Math.Round(int.Parse(itemcount.Text) * piecebrutto, 2) - Math.Round(int.Parse(itemcount.Text) * piecenetto, 2))) : 0,
                            Margin = int.Parse(itemmargin.Text),
                            GTU = Collections.GroupsData.FirstOrDefault(x => x.ID == data.GroupID).GTU
                        };

                        if(_InvoiceType == "VAT Marża")
                        {
                            new_data.VATAmount = Calculate.GetMarginVAT(decimal.Parse(new_data.VAT), Collections.WarehouseData.FirstOrDefault(x => x.ID == new_data.ID).Brutto * new_data.Amount, new_data.Brutto);
                        }

                        Collections.ProductsSold.Add(new_data);
                        if (data.GroupType != "Usługa")
                        {
                            InvoiceProducts.FirstOrDefault(x => x.ID == data.ID).Amount -= new_data.Amount;
                        }
                        Products_Grid.ItemsSource = InvoiceProducts.Where(x => x.Amount > 0);
                        ResetFields();
                    }
                    else
                    {
                        MessageBox.Show("W magazynie nie widnieje podana ilość produktu");
                    }
                }
                else
                {
                    MessageBox.Show("Proszę uzupełnić wymagane dane");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Nie można dodać produktu");
            }
        }
        private void UnlockFields()
        {
            itemcount.IsHitTestVisible = true;
            itemvat.IsHitTestVisible = true;
            itemmargin.IsHitTestVisible = true;
            itembrutto.IsHitTestVisible = true;
            AddProduct_Button.IsEnabled = true;
        }
        private void ResetFields()
        {
            itemcount.IsHitTestVisible = false;
            itemvat.IsHitTestVisible = false;
            itemmargin.IsHitTestVisible = false;
            itembrutto.IsHitTestVisible = false;
            AddProduct_Button.IsEnabled = false;
            itemcount.Text = string.Empty;
            itemname.Text = string.Empty;
            itemmargin.Text = string.Empty;
            itemnetto.Text = string.Empty;
            itembrutto.Text = string.Empty;
            itemvat.SelectedIndex = -1;
        }
    }
}
