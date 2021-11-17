using Frontier.Database.GetQuery;
using Frontier.Database.TableClasses;
using Frontier.Methods;
using Frontier.Variables;
using Frontier.ViewModels;
using System;
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
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
            this.DataContext = Collections.CompanyData;
            LoadSettings();
        }
        private async void LoadSettings()
        {
            await Task.Run(async() => {
                await this.Dispatcher.BeginInvoke(new Action(async() =>
                {
                    using (GetCompanydata companydata = new GetCompanydata())
                    {
                        var query = companydata.CompanyData.Where(x => x.idcompanydata == 1).FirstOrDefault();
                        var data = new CompanyData_ViewModel()
                        {
                            Name = query.Name,
                            NIP = query.NIP,
                            Street = query.Street,
                            REGON = query.REGON,
                            State = query.State,
                            PostCode = query.PostCode,
                            Country = query.Country
                        };
                        Collections.CompanyData.Add(data);
                    }
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
                        var data = new CompanyData()
                        {
                            Name = Compname.Text,
                            NIP = compnip.Text,
                            REGON = compregon.Text,
                            Street = compstreet.Text,
                            PostCode = comppostcode.Text,
                            State = compstate.Text,
                            Country = compcountry.Text
                        };

                        bool updated = await companydata.UpdateCompany(data);
                        if (updated)
                        {
                            companydata.SaveChanges();
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
    }
}
