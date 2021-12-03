using Frontier.Variables;
using Frontier.ViewModels;
using Frontier.Windows.Analyze_Window;
using Frontier.Windows.Auth_Window;
using Frontier.Windows.Confirmation_Window;
using Frontier.Windows.Contactors_Window;
using Frontier.Windows.Invoices_Window;
using Frontier.Windows.Remanent_Window;
using Frontier.Windows.Settings_Window;
using Frontier.Windows.Warehouse_Window;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace Frontier
{

    public partial class MainWindow : Window
    {
        private readonly Login_ViewModel LoginModel = new Login_ViewModel();
        Invoices WInvoice;
        Warehouse WWarehouse;
        Contactors WContactors;
        Analyze WAnalyze;
        Settings WSettings;
        Auth WAuth;
        Remanent WRemanent;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = LoginModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WAuth = new Auth(LoginModel);
            Content_Frame.Content = WAuth;
        }

        public async Task LoadPages()
        {
            await Task.Run(async () =>
            {
                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    WWarehouse = new Warehouse();
                    WContactors = new Contactors();
                    WSettings = new Settings();
                    WInvoice = new Invoices();
                    WAnalyze = new Analyze();
                    WRemanent = new Remanent();
                }));
            });
        }

        private void Clicked_Menu(object sender, RoutedEventArgs e)
        {
            switch (Menu_List.SelectedIndex)
            {
                case 0:
                    Content_Frame.Content = WInvoice;
                    break;
                case 1:
                    Content_Frame.Content = WWarehouse;
                    break;
                case 2:
                    Content_Frame.Content = WContactors;
                    break;
                case 3:
                    Content_Frame.Content = WAnalyze;
                    break;
                case 4:
                    Content_Frame.Content = WRemanent;
                    break;
                case 5:
                    Content_Frame.Content = WSettings;
                    break;
                default:
                    break;
            }
        }
        private void HandleNavigating(Object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Forward || e.NavigationMode == NavigationMode.Back)
            {
                e.Cancel = true;
            }
        }
        private void Logout_Clicked(object sender, RoutedEventArgs e)
        {
            Confirmation confirm = new Confirmation("Logout");
            confirm.Owner = this;
            confirm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            bool? data = confirm.ShowDialog();
            if (data.HasValue && data.Value)
            {
                Logout();
            }
        }
        public void Logout()
        {
            LoginModel.isLogged = false;
            Menu_List.SelectedIndex = -1;
            WAuth = new Auth(LoginModel);
            Content_Frame.Content = WAuth;
            Collections.ResetCollections();
            WInvoice = null;
            WWarehouse = null;
            WContactors = null;
            WAnalyze = null;
            WSettings = null;
        }

    }
}
