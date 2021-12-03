using Frontier.Methods.Numerics;
using Frontier.Variables;
using Frontier.ViewModels;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Frontier.Windows.Invoices_Window.Adjustment_Window.Edit_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Edit.xaml
    /// </summary>
    public partial class Edit : Window
    {
        private ProductsSold_ViewModel _Product { get; set; }
        private int _SelectedSearchType { get; set; }
        public Edit(ProductsSold_ViewModel product, int SelectedSearchType)
        {
            InitializeComponent();
            _Product = product;
            _SelectedSearchType = SelectedSearchType;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillData();
        }
        private void FillData()
        {
            itemvat.SelectedIndex = itemvat.Items.IndexOf(itemvat.Items.OfType<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString().TrimEnd('%') == _Product.VAT));
            itemname.Text = _Product.Name;
            itemcount.Text = _Product.Amount.ToString();
            itemnetto.Text = _Product.PieceNetto.ToString();
            itembrutto.Text = _Product.PieceBrutto.ToString();
        }
        private void CheckNumeric(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex_Check.CheckNumbers(e.Text);
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
            else
            {
                if (Regex.IsMatch(((TextBox)sender).Text.Insert(((TextBox)sender).SelectionStart, e.Text), @"\,\d\d\d"))
                {
                    e.Handled = true;
                    MessageBox.Show("Nie można dodać wiecej niz dwie cyfry po przecinku");
                }
            }
        }
        private void CalculatePrice(object sender, RoutedEventArgs e)
        {
            try
            {
                var IsVATNumeric = decimal.TryParse(itemvat.Text.TrimEnd('%'), out decimal vat);
                if (IsVATNumeric && vat > 0)
                {
                    if (((TextBox)sender).Name == "itembrutto" && Toggle_PriceType.IsChecked == true)
                    {
                        itemnetto.Text = Calculate.GetNetto(vat, decimal.Parse(itembrutto.Text)).ToString();
                    }
                    else if (((TextBox)sender).Name == "itemnetto" && Toggle_PriceType.IsChecked == false)
                    {
                        itembrutto.Text = Calculate.GetBrutto(vat, decimal.Parse(itemnetto.Text)).ToString();
                    }
                }
                else
                {
                    if (((TextBox)sender).Name == "itembrutto" && Toggle_PriceType.IsChecked == true)
                    {
                        itemnetto.Text = itembrutto.Text;
                    }
                    else if (((TextBox)sender).Name == "itemnetto" && Toggle_PriceType.IsChecked == false)
                    {
                        itembrutto.Text = itemnetto.Text;
                    }
                }
            }
            catch (Exception) { }
        }
        private void ToggleChecked(object sender, RoutedEventArgs e)
        {
            itemnetto.IsHitTestVisible = false;
            itemnetto.Text = string.Empty;
            itembrutto.IsHitTestVisible = true;
        }
        private void ToggleUnchecked(object sender, RoutedEventArgs e)
        {
            itembrutto.IsHitTestVisible = false;
            itembrutto.Text = string.Empty;
            itemnetto.IsHitTestVisible = true;
        }
        private void ResetPriceFields(object sender, RoutedEventArgs e)
        {
            if (itemnetto != null && itembrutto != null)
            {
                itemnetto.Text = string.Empty;
                itembrutto.Text = string.Empty;
            }
        }
        private void EditProduct_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var whID_item = Collections.WarehouseData.FirstOrDefault(x => x.ID == _Product.ID && x.Brutto == _Product.PieceBrutto && x.Netto == _Product.PieceNetto);
                var whName_item = Collections.WarehouseData.FirstOrDefault(x => x.Name == _Product.Name && x.Brutto == _Product.PieceBrutto && x.Netto == _Product.PieceNetto);

                if (_SelectedSearchType == 1 && whID_item == null && whName_item == null)
                { MessageBox.Show("Produkt nie istnieje w magazynie"); return; }

                if (_SelectedSearchType == 1 && (whID_item?.Amount < _Product.Amount - int.Parse(itemcount.Text) || whName_item?.Amount < _Product.Amount - int.Parse(itemcount.Text)))
                { MessageBox.Show("Odjęta ilość produktu nie istnieje w magazynie"); return; }

                _Product.VAT = itemvat.Text.TrimEnd('%');
                _Product.Amount = int.Parse(itemcount.Text);
                _Product.PieceBrutto = decimal.Parse(itembrutto.Text);
                _Product.PieceNetto = decimal.Parse(itemnetto.Text);
                _Product.Netto = _Product.PieceNetto * _Product.Amount;
                _Product.Brutto = _Product.PieceBrutto * _Product.Amount;
                _Product.VATAmount = _Product.Brutto - _Product.Netto;
                this.Close();
            }
            catch (Exception) { MessageBox.Show("Nie udało sie zedytować produktu"); }
        }
        private void UpNumber_Clicked(object sender, RoutedEventArgs e)
        {
            if (int.Parse(itemcount.Text) + 1 <= _Product.Amount)
            {
                itemcount.Text = (int.Parse(itemcount.Text) + 1).ToString();
            }
        }
        private void DownNumber_Clicked(object sender, RoutedEventArgs e)
        {
            if (int.Parse(itemcount.Text) - 1 > 0)
            {
                itemcount.Text = (int.Parse(itemcount.Text) - 1).ToString();
            }
        }
    }
}
