using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;
using Frontier.Methods;
using Frontier.Variables;
using Spire.Pdf;

namespace Frontier.Windows.Invoices_Window.Archive_Window
{
    /// <summary>
    /// Logika interakcji dla klasy PdfViewer.xaml
    /// </summary>
    public partial class PdfViewer : Window
    {
        Package package;
        Uri packageUri;
        public PdfViewer()
        {
            InitializeComponent();
        }

        private async void Viewer_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                { 
                    PdfDocument Invoice = new PdfDocument("InvoicePreview.pdf");
                    Invoice.SaveToFile("InvoicePreview.xps", FileFormat.XPS);
                    Invoice.Close();
                    packageUri = new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\InvoicePreview.xps");
                    package = Package.Open("InvoicePreview.xps", FileMode.OpenOrCreate);
                    PackageStore.AddPackage(packageUri, package);
                    XpsDocument xps = new XpsDocument(package, CompressionOption.SuperFast, packageUri.AbsoluteUri);
                    FixedDocumentSequence fds = xps.GetFixedDocumentSequence();
                    DocumentViewer.Document = fds;
                }));
            });
        }
        private async void BeforeClosing(object sender, CancelEventArgs e)
        {
            Collections.ArchiveInvoices_Products.Clear();
            package.Close();
            PackageStore.RemovePackage(packageUri);
            await PermaDelete.Delete(AppDomain.CurrentDomain.BaseDirectory, "InvoicePreview.pdf");
            await PermaDelete.Delete(AppDomain.CurrentDomain.BaseDirectory, "InvoicePreview.xps");
        }

    }
}
