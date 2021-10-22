using Frontier.Windows.Invoices_Window.Adjustment_Window;
using Frontier.Windows.Invoices_Window.Archive_Window;
using Frontier.Windows.Invoices_Window.NewInvoice_Window;
using Frontier.Windows.Invoices_Window.Purchase_Window;
using System.Windows;
using System.Windows.Controls;

namespace Frontier.Windows.Invoices_Window
{
    public partial class Invoices : Page
    {
        NewInvoice WInvoice = new NewInvoice();
        Archive WArchive = new Archive();
        Adjustment WAdjustment = new Adjustment();
        Purchase WPurchase = new Purchase();

        public Invoices()
        {
            InitializeComponent();
            Invoice_Content.Content = WArchive;
        }

        private void Invoice_Clicked(object sender, RoutedEventArgs e)
        {
            Invoice_Content.Content = WInvoice;
        }
        private void Adjustment_Clicked(object sender, RoutedEventArgs e)
        {
            Invoice_Content.Content = WAdjustment;
        }
        private void Purchase_Clicked(object sender, RoutedEventArgs e)
        {
            Invoice_Content.Content = WPurchase;
        }
        private void Archive_Clicked(object sender, RoutedEventArgs e)
        {
            Invoice_Content.Content = WArchive;
        }
    }
}
