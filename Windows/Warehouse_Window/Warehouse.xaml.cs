using Frontier.Database.GetQuery;
using Frontier.Methods;
using Frontier.Methods.Numerics;
using Frontier.Variables;
using Frontier.ViewModels;
using Frontier.Windows.Confirmation_Window;
using Frontier.Windows.Groups_Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Frontier.Windows.Warehouse_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Magazyn.xaml
    /// </summary>
    public partial class Warehouse : Page
    {
        private int ItemID { get; set; }
        private int LastIndex { get; set; }
        private string SelectedGroupType { get; set; }
        ListCollectionView collection;
        Timer timer;

        public Warehouse()
        {
            InitializeComponent();
            LoadWarehouse();
        }
        private async void LoadWarehouse()
        {
            warehousegroups.ItemsSource = Collections.GroupsData;
            editgroup.ItemsSource = Collections.GroupsData;
            collection = new ListCollectionView(Collections.WarehouseData);
            collection.GroupDescriptions.Add(new PropertyGroupDescription("Name"));
            Warehouse_Grid.ItemsSource = collection;
            timer = new Timer();
            timer.Interval = 500;
            timer.Elapsed += OnElapsed;
            await Task.Run(async () =>
            {
                await this.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    try
                    {
                        using (GetGroups groups = new GetGroups())
                        {
                            var query = groups.Groups;
                            foreach (var data in query)
                            {
                                Collections.GroupsData.Add(new Groups_ViewModel
                                {
                                    ID = data.idgroups,
                                    Name = data.Name,
                                    Description = data.Description,
                                    GTU = data.GTU,
                                    VAT = data.VAT,
                                    Type = data.Type
                                });
                            }
                        }
                        await Download_WarehouseItems.Download();
                    }
                    catch (Exception)
                    {
                        Collections.GroupsData.Clear();
                        LoadWarehouse();
                    }
                }));
            });
        }
        private async void AddItem_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (itemname.Text != string.Empty && itemcount.Text != string.Empty && itembrutto.Text != string.Empty && warehousegroups.SelectedIndex > -1)
                {
                    var checkName = Collections.WarehouseData.FirstOrDefault(x => x.Name == itemname.Text);
                    if (checkName != null) { MessageBox.Show("Taka nazwa istnieje już w magazynie"); return; }
                    using (GetWarehouse add_itemWarehouse = new GetWarehouse())
                    {
                        var isNumericMargin = int.TryParse(itemmargin.Text, out int margin);
                        var isNumericVAT = decimal.TryParse(Collections.GroupsData[warehousegroups.SelectedIndex].VAT, out decimal vat);

                        var data = new Database.TableClasses.Warehouse()
                        {
                            Name = itemname.Text,
                            Group = Collections.GroupsData[warehousegroups.SelectedIndex].ID.ToString(),
                            Amount = int.Parse(itemcount.Text),
                            Brutto = Math.Round(decimal.Parse(itembrutto.Text), 2),
                            Margin = isNumericMargin == true ? margin : 0,
                            Netto = isNumericVAT == true ? Calculate.GetNetto(vat, decimal.Parse(itembrutto.Text)) : Math.Round(decimal.Parse(itembrutto.Text), 2),
                            VAT = Collections.GroupsData[warehousegroups.SelectedIndex].Type == 1 ? vat.ToString() : "0"
                        };

                        var update = await add_itemWarehouse.AddItem(data);
                        if (update)
                        {
                            add_itemWarehouse.SaveChanges();

                            var newItem = add_itemWarehouse.Warehouse.OrderByDescending(x => x.idWarehouse).FirstOrDefault();
                            var new_data = new Warehouse_ViewModel()
                            {
                                ID = newItem.idWarehouse,
                                Name = itemname.Text,
                                GroupName = Collections.GroupsData[warehousegroups.SelectedIndex].Name,
                                GroupID = Collections.GroupsData[warehousegroups.SelectedIndex].ID,
                                GroupType = Collections.GroupsData[warehousegroups.SelectedIndex].Type == 0 ? "Towar" : "Usługa",
                                Amount = int.Parse(itemcount.Text),
                                Brutto = data.Brutto,
                                Margin = data.Margin,
                                VAT = data.VAT,
                                Netto = data.Netto
                            };

                            Collections.WarehouseData.Add(new_data);
                            MessageBox.Show("Pomyślnie dodano nowy produkt");
                            ResetFillData("new");
                        }
                        else
                        {
                            throw new ArgumentNullException();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Proszę wypełnić wymagane pola");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas dodawania danych");
            }
        }
        private async void EditItem_Clicked(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
            await Task.Run(async () =>
            {
                await Dispatcher.BeginInvoke(new Action(async () =>
                {
                    try
                    {
                        if (editname.Text != string.Empty && editcount.Text != string.Empty && editbrutto.Text != string.Empty && editgroup.SelectedIndex > -1)
                        {
                            var checkName = Collections.WarehouseData.FirstOrDefault(x => x.Name == editname.Text);
                            if (checkName != null && Collections.WarehouseData.FirstOrDefault(x => x.ID == ItemID).Name != editname.Text)
                            {
                                editname.Text = Collections.WarehouseData.FirstOrDefault(x => x.ID == ItemID).Name;
                                MessageBox.Show("Taka nazwa istnieje już w magazynie");
                                return;
                            }

                            var isNumericMargin = int.TryParse(editmargin.Text, out int margin);
                            decimal NettoPrice = 0;
                            string VAT = "";

                            if (Collections.GroupsData[editgroup.SelectedIndex].Type == 1) // if jest usługa
                            {
                                VAT = Collections.GroupsData[editgroup.SelectedIndex].VAT;
                                var isNumericVAT = decimal.TryParse(Collections.GroupsData[editgroup.SelectedIndex].VAT, out decimal vat);
                                NettoPrice = isNumericVAT == true ? Calculate.GetNetto(vat, decimal.Parse(editbrutto.Text)) : editbrutto.Text.Contains(',') ? Math.Round(decimal.Parse(editbrutto.Text), 2) : Math.Round(decimal.Parse(editbrutto.Text + ",00"), 2);
                            }
                            else // if jest towar
                            {
                                if (int.Parse(editcount.Text) == 0)
                                {
                                    VAT = Collections.GroupsData[editgroup.SelectedIndex].VAT;
                                    NettoPrice = 0.00m;
                                }
                                else
                                {
                                    VAT = Collections.WarehouseData.Where(x => x.ID == ItemID).FirstOrDefault().VAT;
                                    NettoPrice = Collections.WarehouseData.Where(x => x.ID == ItemID).FirstOrDefault().Netto;
                                }
                            }

                            using (GetWarehouse edit_item = new GetWarehouse())
                            {
                                List<int> IDs = new List<int>();
                                foreach (var productsid in Collections.WarehouseData.Where(x => x.Name == Collections.WarehouseData.FirstOrDefault(z => z.ID == ItemID).Name))
                                {
                                    if (productsid.ID != ItemID)
                                    {
                                        IDs.Add(productsid.ID);
                                    }
                                }

                                var data = new Database.TableClasses.Warehouse()
                                {
                                    idWarehouse = ItemID,
                                    Name = editname.Text,
                                    Amount = int.Parse(editcount.Text),
                                    Group = Collections.GroupsData[editgroup.SelectedIndex].ID.ToString(),
                                    Brutto = editbrutto.Text.Contains(',') ? Math.Round(decimal.Parse(editbrutto.Text), 2) : Math.Round(decimal.Parse(editbrutto.Text + ",00"), 2),
                                    Margin = isNumericMargin == true ? margin : 0,
                                    Netto = NettoPrice,
                                    VAT = VAT
                                };
                                var update = await edit_item.EditItem(data);

                                if (update)
                                {
                                    var new_data = new Warehouse_ViewModel()
                                    {
                                        ID = ItemID,
                                        Name = editname.Text,
                                        GroupName = Collections.GroupsData[editgroup.SelectedIndex].Name,
                                        GroupID = Collections.GroupsData[editgroup.SelectedIndex].ID,
                                        GroupType = Collections.GroupsData.Where(x => x.ID == Int32.Parse(data.Group)).FirstOrDefault().Type == 0 ? "Towar" : "Usługa",
                                        Amount = data.Amount,
                                        Brutto = data.Brutto,
                                        Margin = data.Margin,
                                        VAT = VAT,
                                        Netto = data.Netto
                                    };

                                    Collections.WarehouseData[Collections.WarehouseData.IndexOf(Collections.WarehouseData.Where(x => x.ID == ItemID).FirstOrDefault())] = new_data;
                                    if (Collections.ProductsSold.Where(x => x.ID == ItemID).FirstOrDefault() != null) { Collections.ProductsSold.Where(x => x.ID == ItemID).FirstOrDefault().Name = editname.Text; }

                                    foreach (var productid in IDs)
                                    {
                                        var db_item = edit_item.Warehouse.Where(x => x.idWarehouse == productid).FirstOrDefault();
                                        var wh_item = Collections.WarehouseData.Where(x => x.ID == productid).FirstOrDefault();
                                        db_item.Group = data.Group;
                                        db_item.Name = data.Name;
                                        db_item.Margin = data.Margin;
                                        wh_item.Name = data.Name;
                                        wh_item.GroupID = Collections.GroupsData[editgroup.SelectedIndex].ID;
                                        wh_item.GroupName = Collections.GroupsData[editgroup.SelectedIndex].Name;
                                        wh_item.GroupType = Collections.GroupsData.Where(x => x.ID == Int32.Parse(data.Group)).FirstOrDefault().Type == 0 ? "Towar" : "Usługa";
                                        wh_item.Margin = data.Margin;
                                    }

                                    //Workaround for updating datagrid if user searched data
                                    if (SearchBox.Text.Length > 0)
                                    {
                                        var text = SearchBox.Text;
                                        SearchBox.Text = string.Empty;
                                        SearchBox.Text = text;
                                    }

                                    await edit_item.SaveChangesAsync();
                                    ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                                    MessageBox.Show("Pomyślnie zaktualizowano dane");
                                    Switch_EditFields(0, null);
                                    ResetFillData("edit");
                                }
                                else
                                {
                                    throw new ArgumentNullException();
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Proszę uzupełnić wymagane dane");
                        }
                    }
                    catch (Exception)
                    {
                        ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                        MessageBox.Show("Wystąpił błąd podczas aktualizacji danych");
                    }
                }));
            });
        }
        private void EditRow_Clicked(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            var item = (sender as FrameworkElement).DataContext;
            var index = Warehouse_Grid.Items.IndexOf(item);
            var id = Warehouse_Grid.Columns[0].GetCellContent(Warehouse_Grid.Items[index]) as TextBlock;
            ItemID = Int32.Parse(id.Text);
            var data = Collections.WarehouseData.Where(x => x.ID == ItemID).FirstOrDefault();
            FillEditWarehouse(data);
        }
        private void FillEditWarehouse(Warehouse_ViewModel data)
        {
            editname.Text = data.Name;
            editcount.Text = data.Amount.ToString();
            editbrutto.Text = data.Brutto.ToString();
            editmargin.Text = data.Margin.ToString();
            SelectedGroupType = data.GroupType;
            LastIndex = Collections.GroupsData.IndexOf(Collections.GroupsData.Where(x => x.ID == data.GroupID).FirstOrDefault());
            editgroup.SelectedIndex = LastIndex;
            Switch_EditFields(1, data);
        }
        private void ResetFillData(string data)
        {
            switch (data)
            {
                case "new":
                    itemname.Text = string.Empty;
                    itemcount.Text = string.Empty;
                    itemmargin.Text = string.Empty;
                    itembrutto.Text = string.Empty;
                    warehousegroups.SelectedIndex = -1;
                    break;
                case "edit":
                    editname.Text = string.Empty;
                    editcount.Text = string.Empty;
                    editmargin.Text = string.Empty;
                    editbrutto.Text = string.Empty;
                    editgroup.SelectedIndex = -1;
                    break;
                default:
                    break;
            }
        }
        private void CheckNumeric(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex_Check.CheckNumbers(e.Text);
        }
        private void CheckSpace(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
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
        private void Delete_Clicked(object sender, RoutedEventArgs e)
        {
            Confirmation confirm = new Confirmation("Warehouse");
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
                await this.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    try
                    {
                        List<int> ids = new List<int>();
                        foreach (Warehouse_ViewModel data in Warehouse_Grid.SelectedItems)
                        {
                            if (data.GroupType == "Usługa" || data.Amount == 0)
                            {
                                ids.Add(data.ID);
                            }
                        }

                        using (GetWarehouse delete_item = new GetWarehouse())
                        {
                            foreach (int data in ids)
                            {
                                bool updated = delete_item.DeleteItem(data);
                                if (updated)
                                {
                                    Collections.WarehouseData.Remove(Collections.WarehouseData.Where(x => x.ID == data).FirstOrDefault());
                                }
                            }
                            await delete_item.SaveChangesAsync();
                        }

                        //Workaround for updating datagrid if user searched data
                        if (SearchBox.Text.Length > 0)
                        {
                            var text = SearchBox.Text;
                            SearchBox.Text = string.Empty;
                            SearchBox.Text = text;
                        }
                    }
                    catch (Exception) { await Download_WarehouseItems.Download(); MessageBox.Show("Wystąpił błąd podczas usuwania"); }
                }));
            });
            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
        }
        private void Groups_Clicked(object sender, RoutedEventArgs e)
        {
            Groups groups = new Groups();
            groups.Owner = Application.Current.MainWindow;
            groups.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            groups.ShowDialog();
        }
        private void Switch_EditFields(int data, Warehouse_ViewModel wdata)
        {
            switch (data)
            {
                case 0:
                    editname.IsEnabled = false;
                    editcount.IsHitTestVisible = false;
                    editmargin.IsEnabled = false;
                    editbrutto.IsHitTestVisible = false;
                    editgroup.IsEnabled = false;
                    editbutton.IsEnabled = false;
                    break;
                case 1:
                    editname.IsEnabled = true;
                    editmargin.IsEnabled = true;
                    editbutton.IsEnabled = true;
                    if (wdata.GroupType == "Usługa")
                    {
                        editgroup.IsEnabled = false;
                        editbrutto.IsHitTestVisible = true;
                    }
                    else
                    {
                        editgroup.IsEnabled = true;
                        editbrutto.IsHitTestVisible = false;
                    }
                    break;
                default:
                    break;
            }
        }
        private async void Find_Item(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (SearchBox.Text.Length == 0)
                    {
                        Warehouse_Grid.ItemsSource = collection;
                        timer.Stop();
                        return;
                    }
                    timer.Stop();
                    timer.Start();
                }));
            });
        }
        private async void OnElapsed(object source, ElapsedEventArgs e)
        {
            await Task.Run(() =>
            {
                Dispatcher.BeginInvoke(new Action(() => Warehouse_Grid.ItemsSource = SearchType.SelectedIndex == 0 ? Collections.WarehouseData.Where(x => x.Name.ToLower().Contains(SearchBox.Text.ToLower())) : Collections.WarehouseData.Where(x => x.GroupName.ToLower().Contains(SearchBox.Text.ToLower()))));
            });
        }
        private void GroupChanged_Selection(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((ComboBox)sender).Name == "warehousegroups")
                {
                    if (warehousegroups.SelectedIndex > -1)
                    {
                        if (Collections.GroupsData[warehousegroups.SelectedIndex].Type == 1)
                        {
                            itemcount.Text = "1";
                            itembrutto.Text = "";
                            itembrutto.IsHitTestVisible = true;
                        }
                        else
                        {
                            itemcount.Text = "0";
                            itembrutto.Text = "0,00";
                            itembrutto.IsHitTestVisible = false;
                        }
                    }
                }
                else
                {
                    if (editgroup.SelectedIndex > -1 && editgroup.SelectedIndex != LastIndex)
                    {
                        if (SelectedGroupType != (Collections.GroupsData[editgroup.SelectedIndex].Type == 0 ? "Towar" : "Usługa") && int.Parse(editcount.Text) > 0 && SelectedGroupType != "Usługa")
                        {
                            editgroup.SelectedIndex = LastIndex;
                            MessageBox.Show("Nie można zmienić typu grupy gdyż towar dalej widnieje na magazynie");
                        }
                        else if (SelectedGroupType != (Collections.GroupsData[editgroup.SelectedIndex].Type == 0 ? "Towar" : "Usługa"))
                        {
                            if (Collections.GroupsData[editgroup.SelectedIndex].Type == 0)
                            {
                                editcount.Text = "0";
                                editbrutto.Text = "0,00";
                                editbrutto.IsHitTestVisible = false;
                            }
                            else
                            {
                                editcount.Text = "1";
                                editbrutto.Text = "";
                                editbrutto.IsHitTestVisible = true;
                            }
                            SelectedGroupType = Collections.GroupsData[editgroup.SelectedIndex].Type == 0 ? "Towar" : "Usługa";
                            LastIndex = editgroup.SelectedIndex;
                        }
                        else
                        {
                            LastIndex = editgroup.SelectedIndex;
                        }
                    }
                }
            }
            catch (Exception) { editgroup.SelectedIndex = LastIndex; }
        }
    }
}
