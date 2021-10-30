using Frontier.Database.GetQuery;
using Frontier.Database.TableClasses;
using Frontier.Methods;
using Frontier.Variables;
using Frontier.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        private void LoadSettings()
        {
            GetCompanydata companydata = new GetCompanydata();
            var query = companydata.CompanyData.FirstOrDefault(x => x.idcompanydata == 1);
            Collections.CompanyData.Add(new CompanyData_ViewModel()
            {
                Name = query.Name,
                NIP = query.NIP,
                Street = query.Street,
                REGON = query.REGON,
                State = query.State,
                PostCode = query.PostCode,
                Country = query.Country
            });
        }
        private void CheckNumeric(object sender, TextCompositionEventArgs e)
        {
            e.Handled = CheckNIP.CheckNumbers(e.Text);
        }
        private void CheckSyntax_PostCode(object sender, TextCompositionEventArgs e)
        {
            e.Handled = CheckNIP.CheckPostCode(e.Text);
        }
        private void ValidateNIP(object sender, TextChangedEventArgs e)
        {
            if (compnip.Text.Length == 10)
            {
                bool validateNIP = CheckNIP.Checker(compnip.Text);
                if (!validateNIP)
                {
                    MessageBox.Show("NIP jest niepoprawny!");
                    compnip.Text = String.Empty;
                }
            }
        }
        private async void Edit_CompanyData_Clicked(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas aktualizacji danych.");
            }
        }
        private async void ChangeLogin_Clicked(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas aktualizacji danych.");
            }
        }
        private async void ChangePassword_Clicked(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas aktualizacji danych.");
            }
        }
    }
}
