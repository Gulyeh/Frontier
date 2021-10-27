using Frontier.Variables;
using Frontier.ViewModels;
using Frontier.Windows.Analyze_Window;
using Frontier.Windows.Auth_Window;
using Frontier.Windows.Confirmation_Window;
using Frontier.Windows.Contactors_Window;
using Frontier.Windows.Invoices_Window;
using Frontier.Windows.Settings_Window;
using Frontier.Windows.Warehouse_Window;
using System.Windows;

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

        public void LoadPages()
        {
            WInvoice = new Invoices();
            WWarehouse = new Warehouse();
            WContactors = new Contactors();
            WAnalyze = new Analyze();
            WSettings = new Settings();
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
                    Content_Frame.Content = WSettings;
                    break;
                default:
                    break;
            }
        }

        private void Logout_Clicked(object sender, RoutedEventArgs e)
        {
            Confirmation confirm = new Confirmation("Logout");
            confirm.Owner = this;
            confirm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            confirm.ShowDialog();
        }
    }
}
