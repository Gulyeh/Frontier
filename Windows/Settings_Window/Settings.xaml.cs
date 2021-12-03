using Frontier.Database.GetQuery;
using Frontier.Database.TableClasses;
using Frontier.Methods.Invoices;
using Frontier.Methods.Numerics;
using Frontier.Variables;
using Frontier.ViewModels;
using Frontier.Windows.Confirmation_Window;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Frontier.Windows.Settings_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Ustawienia.xaml
    /// </summary>
    public partial class Settings : Page, INotifyPropertyChanged
    {
        private string name;
        public string MainName
        {
            get { return name; }
            set
            {
                if (name == value) return;
                name = value;
                NotifyPropertyChanged("MainName");
            }
        }
        private string _NIP;
        public string NIP
        {
            get { return _NIP; }
            set
            {
                if (_NIP == value) return;
                _NIP = value;
                NotifyPropertyChanged("NIP");
            }
        }
        private string street;
        public string Street
        {
            get { return street; }
            set
            {
                if (street == value) return;
                street = value;
                NotifyPropertyChanged("Street");
            }
        }
        private string regon;
        public string REGON
        {
            get { return regon; }
            set
            {
                if (regon == value) return;
                regon = value;
                NotifyPropertyChanged("REGON");
            }
        }
        private string state;
        public string State
        {
            get { return state; }
            set
            {
                if (state == value) return;
                state = value;
                NotifyPropertyChanged("State");
            }
        }
        private string postcode;
        public string PostCode
        {
            get { return postcode; }
            set
            {
                if (postcode == value) return;
                postcode = value;
                NotifyPropertyChanged("PostCode");
            }
        }
        private string country;
        public string Country
        {
            get { return country; }
            set
            {
                if (country == value) return;
                country = value;
                NotifyPropertyChanged("Country");
            }
        }
        private string bdo;
        public string BDO
        {
            get { return bdo; }
            set
            {
                if (bdo == value) return;
                bdo = value;
                NotifyPropertyChanged("BDO");
            }
        }

        public Settings()
        {
            InitializeComponent();
            LoadSettings();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private async void LoadSettings()
        {
            await Task.Run(async () =>
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        using (GetCompanydata companydata = new GetCompanydata())
                        {
                            var query = companydata.CompanyData.FirstOrDefault();
                            Collections.CompanyData = new Dictionary<string, string>
                            {
                                { "Name", query.Name },
                                { "NIP", query.NIP },
                                { "Street", query.Street },
                                { "REGON", query.REGON },
                                { "State", query.State },
                                { "PostCode", query.PostCode },
                                { "Country", query.Country },
                                { "BDO", query.BDO }
                            };
                            MainName = query.Name;
                            NIP = query.NIP;
                            Street = query.Street;
                            REGON = query.REGON;
                            State = query.State;
                            PostCode = query.PostCode;
                            Country = query.Country;
                            BDO = query.BDO;
                        }
                    }
                    catch (Exception) { MessageBox.Show("Nie udało się załadować danych firmy.\nProszę zalogować się ponownie"); ((MainWindow)Application.Current.MainWindow).Logout(); }
                }));
            });
        }
        private void CheckNumeric(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex_Check.CheckNumbers(e.Text);
        }
        private void CheckSyntax_PostCode(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex_Check.CheckPostCode(e.Text);
        }
        private void ValidateNIP(object sender, TextChangedEventArgs e)
        {
            if (compnip.Text.Length == 10)
            {
                bool validateNIP = Checkers.CheckNIP(compnip.Text);
                if (!validateNIP)
                {
                    MessageBox.Show("NIP jest niepoprawny!");
                    compnip.Text = String.Empty;
                }
            }
        }
        private void CheckSpace(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        private void ValidateREGON(object sender, TextChangedEventArgs e)
        {
            if (compregon.Text.Length == 9 || compregon.Text.Length == 14)
            {
                int oldlength = compregon.Text.Length;
                if (compregon.Text.Length == oldlength)
                {
                    bool validateREGON = Checkers.CheckREGON(compregon.Text);
                    if (!validateREGON)
                    {
                        MessageBox.Show("REGON jest niepoprawny!");
                        compregon.Text = String.Empty;
                    }
                }
            }
        }
        private async void Edit_CompanyData_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Compname.Text != string.Empty && compnip.Text != string.Empty && compstreet.Text != string.Empty && compstate.Text != string.Empty && comppostcode.Text != string.Empty && compcountry.Text != string.Empty)
                {
                    using (GetCompanydata companydata = new GetCompanydata())
                    {
                        bool updated = await companydata.UpdateCompany(new CompanyData()
                        {
                            Name = Compname.Text,
                            NIP = compnip.Text,
                            REGON = compregon.Text,
                            Street = compstreet.Text,
                            PostCode = comppostcode.Text,
                            State = compstate.Text,
                            Country = compcountry.Text,
                            BDO = CompBDO.Text
                        });
                        if (updated)
                        {
                            await companydata.SaveChangesAsync();
                            LoadSettings();
                            MessageBox.Show("Zaktualizowano dane pomyślnie.");
                        }
                        else
                        {
                            throw new ArgumentNullException();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Proszę uzupełnić wymagane dane.");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas aktualizacji danych.");
            }
        }
        private async void ChangeLogin_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LoginData.Text != string.Empty)
                {
                    using (GetUser userdata = new GetUser(GlobalVariables.DatabaseName))
                    {
                        var update = await userdata.UpdateLogin(LoginData.Text);
                        if (update)
                        {
                            userdata.SaveChanges();
                            MessageBox.Show("Zaktualizowano login pomyślnie.");
                            LoginData.Text = string.Empty;
                        }
                        else
                        {
                            throw new ArgumentNullException();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Proszę podać wymagane dane");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas aktualizacji danych.");
            }
        }
        private async void ChangePassword_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PasswordData.Password != string.Empty)
                {
                    using (GetUser userdata = new GetUser(GlobalVariables.DatabaseName))
                    {
                        var update = await userdata.UpdatePassword(PasswordData.Password);
                        if (update)
                        {
                            userdata.SaveChanges();
                            MessageBox.Show("Zaktualizowano hasło pomyślnie.");
                            PasswordData.Password = string.Empty;
                        }
                        else
                        {
                            throw new ArgumentNullException();
                        }

                    }
                }
                else
                {
                    MessageBox.Show("Proszę uzupełnić dane");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas aktualizacji danych.");
            }
        }
        private async void DeleteDB_Clicked(object sender, RoutedEventArgs e)
        {
            Confirmation confirm = new Confirmation("Delete");
            confirm.Owner = Application.Current.MainWindow;
            confirm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            bool? data = confirm.ShowDialog();
            if (data.HasValue && data.Value)
            {
                await Task.Run(async () =>
                {
                    await Dispatcher.BeginInvoke(new Action(async() =>
                    {
                        try
                        {
                            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                            await PermaDelete.Delete(AppDomain.CurrentDomain.BaseDirectory + "/Database", GlobalVariables.DatabaseName + ".sqlite");
                            ((MainWindow)Application.Current.MainWindow).Logout();
                            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                        }
                        catch (Exception)
                        {
                            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                            MessageBox.Show("Wystąpił błąd podczas usuwania bazy danych");
                        }
                    }));
                });
            }
        }
        private async void ExportDB_Clicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog exportSaveFileDialog = new System.Windows.Forms.SaveFileDialog();

            exportSaveFileDialog.Title = "Select Destination";
            exportSaveFileDialog.Filter = "SQLite(*.sqlite)|*.sqlite";

            if (System.Windows.Forms.DialogResult.OK == exportSaveFileDialog.ShowDialog())
            {
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                await Task.Run(async () =>
                {
                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            File.Copy(AppDomain.CurrentDomain.BaseDirectory + "/Database/" + GlobalVariables.DatabaseName + ".sqlite", exportSaveFileDialog.FileName, true);
                            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                            MessageBox.Show("Baza danych została wyeksportowana pomyślnie");
                        }
                        catch (Exception)
                        {
                            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                            MessageBox.Show("Wystąpił błąd podczas eksportowania bazy danych");
                        }
                    }));
                });
            }
            
        }
    }
}
