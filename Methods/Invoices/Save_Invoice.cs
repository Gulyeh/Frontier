using Frontier.Classes;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;

namespace Frontier.Methods.Invoices
{
    internal class Save_Invoice
    {
        public static async Task SaveWord(InvoiceData _InvoiceData)
        {
            System.Windows.Forms.SaveFileDialog exportSaveFileDialog = new System.Windows.Forms.SaveFileDialog();

            exportSaveFileDialog.Title = "Select Destination";
            exportSaveFileDialog.Filter = "Word(*.docx)|*.docx";

            if (System.Windows.Forms.DialogResult.OK == exportSaveFileDialog.ShowDialog())
            {
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                await Generate_Invoice.CreateInvoice(_InvoiceData, AppDomain.CurrentDomain.BaseDirectory + "\\Invoice.pdf");
                await Generate_Invoice.ConvertWord(AppDomain.CurrentDomain.BaseDirectory, exportSaveFileDialog.FileName);
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
            }
        }
        public static async Task SavePDF(InvoiceData _InvoiceData)
        {
            System.Windows.Forms.SaveFileDialog exportSaveFileDialog = new System.Windows.Forms.SaveFileDialog();

            exportSaveFileDialog.Title = "Select Destination";
            exportSaveFileDialog.Filter = "PDF(*.pdf)|*.pdf";

            if (System.Windows.Forms.DialogResult.OK == exportSaveFileDialog.ShowDialog())
            {
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                await Generate_Invoice.CreateInvoice(_InvoiceData, exportSaveFileDialog.FileName);
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
            }
        }
        public static async Task Print(InvoiceData _InvoiceData)
        {
            using (System.Windows.Forms.PrintDialog dialogPrint = new System.Windows.Forms.PrintDialog())
            {
                if (System.Windows.Forms.DialogResult.OK == dialogPrint.ShowDialog())
                {
                    ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                    await Task.Run(async () =>
                    {
                        await Generate_Invoice.CreateInvoice(_InvoiceData, AppDomain.CurrentDomain.BaseDirectory + "\\Invoice.pdf");

                        PdfDocument pdf = new PdfDocument();
                        pdf.LoadFromFile(AppDomain.CurrentDomain.BaseDirectory + "\\Invoice.pdf");

                        PdfMargins pdfMargin = new PdfMargins();
                        pdfMargin.Left = 0;
                        pdfMargin.Right = 0;
                        pdfMargin.Top = 0;
                        pdfMargin.Bottom = 0;

                        PdfDocument doc1 = new PdfDocument();
                        foreach (PdfPageBase page in pdf.Pages)
                        {
                            SizeF size = page.Size;
                            PdfPageBase p = doc1.Pages.Add(new SizeF(size.Width + 50F, size.Height + 100F), pdfMargin);
                            page.CreateTemplate().Draw(p, 15, 0);
                        }
                        doc1.PrintSettings.PrinterName = dialogPrint.PrinterSettings.PrinterName;
                        doc1.Print();

                        await PermaDelete.Delete(AppDomain.CurrentDomain.BaseDirectory, "Invoice.pdf");
                    });
                    ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                }
            }
        }
    }
}
