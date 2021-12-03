using Frontier.Methods.Numerics;
using Frontier.Variables;
using Frontier.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Frontier.Windows.Invoices_Window.Purchase_Window
{
    /// <summary>
    /// Logika interakcji dla klasy AddBought.xaml
    /// </summary>
    public partial class AddBought : Window
    {
        public AddBought()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    List<Warehouse_ViewModel> list = new List<Warehouse_ViewModel>();
                    foreach (var data in Collections.WarehouseData.Where(x => x.GroupType != "Usługa"))
                    {
                        if (!list.Any(x => x.Name == data.Name))
                        {
                            list.Add(data);
                        }
                    }
                    Products_Grid.ItemsSource = list;
                }));
            });
        }
        private void ProductSelection_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (Products_Grid.SelectedItem != null)
            {
                Warehouse_ViewModel data = (Warehouse_ViewModel)Products_Grid.SelectedItem;
                itemname.Text = data.Name;
            }
        }
        private void FindProduct(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text.Length == 0)
            {
                Products_Grid.ItemsSource = Collections.WarehouseData.Where(x => x.GroupType != "Usługa");
            }
            else
            {
                Products_Grid.ItemsSource = Collections.WarehouseData.Where(x => x.GroupType != "Usługa" && x.Name.ToLower().Contains(SearchBox.Text));
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
            var IsVATNumeric = decimal.TryParse(itemvat.Text.TrimEnd('%'), out decimal vat);
            if (IsVATNumeric && vat > 0)
            {
                if (((TextBox)sender).Name == "itembrutto" && Toggle_PriceType.IsChecked == true && itembrutto.Text != string.Empty)
                {
                    itemnetto.Text = Calculate.GetNetto(vat, decimal.Parse(itembrutto.Text)).ToString();
                }
                else if (((TextBox)sender).Name == "itemnetto" && Toggle_PriceType.IsChecked == false && itemnetto.Text != string.Empty)
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
        private void ResetAllFields()
        {
            itemnetto.Text = string.Empty;
            itembrutto.Text = string.Empty;
            itemcount.Text = string.Empty;
        }
        private void CheckNumeric(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex_Check.CheckNumbers(e.Text.Replace(" ", ""));
        }
        private void AddProduct_Clicked(object sender, RoutedEventArgs e)
        {
            if (itemname.Text != string.Empty && itemcount.Text != string.Empty && itembrutto.Text != string.Empty && itemnetto.Text != string.Empty)
            {
                if (int.Parse(itemcount.Text) == 0)
                {
                    return;
                }

                var item = new ProductsSold_ViewModel
                {
                    Name = itemname.Text,
                    Amount = int.Parse(itemcount.Text),
                    PieceNetto = decimal.Parse(itemnetto.Text),
                    VAT = itemvat.Text.TrimEnd('%'),
                    PieceBrutto = decimal.Parse(itembrutto.Text),
                    Netto = int.Parse(itemcount.Text) * decimal.Parse(itemnetto.Text),
                    VATAmount = (int.Parse(itemcount.Text) * decimal.Parse(itembrutto.Text)) - (decimal.Parse(itemcount.Text) * decimal.Parse(itemnetto.Text)),
                    Brutto = int.Parse(itemcount.Text) * decimal.Parse(itembrutto.Text),
                    GTU = Collections.GroupsData.FirstOrDefault(x => x.ID == ((Warehouse_ViewModel)Products_Grid.SelectedItem).GroupID).GTU,
                    GroupType = ((Warehouse_ViewModel)Products_Grid.SelectedItem).GroupType
                };

                if (GlobalVariables.InvoicePage != "Correction")
                {
                    var exists = Collections.ProductsBought.FirstOrDefault(x => x.Name == item.Name && x.PieceNetto == item.PieceNetto && x.PieceBrutto == item.PieceBrutto);
                    if (exists != null)
                    {
                        exists.Amount += item.Amount;
                        exists.Brutto = exists.PieceBrutto * exists.Amount;
                        exists.Netto = exists.PieceNetto * exists.Amount;
                        exists.VATAmount = exists.Brutto - exists.Netto;
                    }
                    else
                    {
                        Collections.ProductsBought.Add(item);
                    }
                }
                else
                {
                    var exists = Collections.Products_Correction.FirstOrDefault(x => x.Name == item.Name && x.PieceNetto == item.PieceNetto && x.PieceBrutto == item.PieceBrutto);
                    if (exists != null)
                    {
                        exists.Amount += item.Amount;
                        exists.Brutto = exists.PieceBrutto * exists.Amount;
                        exists.Netto = exists.PieceNetto * exists.Amount;
                        exists.VATAmount = exists.Brutto - exists.Netto;
                    }
                    else
                    {
                        Collections.Products_Correction.Add(item);
                    }
                }
                ResetAllFields();
            }
            else
            {
                MessageBox.Show("Proszę wypełnić wymagane pola");
            }
        }
    }
}
