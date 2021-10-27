using Frontier.Database.GetQuery;
using Frontier.Database.TableClasses;
using Frontier.Variables;
using Frontier.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Frontier.Windows.Settings_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Ustawienia.xaml
    /// </summary>
    public partial class Settings : Page
    {
        ObservableCollection<CompanyData_ViewModel> data { get; set; } = new ObservableCollection<CompanyData_ViewModel>();

        public Settings()
        {
            InitializeComponent();
            this.DataContext = data;
            LoadSettings();
        }

        private void LoadSettings()
        {
            GetCompanydata companydata = new GetCompanydata();
            var query = companydata.CompanyData.FirstOrDefault(x => x.idcompanydata == 1);
            data.Add(new CompanyData_ViewModel()
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
        private void Edit_CompanyData_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                GetCompanydata companydata = new GetCompanydata();
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
                bool updated = companydata.UpdateCompany(data);
                companydata.SaveChanges();
                if (updated)
                {
                    MessageBox.Show("Zaktualizowano dane pomyślnie.");
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas aktualizacji danych.");
            }
        }
        private void ChangeLogin_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                GetUser userdata = new GetUser(GlobalVariables.DatabaseName);
                var update = userdata.UpdateLogin(LoginData.Text);
                userdata.SaveChanges();
                if (update)
                {
                    MessageBox.Show("Zaktualizowano login pomyślnie.");
                    LoginData.Text = string.Empty;
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas aktualizacji danych.");
            }
        }
        private void ChangePassword_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                GetUser userdata = new GetUser(GlobalVariables.DatabaseName);
                var update = userdata.UpdatePassword(PasswordData.Password);
                userdata.SaveChanges();
                if (update)
                {
                    MessageBox.Show("Zaktualizowano hasło pomyślnie.");
                    PasswordData.Password = string.Empty;
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas aktualizacji danych.");
            }
        }
    }
}
