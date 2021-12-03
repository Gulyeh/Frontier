using Frontier.Database.GetQuery;
using Frontier.Methods.Numerics;
using Frontier.Variables;
using Frontier.ViewModels;
using Frontier.Windows.Confirmation_Window;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Frontier.Windows.Groups_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Grupy.xaml
    /// </summary>
    public partial class Groups : Window
    {
        public Groups()
        {
            InitializeComponent();
            GroupList.ItemsSource = Collections.GroupsData;
        }
        private async void CreateGroup_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (groupname.Text != string.Empty)
                {
                    using (GetGroups group = new GetGroups())
                    {
                        var data = new Database.TableClasses.Groups()
                        {
                            Name = groupname.Text,
                            Description = groupdescription.Text,
                            VAT = groupvat.Text.Replace("%", ""),
                            GTU = groupgtu.Text,
                            Type = type.SelectedIndex
                        };

                        var updated = await group.AddGroups(data);
                        if (updated)
                        {
                            var newgroupID = group.Groups.OrderByDescending(x => x.idgroups).First().idgroups;
                            var vm = new Groups_ViewModel()
                            {
                                ID = newgroupID,
                                Name = groupname.Text,
                                Description = groupdescription.Text,
                                VAT = groupvat.Text.Replace("%", ""),
                                GTU = groupgtu.Text,
                                Type = type.SelectedIndex
                            };
                            Collections.GroupsData.Add(vm);
                            await group.SaveChangesAsync();
                            MessageBox.Show("Pomyślnie dodano grupę");
                            ResetFields("new");
                        }
                        else
                        {
                            throw new ArgumentNullException();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Proszę podać nazwę grupy");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas tworzenia grupy");
            }
        }
        private async void EditGroup_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var index = GroupList.SelectedIndex;
                using (GetGroups edit_group = new GetGroups())
                {
                    var data = new Database.TableClasses.Groups()
                    {
                        idgroups = Collections.GroupsData[index].ID,
                        Name = editname.Text,
                        Description = editdescription.Text,
                        VAT = editvat.Text.Replace("%", ""),
                        GTU = editgtu.Text,
                        Type = Collections.GroupsData[index].Type
                    };

                    var updated = await edit_group.EditGroup(data);
                    if (updated)
                    {
                        var newdata = new Groups_ViewModel()
                        {
                            ID = Collections.GroupsData[index].ID,
                            Name = editname.Text,
                            Description = editdescription.Text,
                            VAT = editvat.Text.Replace("%", ""),
                            GTU = editgtu.Text,
                            Type = Collections.GroupsData[index].Type
                        };
                        Collections.GroupsData[Collections.GroupsData.IndexOf(Collections.GroupsData.Where(x => x.ID == Collections.GroupsData[index].ID).FirstOrDefault())] = newdata;
                        await CorrectData(index);
                        await edit_group.SaveChangesAsync();
                        MessageBox.Show("Pomyślnie zaktualizowano grupę");
                        ResetFields("edit");
                    }
                    else
                    {
                        throw new ArgumentNullException();
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas edytowania grupy");
            }
        }
        private async Task CorrectData(int index)
        {
            await Task.Run(async () =>
            {
                await this.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    try
                    {
                        using (GetWarehouse item = new GetWarehouse())
                        {
                            using (GetWarehouse edit_item = new GetWarehouse())
                            {
                                var groupdata = Collections.GroupsData[index];
                                foreach (var data in Collections.WarehouseData.Where(x => x.GroupID == groupdata.ID))
                                {
                                    if (groupdata.Type == 1)
                                    {
                                        var new_data = new Database.TableClasses.Warehouse()
                                        {
                                            idWarehouse = data.ID,
                                            Name = data.Name,
                                            Amount = data.Amount,
                                            Group = data.GroupID.ToString(),
                                            Brutto = data.Brutto,
                                            Margin = data.Margin,
                                            Netto = data.Netto,
                                            VAT = groupdata.VAT
                                        };
                                        var update = await edit_item.EditItem(new_data);
                                        if (update)
                                        {
                                            Collections.WarehouseData.Where(x => x.ID == data.ID).FirstOrDefault().Netto = decimal.TryParse(groupdata.VAT, out decimal vat) == true ? Calculate.GetNetto(vat, Collections.WarehouseData.Where(x => x.ID == data.ID).FirstOrDefault().Brutto) : Collections.WarehouseData.Where(x => x.ID == data.ID).FirstOrDefault().Brutto;
                                            Collections.WarehouseData.Where(x => x.ID == data.ID).FirstOrDefault().VAT = groupdata.VAT;
                                        }
                                    }
                                    Collections.WarehouseData.Where(x => x.ID == data.ID).FirstOrDefault().GroupName = groupdata.Name;
                                }
                                await edit_item.SaveChangesAsync();
                            }
                        }
                    }
                    catch (Exception) { }
                }));
            });          
        }
        private async void DeleteGroup_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                Confirmation confirm = new Confirmation("Delete");
                confirm.Owner = Application.Current.MainWindow;
                confirm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                bool? agreed = confirm.ShowDialog();
                if (agreed.HasValue && agreed.Value)
                {
                    if (!Collections.WarehouseData.Any(x => x.GroupID == Collections.GroupsData[GroupList.SelectedIndex].ID))
                    {
                        using (GetGroups delete_group = new GetGroups())
                        {
                            var updated = delete_group.DeleteGroup(Collections.GroupsData[GroupList.SelectedIndex].ID);
                            if (updated)
                            {
                                Collections.GroupsData.Remove(Collections.GroupsData.Where(x => x.ID == Collections.GroupsData[GroupList.SelectedIndex].ID).FirstOrDefault());
                                await delete_group.SaveChangesAsync();
                                MessageBox.Show("Pomyślnie usuniętno grupę");
                                ResetFields("edit");
                            }
                            else
                            {
                                throw new ArgumentNullException();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Nie można usunąc danej grupy gdyż nie jest pusta");
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas usuwania grupy");
            }
        }
        private void SelectedGroup(object sender, SelectionChangedEventArgs e)
        {
            if (GroupList.SelectedIndex > -1)
            {
                editname.IsEnabled = true;
                editdescription.IsEnabled = true;
                editvat.IsEnabled = true;
                editgtu.IsEnabled = true;
                editbutton.IsEnabled = true;
                deletebutton.IsEnabled = true;
                FillEditData();
            }
            else
            {
                editname.IsEnabled = false;
                editdescription.IsEnabled = false;
                editvat.IsEnabled = false;
                editgtu.IsEnabled = false;
                editbutton.IsEnabled = false;
                deletebutton.IsEnabled = false;
            }
        }
        private void FillEditData()
        {
            var data = Collections.GroupsData[GroupList.SelectedIndex];
            editname.Text = data.Name;
            editdescription.Text = data.Description;
            var Find_vatItem = editvat.Items.OfType<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString().Contains(data.VAT));
            var Find_gtuItem = editgtu.Items.OfType<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == data.GTU);
            editvat.SelectedIndex = editvat.Items.IndexOf(Find_vatItem);
            editgtu.SelectedIndex = editgtu.Items.IndexOf(Find_gtuItem);
        }
        private void ResetFields(string data)
        {
            switch (data)
            {
                case "new":
                    groupname.Text = string.Empty;
                    groupdescription.Text = string.Empty;
                    groupvat.SelectedIndex = 0;
                    groupgtu.SelectedIndex = 0;
                    type.SelectedIndex = 0;
                    break;
                case "edit":
                    editname.Text = string.Empty;
                    editdescription.Text = string.Empty;
                    editgtu.SelectedIndex = 0;
                    editvat.SelectedIndex = 0;
                    GroupList.SelectedIndex = -1;
                    break;
                default:
                    break;
            }
        }
    }
}
