using Frontier.Variables;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Frontier.Windows.Remanent_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Remanent.xaml
    /// </summary>
    public partial class Remanent : Page
    {
        public class RemanentData
        {
            public string Name { get; set; }
            public int Amount { get; set; }
            private decimal netto { get; set; }
            public decimal Netto
            {
                get { return netto; }
                set
                {
                    if (netto == value) return;
                    netto = Math.Round(decimal.Parse(value.ToString("F2")), 2);
                }
            }
            private decimal brutto { get; set; }
            public decimal Brutto
            {
                get { return brutto; }
                set
                {
                    if (brutto == value) return;
                    brutto = Math.Round(decimal.Parse(value.ToString("F2")), 2);
                }
            }
        };
        private decimal sumnetto { get; set; }
        public decimal SumNetto
        {
            get { return sumnetto; }
            set
            {
                if (sumnetto == value) return;
                sumnetto = Math.Round(decimal.Parse(value.ToString("F2")), 2);
            }
        }
        private decimal sumbrutto { get; set; }
        public decimal SumBrutto
        {
            get { return sumbrutto; }
            set
            {
                if (sumbrutto == value) return;
                sumbrutto = Math.Round(decimal.Parse(value.ToString("F2")), 2);
            }
        }

        ObservableCollection<RemanentData> RemanentItems;


        public Remanent()
        {
            InitializeComponent();
            RemanentItems = new ObservableCollection<RemanentData>();
            Warehouse_Grid.ItemsSource = RemanentItems;
        }
        private async void LoadData(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    SumNetto = 0;
                    SumBrutto = 0;
                    foreach (var item in Collections.WarehouseData.Where(x => x.GroupType != "Usługa"))
                    {
                        var data = RemanentItems.FirstOrDefault(x => x.Name == item.Name);
                        if (data != null)
                        {
                            data.Amount += item.Amount;
                            data.Netto += item.Netto * item.Amount;
                            data.Brutto += item.Brutto * item.Amount;
                        }
                        else
                        {
                            RemanentItems.Add(new RemanentData
                            {
                                Name = item.Name,
                                Amount = item.Amount,
                                Netto = item.Netto * item.Amount,
                                Brutto = item.Brutto * item.Amount
                            });

                        }
                        SumNetto += item.Netto * item.Amount;
                        SumBrutto += item.Brutto * item.Amount;
                    }
                    Netto.Text = SumNetto.ToString();
                    Brutto.Text = SumBrutto.ToString();
                }));
            });
        }
        private async void GenerateReport_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.SaveFileDialog exportSaveFileDialog = new System.Windows.Forms.SaveFileDialog();

                exportSaveFileDialog.Title = "Select Destination";
                exportSaveFileDialog.Filter = "PDF(*.pdf)|*.pdf";

                if (System.Windows.Forms.DialogResult.OK == exportSaveFileDialog.ShowDialog())
                {
                    ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                    await SaveReport(exportSaveFileDialog.FileName);
                    ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Błąd podczas zapisu");
                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
            }
        }
        private async Task SaveReport(string Destination)
        {
            await Task.Run(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string FONT = AppDomain.CurrentDomain.BaseDirectory + "//Font/Calibri.ttf";
                    PdfFont font = PdfFontFactory.CreateFont(FONT, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                    PdfWriter writer = new PdfWriter(Destination);
                    PdfDocument pdf = new PdfDocument(writer);
                    Document document = new Document(pdf).SetFont(font);

                    Paragraph empty = new Paragraph();

                    Paragraph header = new Paragraph("Raport magazynu")
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetFontSize(20)
                        .SetMultipliedLeading(0.5f);
                    document.Add(header);

                    Paragraph subheader = new Paragraph("Wygenerowano:\n" + DateTime.Now)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetFontSize(8)
                        .SetMultipliedLeading(1.0f);
                    document.Add(subheader);

                    document.Add(empty);
                    document.Add(empty);
                    document.Add(empty);

                    Table table = new Table(UnitValue.CreatePercentArray(new float[] { 5, 30, 6, 6, 15, 15 })).SetFontSize(7).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                    table.SetWidth(UnitValue.CreatePercentValue(100));
                    table.AddHeaderCell("L.p");
                    table.AddHeaderCell("Nazwa towaru");
                    table.AddHeaderCell("J.m");
                    table.AddHeaderCell("Ilość");
                    table.AddHeaderCell("Wartość netto");
                    table.AddHeaderCell("Wartość brutto");

                    int i = 1;
                    foreach (RemanentData data in Warehouse_Grid.Items)
                    {
                        table.AddCell(i.ToString() + ".");
                        table.AddCell(data.Name);
                        table.AddCell("szt.");
                        table.AddCell(data.Amount.ToString());
                        table.AddCell(data.Netto.ToString());
                        table.AddCell(data.Brutto.ToString());
                        i++;
                    }
                    document.Add(table);
                    document.Add(empty);
                    document.Add(empty);
                    document.Add(empty);
                    document.Add(empty);

                    Paragraph SumNetto = new Paragraph()
                            .SetFontSize(10)
                            .SetMultipliedLeading(0.5f);
                    SumNetto.Add(new Tab());
                    SumNetto.AddTabStops(new TabStop(430, TabAlignment.RIGHT));
                    SumNetto.Add("Suma Netto:");
                    SumNetto.Add(new Tab());
                    SumNetto.AddTabStops(new TabStop(0, TabAlignment.RIGHT));
                    SumNetto.Add(Netto.Text + " PLN");
                    document.Add(SumNetto);

                    Paragraph SumBrutto = new Paragraph()
                            .SetFontSize(10)
                            .SetMultipliedLeading(0.5f);
                    SumBrutto.Add(new Tab());
                    SumBrutto.AddTabStops(new TabStop(430, TabAlignment.RIGHT));
                    SumBrutto.Add("Suma Brutto:");
                    SumBrutto.Add(new Tab());
                    SumBrutto.AddTabStops(new TabStop(0, TabAlignment.RIGHT));
                    SumBrutto.Add(Brutto.Text + " PLN");
                    document.Add(SumBrutto);
                    document.Close();
                }));
            });
        }
    }
}
