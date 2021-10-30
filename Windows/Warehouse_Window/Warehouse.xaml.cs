using Frontier.Database.GetQuery;
using Frontier.Methods;
using Frontier.Variables;
using Frontier.ViewModels;
using Frontier.Windows.Confirmation_Window;
using Frontier.Windows.Groups_Window;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Frontier.Windows.Warehouse_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Magazyn.xaml
    /// </summary>
    public partial class Warehouse : Page
    {
        private int ItemID { get; set; }

        public Warehouse()
        {
            InitializeComponent();
            Warehouse_Grid.ItemsSource = Collections.WarehouseData;
            warehousegroups.ItemsSource = Collections.GroupsData;
            editgroup.ItemsSource = Collections.GroupsData;
            LoadWarehouse();
        }

        private async void LoadWarehouse()
        {
            await Task.Run(async() => 
            {
                await this.Dispatcher.BeginInvoke(new Action(() => { 
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
                                VAT = data.VAT
                            });
                        }
                    }

                    using (GetWarehouse warehouse = new GetWarehouse())
                    {
                        var query = warehouse.Warehouse;
                        foreach (var data in query)
                        {
                            Collections.WarehouseData.Add(new Warehouse_ViewModel
                            {
                                ID = data.idWarehouse,
                                GroupID = Int32.Parse(data.Group),
                                GroupName = Collections.GroupsData.Where(x => x.ID == Int32.Parse(data.Group)).FirstOrDefault().Name,
                                Name = data.Name,
                                Amount = data.Amount,
                                Brutto = data.Brutto,
                                Netto = data.Netto,
                                VAT = Collections.GroupsData.Where(x => x.ID == Int32.Parse(data.Group)).FirstOrDefault().VAT,
                                Margin = data.Margin
                            });
                        }
                    }
                }));
            });
        }
        private async void AddItem_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (itemname.Text != string.Empty && itemcount.Text != string.Empty && itemnetto.Text != string.Empty && warehousegroups.SelectedIndex > -1)
                {
                    using (GetWarehouse add_itemWarehouse = new GetWarehouse())
                    {
                        var isNumericMargin = int.TryParse(itemmargin.Text, out int margin);
                        var isNumericVAT = double.TryParse(Collections.GroupsData[warehousegroups.SelectedIndex].VAT, out double vat);

                        var data = new Database.TableClasses.Warehouse()
                        {
                            Name = itemname.Text,
                            Group = Collections.GroupsData[warehousegroups.SelectedIndex].ID.ToString(),
                            Amount = int.Parse(itemcount.Text),
                            Netto = double.Parse(itemnetto.Text),
                            Margin = isNumericMargin == true ? margin : 0,
                            Brutto = isNumericVAT == true ? Calculate.GetBrutto(vat, double.Parse(itemnetto.Text)) : double.Parse(itemnetto.Text)
                        };

                        var update = await add_itemWarehouse.AddItem(data);
                        if (update)
                        {
                            add_itemWarehouse.SaveChanges();

                            var newItem = add_itemWarehouse.Warehouse.OrderByDescending(x => x.idWarehouse).First();
                            var new_data = new Warehouse_ViewModel()
                            {
                                ID = newItem.idWarehouse,
                                Name = itemname.Text,
                                GroupName = Collections.GroupsData[warehousegroups.SelectedIndex].Name,
                                GroupID = Collections.GroupsData[warehousegroups.SelectedIndex].ID,
                                Amount = int.Parse(itemcount.Text),
                                Netto = double.Parse(itemnetto.Text),
                                Margin = data.Margin,
                                VAT = Collections.GroupsData[warehousegroups.SelectedIndex].VAT,
                                Brutto = data.Brutto
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
            try
            {
                if (editname.Text != string.Empty && editcount.Text != string.Empty && editnetto.Text != string.Empty && editgroup.SelectedIndex > -1)
                {
                    var isNumericMargin = int.TryParse(editmargin.Text, out int margin);
                    var isNumericVAT = double.TryParse(Collections.GroupsData[editgroup.SelectedIndex].VAT, out double vat);

                    using (GetWarehouse edit_item = new GetWarehouse())
                    {
                        var data = new Database.TableClasses.Warehouse()
                        {
                            idWarehouse = ItemID,
                            Name = editname.Text,
                            Amount = int.Parse(editcount.Text),
                            Group = Collections.GroupsData[editgroup.SelectedIndex].ID.ToString(),
                            Netto = double.Parse(editnetto.Text),
                            Margin = isNumericMargin == true ? margin : 0,
                            Brutto = isNumericVAT == true ? Calculate.GetBrutto(vat, double.Parse(editnetto.Text)) : double.Parse(editnetto.Text)
                        };

                        var update = await edit_item.EditItem(data);
                        if (update)
                        {
                            await edit_item.SaveChangesAsync();
                            var new_data = new Warehouse_ViewModel()
                            {
                                ID = ItemID,
                                Name = editname.Text,
                                GroupName = Collections.GroupsData[editgroup.SelectedIndex].Name,
                                GroupID = Collections.GroupsData[editgroup.SelectedIndex].ID,
                                Amount = int.Parse(editcount.Text),
                                Netto = double.Parse(editnetto.Text),
                                Margin = data.Margin,
                                VAT = Collections.GroupsData[editgroup.SelectedIndex].VAT,
                                Brutto = data.Brutto
                            };
                            Collections.WarehouseData[Collections.WarehouseData.IndexOf(Collections.WarehouseData.Where(x => x.ID == ItemID).FirstOrDefault())] = new_data;

                            //Workaround for updating datagrid if user searched data
                            if (SearchBox.Text.Length > 0)
                            {
                                var text = SearchBox.Text;
                                SearchBox.Text = string.Empty;
                                SearchBox.Text = text;
                            }

                            MessageBox.Show("Pomyślnie zaktualizowano dane");
                            Switch_EditFields(0);
                            ResetFillData("edit");
                        }
                        else
                        {
                            throw new ArgumentNullException();
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas aktualizacji danych");
            }
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
            editnetto.Text = data.Netto.ToString();
            editmargin.Text = data.Margin.ToString();
            editgroup.SelectedIndex = Collections.GroupsData.IndexOf(Collections.GroupsData.Where(x => x.ID == data.GroupID).FirstOrDefault());
            Switch_EditFields(1);
        }
        private void ResetFillData(string data)
        {
            switch (data)
            {
                case "new":
                    itemname.Text = string.Empty;
                    itemcount.Text = string.Empty;
                    itemmargin.Text = string.Empty;
                    itemnetto.Text = string.Empty;
                    warehousegroups.SelectedIndex = -1;
                    break;
                case "edit":
                    editname.Text = string.Empty;
                    editcount.Text = string.Empty;
                    editmargin.Text = string.Empty;
                    editnetto.Text = string.Empty;
                    editgroup.SelectedIndex = -1;
                    break;
                default:
                    break;
            }
        }
        private void CheckNumeric(object sender, TextCompositionEventArgs e)
        {
            e.Handled = CheckNIP.CheckNumbers(e.Text);
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
            await Task.Run(async () =>
            {
                await this.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    List<int> ids = new List<int>();
                    foreach (Warehouse_ViewModel data in Warehouse_Grid.SelectedItems)
                    {
                        ids.Add(data.ID);
                    }

                    using (GetWarehouse delete_item = new GetWarehouse())
                    {
                        foreach (int data in ids)
                        {
                            bool updated = delete_item.DeleteItem(data);
                            if (updated)
                            {
                                await delete_item.SaveChangesAsync();
                                Collections.WarehouseData.Remove(Collections.WarehouseData.Where(x => x.ID == data).FirstOrDefault());
                            }
                        }
                    }

                    //Workaround for updating datagrid if user searched data
                    if (SearchBox.Text.Length > 0)
                    {
                        var text = SearchBox.Text;
                        SearchBox.Text = string.Empty;
                        SearchBox.Text = text;
                    }

                }));
            });
        }
        private void Groups_Clicked(object sender, RoutedEventArgs e)
        {
            Groups groups = new Groups();
            groups.Owner = Application.Current.MainWindow;
            groups.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            groups.ShowDialog();
        }
        private void Switch_EditFields(int data)
        {
            switch (data)
            {
                case 0:
                    editname.IsEnabled = false;
                    editcount.IsEnabled = false;
                    editmargin.IsEnabled = false;
                    editnetto.IsEnabled = false;
                    editgroup.IsEnabled = false;
                    editbutton.IsEnabled = false;
                    break;
                case 1:
                    editname.IsEnabled = true;
                    editcount.IsEnabled = true;
                    editmargin.IsEnabled = true;
                    editnetto.IsEnabled = true;
                    editgroup.IsEnabled = true;
                    editbutton.IsEnabled = true;
                    break;
                default:
                    break;
            }
        }
        private void Find_Item(object sender, RoutedEventArgs e)
        {
            if(SearchBox.Text.Length == 0)
            {
                Warehouse_Grid.ItemsSource = Collections.WarehouseData;
            }
            else
            {
                Warehouse_Grid.ItemsSource = SearchType.SelectedIndex == 0 ? Collections.WarehouseData.Where(x => x.Name.ToLower().Contains(SearchBox.Text)) : Collections.WarehouseData.Where(x => x.GroupName.ToLower().Contains(SearchBox.Text));
            }
        }
    }
}
