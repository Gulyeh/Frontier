using Frontier.Variables;
using Frontier.Windows.Invoices_Window.Adjustment_Window;
using Frontier.Windows.Invoices_Window.Archive_Window;
using Frontier.Windows.Invoices_Window.NewInvoice_Window;
using Frontier.Windows.Invoices_Window.Purchase_Window;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

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
            GlobalVariables.InvoicePage = "Archive";
        }

        private void Invoice_Clicked(object sender, RoutedEventArgs e)
        {
            Invoice_Content.Content = WInvoice;
            GlobalVariables.InvoicePage = "NewInvoice";
        }
        private void Adjustment_Clicked(object sender, RoutedEventArgs e)
        {
            Invoice_Content.Content = WAdjustment;
            GlobalVariables.InvoicePage = "Correction";
        }
        private void Purchase_Clicked(object sender, RoutedEventArgs e)
        {
            Invoice_Content.Content = WPurchase;
            GlobalVariables.InvoicePage = "Purchase";
        }
        private void Archive_Clicked(object sender, RoutedEventArgs e)
        {
            Invoice_Content.Content = WArchive;
            GlobalVariables.InvoicePage = "Archive";
        }
        private void HandleNavigating(Object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Forward || e.NavigationMode == NavigationMode.Back)
            {
                e.Cancel = true;
            }
        }
    }
}
