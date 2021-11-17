using Frontier.Database.GetQuery;
using Frontier.Methods;
using Frontier.Variables;
using Frontier.ViewModels;
using Frontier.Windows.Confirmation_Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Frontier.Windows.Contactors_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Kontrahenci.xaml
    /// </summary>
    public partial class Contactors : Page
    {
        private int ContactorID { get; set; }
        public Contactors()
        {
            InitializeComponent();
            Contactors_Grid.ItemsSource = Collections.ContactorsData;
            LoadContactors();
        }
        private async void LoadContactors()
        {
            await Task.Run(async () =>
            {
                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    using (GetContactors Contactors = new GetContactors())
                    {
                        var query = Contactors.Contactors;
                        foreach (var data in query)
                        {
                            Collections.ContactorsData.Add(new Contactors_ViewModel
                            {
                                ID = data.idContactors,
                                Address = data.Street,
                                Country = data.Country,
                                Name = data.Name,
                                State = data.State,
                                NIP = data.NIP,
                                PostCode = data.PostCode,
                                Regon = data.REGON
                            });
                        }
                    }
                }));
            });
        }
        private void NIP_CheckNumeric(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex_Check.CheckNumbers(e.Text);
        }
        private void CheckSyntax_PostCode(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex_Check.CheckPostCode(e.Text);
        }
        private void CheckSpace(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        private void ValidateNIP(object sender, TextChangedEventArgs e)
        {
            if (contnip.Text.Length == 10)
            {
                bool validateNIP = Checkers.CheckNIP(contnip.Text);
                if (!validateNIP)
                {
                    MessageBox.Show("NIP jest niepoprawny!");
                    contnip.Text = String.Empty;
                }
            }
        }
        private void ValidateREGON(object sender, TextChangedEventArgs e)
        {
            if (contregon.Text.Length == 9 || contregon.Text.Length == 14)
            {
                int oldlength = contregon.Text.Length;
                if (contregon.Text.Length == oldlength)
                {
                    bool validateREGON = Checkers.CheckREGON(contregon.Text);
                    if (!validateREGON)
                    {
                        MessageBox.Show("REGON jest niepoprawny!");
                        contregon.Text = String.Empty;
                    }
                }
            }
        }
        private async void AddContactor_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (contname.Text != string.Empty && contnip.Text != string.Empty && contnip.Text.Length == 10 && contstreet.Text != string.Empty && contstate.Text != string.Empty && contpostcode.Text != string.Empty && contcountry.Text != string.Empty)
                {
                    if (!Collections.ContactorsData.Any(x => x.NIP == contnip.Text) && !Collections.ContactorsData.Any(x => x.Name == contname.Text))
                    {
                        using (GetContactors contactor = new GetContactors())
                        {
                            var data = new Database.TableClasses.Contactors()
                            {
                                Name = contname.Text,
                                NIP = contnip.Text,
                                Street = contstreet.Text,
                                REGON = contregon.Text,
                                State = contstate.Text,
                                PostCode = contpostcode.Text,
                                Country = contcountry.Text
                            };
                            var updated = await contactor.AddContractor(data);

                            if (updated)
                            {
                                contactor.SaveChanges();
                                var newcontactor = contactor.Contactors.OrderByDescending(x => x.idContactors).First();
                                var row = new Contactors_ViewModel()
                                {
                                    ID = newcontactor.idContactors,
                                    Name = newcontactor.Name,
                                    NIP = newcontactor.NIP,
                                    Address = newcontactor.Street,
                                    State = newcontactor.State,
                                    Regon = newcontactor.REGON,
                                    PostCode = newcontactor.PostCode,
                                    Country = newcontactor.Country
                                };
                                Collections.ContactorsData.Add(row);
                                MessageBox.Show("Pomyślnie dodano nowego kontrahenta");
                                ResetTextBoxes("new");
                            }
                            else
                            {
                                throw new ArgumentNullException();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Kontrahent o takiej nazwie lub NIPie już istnieje w bazie");
                    }
                }
                else
                {
                    MessageBox.Show("Proszę wypełnić wszystkie wymagane dane");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas aktualizacji danych.");
            }
        }
        private async void EditContactor_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (editname.Text != string.Empty && editnip.Text != string.Empty && editstreet.Text != string.Empty && editstate.Text != string.Empty && editpostcode.Text != string.Empty && editcountry.Text != string.Empty)
                {
                    if (!Collections.ContactorsData.Any(x => x.NIP == editnip.Text && x.ID != ContactorID) && !Collections.ContactorsData.Any(x => x.Name == editname.Text && x.ID != ContactorID))
                    {
                        using (GetContactors edit_contactor = new GetContactors())
                        {
                            var data = new Database.TableClasses.Contactors()
                            {
                                idContactors = ContactorID,
                                Name = editname.Text,
                                NIP = editnip.Text,
                                Street = editstreet.Text,
                                REGON = editregon.Text,
                                State = editstate.Text,
                                PostCode = editpostcode.Text,
                                Country = editcountry.Text
                            };

                            var updated = await edit_contactor.EditContactor(data);
                            if (updated)
                            {
                                edit_contactor.SaveChanges();
                                var row = Collections.ContactorsData.Where(x => x.ID == data.idContactors).FirstOrDefault();
                                var new_model = new Contactors_ViewModel()
                                {
                                    ID = row.ID,
                                    Name = editname.Text,
                                    NIP = editnip.Text,
                                    Address = editstreet.Text,
                                    Regon = editregon.Text,
                                    State = editstate.Text,
                                    PostCode = editpostcode.Text,
                                    Country = editcountry.Text
                                };
                                Collections.ContactorsData[Collections.ContactorsData.IndexOf(Collections.ContactorsData.Where(x => x.ID == data.idContactors).FirstOrDefault())] = new_model;

                                //Workaround for updating datagrid if user searched data
                                if (SearchBox.Text.Length > 0)
                                {
                                    var text = SearchBox.Text;
                                    SearchBox.Text = string.Empty;
                                    SearchBox.Text = text;
                                }

                                MessageBox.Show("Pomyślnie zaktualizowano dane kontrahenta");
                                ResetTextBoxes("edit");
                                Switch_EditContactor(0);
                            }
                            else
                            {
                                throw new ArgumentNullException();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Kontrahent o takiej nazwie lub NIPie już istnieje w bazie");
                    }
                }
                else
                {
                    MessageBox.Show("Proszę wypełnić wszystkie wymagane dane");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas aktualizacji danych.");
            }
        }
        private void Switch_EditContactor(int data)
        {
            switch (data)
            {
                case 0:
                    editname.IsEnabled = false;
                    editnip.IsEnabled = false;
                    editstreet.IsEnabled = false;
                    editregon.IsEnabled = false;
                    editstate.IsEnabled = false;
                    editpostcode.IsEnabled = false;
                    editcountry.IsEnabled = false;
                    EditContactor_Button.IsEnabled = false;
                    break;
                case 1:
                    editname.IsEnabled = true;
                    editnip.IsEnabled = true;
                    editstreet.IsEnabled = true;
                    editregon.IsEnabled = true;
                    editstate.IsEnabled = true;
                    editpostcode.IsEnabled = true;
                    editcountry.IsEnabled = true;
                    EditContactor_Button.IsEnabled = true;
                    break;
                default:
                    break;
            }
        }
        private void ResetTextBoxes(string data)
        {
            switch (data)
            {
                case "new":
                    contname.Text = string.Empty;
                    contnip.Text = string.Empty;
                    contstreet.Text = string.Empty;
                    contregon.Text = string.Empty;
                    contstate.Text = string.Empty;
                    contpostcode.Text = string.Empty;
                    contcountry.Text = string.Empty;
                    break;
                case "edit":
                    editname.Text = string.Empty;
                    editnip.Text = string.Empty;
                    editstreet.Text = string.Empty;
                    editregon.Text = string.Empty;
                    editstate.Text = string.Empty;
                    editpostcode.Text = string.Empty;
                    editcountry.Text = string.Empty;
                    break;
                default:
                    break;
            }
        }
        private void Delete_Clicked(object sender, RoutedEventArgs e)
        {
            Confirmation confirm = new Confirmation("Contactors");
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
                    foreach (Contactors_ViewModel data in Contactors_Grid.SelectedItems)
                    {
                        ids.Add(data.ID);
                    }

                    using (GetContactors delete_contactor = new GetContactors())
                    {
                        foreach (int data in ids)
                        {
                            bool updated = delete_contactor.DeleteContactor(data);
                            if (updated)
                            {
                                await delete_contactor.SaveChangesAsync();
                                Collections.ContactorsData.Remove(Collections.ContactorsData.Where(x => x.ID == data).FirstOrDefault());
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
        private void Find_Contactors(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text.Length > 0)
            {
                Contactors_Grid.ItemsSource = Collections.ContactorsData.Where(x => x.Name.ToLower().Contains(SearchBox.Text));
            }
            else
            {
                Contactors_Grid.ItemsSource = Collections.ContactorsData;
            }
        }
        private void EditRow_Clicked(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            var item = (sender as FrameworkElement).DataContext;
            var index = Contactors_Grid.Items.IndexOf(item);
            var id = Contactors_Grid.Columns[0].GetCellContent(Contactors_Grid.Items[index]) as TextBlock;
            ContactorID = Int32.Parse(id.Text);
            var data = Collections.ContactorsData.Where(x => x.ID == ContactorID).FirstOrDefault();
            FillEditContactor(data);
        }
        private void FillEditContactor(Contactors_ViewModel data)
        {
            editname.Text = data.Name;
            editnip.Text = data.NIP;
            editstreet.Text = data.Address;
            editregon.Text = data.Regon;
            editstate.Text = data.State;
            editpostcode.Text = data.PostCode;
            editcountry.Text = data.Country;
            Switch_EditContactor(1);
        }
    }
}
