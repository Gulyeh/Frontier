using Frontier.Classes;
using Frontier.Variables;
using Frontier.ViewModels;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using LiczbyNaSlowaNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frontier.Methods.Invoices
{
    class Generate_Invoice
    {
        public static async Task CreateInvoice(InvoiceData invoicedata, string Destination)
        {
            var buyer = Collections.ContactorsData.FirstOrDefault(x => x.ID == invoicedata.BuyerID);
            string FONT = AppDomain.CurrentDomain.BaseDirectory + "Font\\Calibri.ttf";
            PdfFont font = PdfFontFactory.CreateFont(FONT, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            PdfWriter writer = new PdfWriter(Destination);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf).SetFont(font);

            switch (invoicedata.InvoiceType)
            {
                case "Kupno":
                case "Sprzedaż":
                    await BasicInvoice(invoicedata, Destination, document, buyer);
                    break;
                case "Proforma":
                    await ProformInvoice(invoicedata, Destination, document, buyer);
                    break;
                case "VAT Marża":
                    await MarginInvoice(invoicedata, Destination, document, buyer);
                    break;
                default:
                    break;
            }
        }
        private static async Task BasicInvoice(InvoiceData invoicedata, string Destination, Document document, Contactors_ViewModel buyer)
        {
            await Task.Run(() =>
            {
                Paragraph header = new Paragraph("Faktura")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetMultipliedLeading(0.5f);
                document.Add(header);

                Paragraph subheader = new Paragraph("Nr: " + invoicedata.InvoiceNumber + "\n\n")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12)
                    .SetMultipliedLeading(1.0f);
                document.Add(subheader);

                Paragraph InvoiceState = new Paragraph()
                    .SetFontSize(8)
                    .SetMultipliedLeading(0.5f);
                InvoiceState.Add(new Tab());
                InvoiceState.AddTabStops(new TabStop(430, TabAlignment.RIGHT));
                InvoiceState.Add("Miejsce wystawienia:");
                InvoiceState.Add(new Tab());
                InvoiceState.AddTabStops(new TabStop(0, TabAlignment.RIGHT));
                InvoiceState.Add(Collections.CompanyData["State"]);
                document.Add(InvoiceState);


                Paragraph InvoiceCreated = new Paragraph()
                    .SetFontSize(8)
                    .SetMultipliedLeading(0.5f);
                InvoiceCreated.Add(new Tab());
                InvoiceCreated.AddTabStops(new TabStop(430, TabAlignment.RIGHT));
                InvoiceCreated.Add("Data wystawienia:");
                InvoiceCreated.Add(new Tab());
                InvoiceCreated.AddTabStops(new TabStop(0, TabAlignment.RIGHT));
                InvoiceCreated.Add(invoicedata.InvoiceDate);
                document.Add(InvoiceCreated);


                Paragraph InvoiceSold = new Paragraph()
                    .SetFontSize(8)
                    .SetMultipliedLeading(0.5f);
                InvoiceSold.Add(new Tab());
                InvoiceSold.AddTabStops(new TabStop(430, TabAlignment.RIGHT));
                InvoiceSold.Add("Data sprzedaży:");
                InvoiceSold.Add(new Tab());
                InvoiceSold.AddTabStops(new TabStop(0, TabAlignment.RIGHT));
                InvoiceSold.Add(invoicedata.SellDate);
                document.Add(InvoiceSold);

                Paragraph empty = new Paragraph();
                document.Add(empty);
                document.Add(empty);

                Paragraph p = new Paragraph()
                    .SetFontSize(12);
                p.Add(new Tab());
                p.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                p.Add("Sprzedawca");
                p.Add(new Tab());
                p.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                p.Add("Nabywca");
                document.Add(p);

                LineSeparator ls = new LineSeparator(new SolidLine());
                document.Add(ls);
                document.Add(empty);

                Paragraph a = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                a.Add(new Tab());
                a.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                a.Add(Collections.CompanyData["Name"]);
                a.Add(new Tab());
                a.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                a.Add(buyer.Name);
                document.Add(a);

                Paragraph b = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                b.Add(new Tab());
                b.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                b.Add(Collections.CompanyData["Street"]);
                b.Add(new Tab());
                b.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                b.Add(buyer.Address);
                document.Add(b);

                Paragraph c = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                c.Add(new Tab());
                c.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                c.Add(Collections.CompanyData["PostCode"] + " " + Collections.CompanyData["State"]);
                c.Add(new Tab());
                c.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                c.Add(buyer.PostCode + " " + buyer.State);
                document.Add(c);

                Paragraph d = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                d.Add(new Tab());
                d.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                d.Add("NIP " + Collections.CompanyData["NIP"]);
                d.Add(new Tab());
                d.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                d.Add("NIP " + buyer.NIP);
                document.Add(d);

                Paragraph e = new Paragraph()
                        .SetFontSize(9)
                        .SetMultipliedLeading(0.7f);
                if (Collections.CompanyData["REGON"] != string.Empty)
                {
                    e.Add(new Tab());
                    e.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                    e.Add("REGON " + Collections.CompanyData["REGON"]);
                }
                if (buyer.Regon != string.Empty)
                {
                    e.Add(new Tab());
                    e.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                    e.Add("REGON " + buyer.Regon);
                }

                Paragraph z = new Paragraph()
                        .SetFontSize(9)
                        .SetMultipliedLeading(0.7f);
                if (Collections.CompanyData["BDO"] != string.Empty)
                {
                    z.Add(new Tab());
                    z.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                    z.Add("BDO " + Collections.CompanyData["BDO"]);
                }

                document.Add(e);
                document.Add(z);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);

                Table table = new Table(UnitValue.CreatePercentArray(new float[] { 5, 30, 6, 6, 15, 6, 15, 15, 15, 15 })).SetFontSize(7).SetTextAlignment(TextAlignment.CENTER);
                table.SetWidth(UnitValue.CreatePercentValue(100));
                table.AddHeaderCell("L.p");
                table.AddHeaderCell("Nazwa towaru/usługi");
                table.AddHeaderCell("J.m");
                table.AddHeaderCell("Ilość");
                table.AddHeaderCell("Cena netto");
                table.AddHeaderCell("VAT");
                table.AddHeaderCell("Cena brutto");
                table.AddHeaderCell("Wartość netto");
                table.AddHeaderCell("Wartość VAT");
                table.AddHeaderCell("Wartość brutto");

                int iter = 1;
                List<ProductsSold_ViewModel> checkduplicates = new List<ProductsSold_ViewModel>();
                List<ProductsSold_ViewModel> Collection;
                if (GlobalVariables.InvoicePage == "Archive")
                {
                    Collection = Collections.ArchiveInvoices_Products;
                }
                else if (GlobalVariables.InvoicePage == "NewInvoice")
                {
                    Collection = Collections.ProductsSold.ToList();
                }
                else
                {
                    Collection = Collections.Products_Correction.ToList();
                }

                foreach (ProductsSold_ViewModel data in Collection)
                {
                    var query = checkduplicates.FirstOrDefault(x => x.Name == data.Name && x.PieceBrutto == data.PieceBrutto && x.VAT == data.VAT && x.PieceNetto == data.PieceNetto);
                    if (query == null)
                    {
                        checkduplicates.Add(data);
                    }
                    else
                    {
                        query.Amount += data.Amount;
                        query.Netto += data.Netto;
                        query.Brutto += data.Brutto;
                        query.VATAmount = query.Brutto - query.Netto;
                    }
                }

                foreach (ProductsSold_ViewModel data in checkduplicates)
                {
                    table.AddCell(iter.ToString() + ".");
                    table.AddCell(data.Name + "\n( GTU: " + data.GTU + " )");
                    table.AddCell("szt.");
                    table.AddCell(data.Amount.ToString());
                    table.AddCell(data.PieceNetto.ToString());
                    table.AddCell(data.VAT);
                    table.AddCell(data.PieceBrutto.ToString());
                    table.AddCell(data.Netto.ToString());
                    table.AddCell(data.VATAmount.ToString());
                    table.AddCell(data.Brutto.ToString());
                    iter++;
                }

                document.Add(table);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                checkduplicates.Clear();

                Paragraph f = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                f.Add("Razem:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                f.Add(new Tab());
                f.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                f.Add(invoicedata.TotalPrice + " " + invoicedata.Currency);
                document.Add(f);

                Paragraph g = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                g.Add("Kwota słownie:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                g.Add(new Tab());
                g.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                g.Add(NumberToText.Convert(decimal.Parse(invoicedata.TotalPrice.Split(',')[0])) + " " + invoicedata.TotalPrice.Split(',')[1] + "/100");
                document.Add(g);

                Paragraph h = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(4f)
                    .SetBold();
                h.Add("Do zapłaty / zapłacono:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                h.Add(new Tab());
                h.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                decimal calc = decimal.Parse(invoicedata.TotalPrice) - decimal.Parse(invoicedata.PaidPrice);
                h.Add(calc.ToString() + " " + invoicedata.Currency + " / " + invoicedata.PaidPrice + " " + invoicedata.Currency);
                h.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                document.Add(h);

                Paragraph i = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                i.Add("Forma płatności:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                i.Add(new Tab());
                i.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                i.Add(invoicedata.Payment);
                document.Add(i);

                Paragraph j = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                Paragraph k = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                Paragraph l = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);

                if (invoicedata.PaymentDays != null && invoicedata.PaymentDays != string.Empty)
                {
                    j.Add("Termin płatności:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                    j.Add(new Tab());
                    j.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                    j.Add(Convert.ToDateTime(invoicedata.InvoiceDate).AddDays(double.Parse(invoicedata.PaymentDays)).ToString("d"));
                }

                if (invoicedata.Payment == "Przelew")
                {
                    k.Add("Numer konta:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                    k.Add(new Tab());
                    k.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                    k.Add(invoicedata.AccountNumber);

                    l.Add("Bank:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                    l.Add(new Tab());
                    l.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                    l.Add(invoicedata.BankName);
                }
                document.Add(j);
                document.Add(k);
                document.Add(l);
                document.Add(empty);
                document.Add(empty);


                Paragraph m = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                m.Add("Uwagi:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                m.Add(new Tab());
                m.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                m.Add(invoicedata.Description);
                document.Add(m);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);

                Paragraph n = new Paragraph()
                        .SetMultipliedLeading(0.5f);
                LineSeparator ls1 = new LineSeparator(new SolidLine()).SetWidth(150);
                n.Add(new Tab());
                n.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                n.Add(ls1);
                n.Add(new Tab());
                n.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                n.Add(ls1);
                document.Add(n);

                Paragraph o = new Paragraph()
                    .SetFontSize(6);
                o.Add(new Tab());
                o.AddTabStops(new TabStop(45, TabAlignment.LEFT));
                o.Add("Osoba upoważniona do wystawienia");
                o.Add(new Tab());
                o.AddTabStops(new TabStop(385, TabAlignment.LEFT));
                o.Add("Osoba upoważniona do odbioru");
                document.Add(o);
                document.Close();
            });
        }
        private static async Task ProformInvoice(InvoiceData invoicedata, string Destination, Document document, Contactors_ViewModel buyer)
        {
            await Task.Run(() =>
            {
                Paragraph header = new Paragraph("Faktura pro-forma")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetMultipliedLeading(0.5f);
                document.Add(header);

                Paragraph subheader = new Paragraph("Nr: " + invoicedata.InvoiceNumber + "\n\n")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12)
                    .SetMultipliedLeading(1.0f);
                document.Add(subheader);

                Paragraph InvoiceState = new Paragraph()
                    .SetFontSize(8)
                    .SetMultipliedLeading(0.5f);
                InvoiceState.Add(new Tab());
                InvoiceState.AddTabStops(new TabStop(430, TabAlignment.RIGHT));
                InvoiceState.Add("Miejsce wystawienia:");
                InvoiceState.Add(new Tab());
                InvoiceState.AddTabStops(new TabStop(0, TabAlignment.RIGHT));
                InvoiceState.Add(Collections.CompanyData["State"]);
                document.Add(InvoiceState);


                Paragraph InvoiceCreated = new Paragraph()
                    .SetFontSize(8)
                    .SetMultipliedLeading(0.5f);
                InvoiceCreated.Add(new Tab());
                InvoiceCreated.AddTabStops(new TabStop(430, TabAlignment.RIGHT));
                InvoiceCreated.Add("Data wystawienia:");
                InvoiceCreated.Add(new Tab());
                InvoiceCreated.AddTabStops(new TabStop(0, TabAlignment.RIGHT));
                InvoiceCreated.Add(invoicedata.InvoiceDate);
                document.Add(InvoiceCreated);

                if (invoicedata.SellDate != null && invoicedata.SellDate != string.Empty)
                {
                    Paragraph InvoiceSold = new Paragraph()
                        .SetFontSize(8)
                        .SetMultipliedLeading(0.5f);
                    InvoiceSold.Add(new Tab());
                    InvoiceSold.AddTabStops(new TabStop(430, TabAlignment.RIGHT));
                    InvoiceSold.Add("Data sprzedaży:");
                    InvoiceSold.Add(new Tab());
                    InvoiceSold.AddTabStops(new TabStop(0, TabAlignment.RIGHT));
                    InvoiceSold.Add(invoicedata.SellDate);
                    document.Add(InvoiceSold);
                }

                Paragraph empty = new Paragraph();
                document.Add(empty);
                document.Add(empty);

                Paragraph p = new Paragraph()
                    .SetFontSize(12);
                p.Add(new Tab());
                p.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                p.Add("Sprzedawca");
                p.Add(new Tab());
                p.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                p.Add("Nabywca");
                document.Add(p);

                LineSeparator ls = new LineSeparator(new SolidLine());
                document.Add(ls);
                document.Add(empty);

                Paragraph a = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                a.Add(new Tab());
                a.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                a.Add(Collections.CompanyData["Name"]);
                a.Add(new Tab());
                a.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                a.Add(buyer.Name);
                document.Add(a);

                Paragraph b = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                b.Add(new Tab());
                b.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                b.Add(Collections.CompanyData["Street"]);
                b.Add(new Tab());
                b.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                b.Add(buyer.Address);
                document.Add(b);

                Paragraph c = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                c.Add(new Tab());
                c.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                c.Add(Collections.CompanyData["PostCode"] + " " + Collections.CompanyData["State"]);
                c.Add(new Tab());
                c.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                c.Add(buyer.PostCode + " " + buyer.State);
                document.Add(c);

                Paragraph d = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                d.Add(new Tab());
                d.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                d.Add("NIP " + Collections.CompanyData["NIP"]);
                d.Add(new Tab());
                d.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                d.Add("NIP " + buyer.NIP);
                document.Add(d);

                Paragraph e = new Paragraph()
                        .SetFontSize(9)
                        .SetMultipliedLeading(0.7f);
                if (Collections.CompanyData["REGON"] != string.Empty)
                {
                    e.Add(new Tab());
                    e.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                    e.Add("REGON " + Collections.CompanyData["REGON"]);
                }
                if (buyer.Regon != string.Empty)
                {
                    e.Add(new Tab());
                    e.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                    e.Add("REGON " + buyer.Regon);
                }

                Paragraph z = new Paragraph()
                        .SetFontSize(9)
                        .SetMultipliedLeading(0.7f);
                if (Collections.CompanyData["BDO"] != string.Empty)
                {
                    z.Add(new Tab());
                    z.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                    z.Add("BDO " + Collections.CompanyData["BDO"]);
                }

                document.Add(e);
                document.Add(z);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);

                Table table = new Table(UnitValue.CreatePercentArray(new float[] { 5, 30, 6, 6, 15, 6, 15, 15, 15, 15 })).SetFontSize(7).SetTextAlignment(TextAlignment.CENTER);
                table.SetWidth(UnitValue.CreatePercentValue(100));
                table.AddHeaderCell("L.p");
                table.AddHeaderCell("Nazwa towaru/usługi");
                table.AddHeaderCell("J.m");
                table.AddHeaderCell("Ilość");
                table.AddHeaderCell("Cena netto");
                table.AddHeaderCell("VAT");
                table.AddHeaderCell("Cena brutto");
                table.AddHeaderCell("Wartość netto");
                table.AddHeaderCell("Wartość VAT");
                table.AddHeaderCell("Wartość brutto");

                int iter = 1;
                List<ProductsSold_ViewModel> checkduplicates = new List<ProductsSold_ViewModel>();
                List<ProductsSold_ViewModel> Collection;
                if (GlobalVariables.InvoicePage == "Archive")
                {
                    Collection = Collections.ArchiveInvoices_Products;
                }
                else if (GlobalVariables.InvoicePage == "NewInvoice")
                {
                    Collection = Collections.ProductsSold.ToList();
                }
                else
                {
                    Collection = Collections.Products_Correction.ToList();
                }

                foreach (ProductsSold_ViewModel data in Collection)
                {
                    var query = checkduplicates.FirstOrDefault(x => x.Name == data.Name && x.PieceBrutto == data.PieceBrutto && x.VAT == data.VAT && x.PieceNetto == data.PieceNetto);
                    if (query == null)
                    {
                        checkduplicates.Add(data);
                    }
                    else
                    {
                        query.Amount += data.Amount;
                        query.Netto += data.Netto;
                        query.Brutto += data.Brutto;
                        query.VATAmount = query.Brutto - query.Netto;
                    }
                }

                foreach (ProductsSold_ViewModel data in checkduplicates)
                {
                    table.AddCell(iter.ToString() + ".");
                    table.AddCell(data.Name + "\n( GTU: " + data.GTU + " )");
                    table.AddCell("szt.");
                    table.AddCell(data.Amount.ToString());
                    table.AddCell(data.PieceNetto.ToString());
                    table.AddCell(data.VAT);
                    table.AddCell(data.PieceBrutto.ToString());
                    table.AddCell(data.Netto.ToString());
                    table.AddCell(data.VATAmount.ToString());
                    table.AddCell(data.Brutto.ToString());
                    iter++;
                }

                document.Add(table);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);

                Paragraph f = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                f.Add("Razem:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                f.Add(new Tab());
                f.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                f.Add(invoicedata.TotalPrice + " " + invoicedata.Currency);
                document.Add(f);

                Paragraph g = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                g.Add("Kwota słownie:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                g.Add(new Tab());
                g.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                g.Add(NumberToText.Convert(decimal.Parse(invoicedata.TotalPrice.Split(',')[0])) + " " + invoicedata.TotalPrice.Split(',')[1] + "/100");
                document.Add(g);

                Paragraph h = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(4f)
                    .SetBold();
                h.Add("Do zapłaty / zapłacono:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                h.Add(new Tab());
                h.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                decimal calc = decimal.Parse(invoicedata.TotalPrice) - decimal.Parse(invoicedata.PaidPrice);
                h.Add(calc.ToString() + " " + invoicedata.Currency + " / " + invoicedata.PaidPrice + " " + invoicedata.Currency);
                h.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                document.Add(h);

                Paragraph i = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                i.Add("Forma płatności:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                i.Add(new Tab());
                i.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                i.Add(invoicedata.Payment);
                document.Add(i);

                Paragraph j = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                j.Add("Termin płatności:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                j.Add(new Tab());
                j.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                j.Add(Convert.ToDateTime(invoicedata.InvoiceDate).AddDays(double.Parse(invoicedata.PaymentDays)).ToString("d"));
                document.Add(j);

                Paragraph k = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                Paragraph l = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);

                if (invoicedata.Payment == "Przelew")
                {
                    k.Add("Numer konta:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                    k.Add(new Tab());
                    k.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                    k.Add(invoicedata.AccountNumber);

                    l.Add("Bank:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                    l.Add(new Tab());
                    l.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                    l.Add(invoicedata.BankName);
                }
                document.Add(k);
                document.Add(l);
                document.Add(empty);
                document.Add(empty);


                Paragraph m = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                m.Add("Uwagi:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                m.Add(new Tab());
                m.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                m.Add(invoicedata.Description);
                document.Add(m);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);

                Paragraph n = new Paragraph()
                        .SetMultipliedLeading(0.5f);
                LineSeparator ls1 = new LineSeparator(new SolidLine()).SetWidth(150);
                n.Add(new Tab());
                n.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                n.Add(ls1);
                n.Add(new Tab());
                n.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                n.Add(ls1);
                document.Add(n);

                Paragraph o = new Paragraph()
                    .SetFontSize(6);
                o.Add(new Tab());
                o.AddTabStops(new TabStop(45, TabAlignment.LEFT));
                o.Add("Osoba upoważniona do wystawienia");
                o.Add(new Tab());
                o.AddTabStops(new TabStop(385, TabAlignment.LEFT));
                o.Add("Osoba upoważniona do odbioru");
                document.Add(o);
                document.Close();
            });
        }
        private static async Task MarginInvoice(InvoiceData invoicedata, string Destination, Document document, Contactors_ViewModel buyer)
        {
            await Task.Run(() =>
            {
                Paragraph header = new Paragraph("Faktura VAT Marża")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetMultipliedLeading(0.5f);
                document.Add(header);

                Paragraph subheader = new Paragraph("Nr: " + invoicedata.InvoiceNumber + "\n\n")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12)
                    .SetMultipliedLeading(1.0f);
                document.Add(subheader);

                Paragraph InvoiceState = new Paragraph()
                    .SetFontSize(8)
                    .SetMultipliedLeading(0.5f);
                InvoiceState.Add(new Tab());
                InvoiceState.AddTabStops(new TabStop(430, TabAlignment.RIGHT));
                InvoiceState.Add("Miejsce wystawienia:");
                InvoiceState.Add(new Tab());
                InvoiceState.AddTabStops(new TabStop(0, TabAlignment.RIGHT));
                InvoiceState.Add(Collections.CompanyData["State"]);
                document.Add(InvoiceState);


                Paragraph InvoiceCreated = new Paragraph()
                    .SetFontSize(8)
                    .SetMultipliedLeading(0.5f);
                InvoiceCreated.Add(new Tab());
                InvoiceCreated.AddTabStops(new TabStop(430, TabAlignment.RIGHT));
                InvoiceCreated.Add("Data wystawienia:");
                InvoiceCreated.Add(new Tab());
                InvoiceCreated.AddTabStops(new TabStop(0, TabAlignment.RIGHT));
                InvoiceCreated.Add(invoicedata.InvoiceDate);
                document.Add(InvoiceCreated);


                Paragraph InvoiceSold = new Paragraph()
                    .SetFontSize(8)
                    .SetMultipliedLeading(0.5f);
                InvoiceSold.Add(new Tab());
                InvoiceSold.AddTabStops(new TabStop(430, TabAlignment.RIGHT));
                InvoiceSold.Add("Data sprzedaży:");
                InvoiceSold.Add(new Tab());
                InvoiceSold.AddTabStops(new TabStop(0, TabAlignment.RIGHT));
                InvoiceSold.Add(invoicedata.SellDate);
                document.Add(InvoiceSold);

                Paragraph empty = new Paragraph();
                document.Add(empty);
                document.Add(empty);

                Paragraph p = new Paragraph()
                    .SetFontSize(12);
                p.Add(new Tab());
                p.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                p.Add("Sprzedawca");
                p.Add(new Tab());
                p.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                p.Add("Nabywca");
                document.Add(p);

                LineSeparator ls = new LineSeparator(new SolidLine());
                document.Add(ls);
                document.Add(empty);

                Paragraph a = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                a.Add(new Tab());
                a.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                a.Add(Collections.CompanyData["Name"]);
                a.Add(new Tab());
                a.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                a.Add(buyer.Name);
                document.Add(a);

                Paragraph b = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                b.Add(new Tab());
                b.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                b.Add(Collections.CompanyData["Street"]);
                b.Add(new Tab());
                b.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                b.Add(buyer.Address);
                document.Add(b);

                Paragraph c = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                c.Add(new Tab());
                c.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                c.Add(Collections.CompanyData["PostCode"] + " " + Collections.CompanyData["State"]);
                c.Add(new Tab());
                c.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                c.Add(buyer.PostCode + " " + buyer.State);
                document.Add(c);

                Paragraph d = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                d.Add(new Tab());
                d.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                d.Add("NIP " + Collections.CompanyData["NIP"]);
                d.Add(new Tab());
                d.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                d.Add("NIP " + buyer.NIP);
                document.Add(d);

                Paragraph e = new Paragraph()
                        .SetFontSize(9)
                        .SetMultipliedLeading(0.7f);
                if (Collections.CompanyData["REGON"] != string.Empty)
                {
                    e.Add(new Tab());
                    e.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                    e.Add("REGON " + Collections.CompanyData["REGON"]);
                }
                if (buyer.Regon != string.Empty)
                {
                    e.Add(new Tab());
                    e.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                    e.Add("REGON " + buyer.Regon);
                }

                Paragraph z = new Paragraph()
                        .SetFontSize(9)
                        .SetMultipliedLeading(0.7f);
                if (Collections.CompanyData["BDO"] != string.Empty)
                {
                    z.Add(new Tab());
                    z.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                    z.Add("BDO " + Collections.CompanyData["BDO"]);
                }

                document.Add(e);
                document.Add(z);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);

                Table table = new Table(UnitValue.CreatePercentArray(new float[] { 5, 90, 6, 6, 25, 25 })).SetFontSize(7).SetTextAlignment(TextAlignment.CENTER);
                table.SetWidth(UnitValue.CreatePercentValue(100));
                table.AddHeaderCell("L.p");
                table.AddHeaderCell("Nazwa towaru/usługi");
                table.AddHeaderCell("J.m");
                table.AddHeaderCell("Ilość");
                table.AddHeaderCell("Wartość netto");
                table.AddHeaderCell("Wartość brutto");

                int iter = 1;
                List<ProductsSold_ViewModel> checkduplicates = new List<ProductsSold_ViewModel>();
                List<ProductsSold_ViewModel> Collection;
                if (GlobalVariables.InvoicePage == "Archive")
                {
                    Collection = Collections.ArchiveInvoices_Products;
                }
                else if (GlobalVariables.InvoicePage == "NewInvoice")
                {
                    Collection = Collections.ProductsSold.ToList();
                }
                else
                {
                    Collection = Collections.Products_Correction.ToList();
                }

                foreach (ProductsSold_ViewModel data in Collection)
                {
                    var query = checkduplicates.FirstOrDefault(x => x.Name == data.Name && x.PieceBrutto == data.PieceBrutto && x.VAT == data.VAT && x.PieceNetto == data.PieceNetto);
                    if (query == null)
                    {
                        checkduplicates.Add(data);
                    }
                    else
                    {
                        query.Amount += data.Amount;
                        query.Netto += data.Netto;
                        query.Brutto += data.Brutto;
                    }
                }

                foreach (ProductsSold_ViewModel data in checkduplicates)
                {
                    table.AddCell(iter.ToString() + ".");
                    table.AddCell(data.Name + "\n( GTU: " + data.GTU + " )");
                    table.AddCell("szt.");
                    table.AddCell(data.Amount.ToString());
                    table.AddCell(data.Netto.ToString());
                    table.AddCell(data.Brutto.ToString());
                    iter++;
                }

                document.Add(table);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);

                Paragraph f = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                f.Add("Razem:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                f.Add(new Tab());
                f.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                f.Add(invoicedata.TotalPrice + " " + invoicedata.Currency);
                document.Add(f);

                Paragraph g = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                g.Add("Kwota słownie:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                g.Add(new Tab());
                g.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                g.Add(NumberToText.Convert(decimal.Parse(invoicedata.TotalPrice.Split(',')[0])) + " " + invoicedata.TotalPrice.Split(',')[1] + "/100");
                document.Add(g);

                Paragraph h = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(4f)
                    .SetBold();
                h.Add("Do zapłaty / zapłacono:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                h.Add(new Tab());
                h.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                decimal calc = decimal.Parse(invoicedata.TotalPrice) - decimal.Parse(invoicedata.PaidPrice);
                h.Add(calc.ToString() + " " + invoicedata.Currency + " / " + invoicedata.PaidPrice + " " + invoicedata.Currency);
                h.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                document.Add(h);

                Paragraph i = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                i.Add("Forma płatności:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                i.Add(new Tab());
                i.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                i.Add(invoicedata.Payment);
                document.Add(i);

                Paragraph j = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                Paragraph k = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                Paragraph l = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);

                if (invoicedata.PaymentDays != null && invoicedata.PaymentDays != string.Empty)
                {
                    j.Add("Termin płatności:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                    j.Add(new Tab());
                    j.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                    j.Add(Convert.ToDateTime(invoicedata.InvoiceDate).AddDays(double.Parse(invoicedata.PaymentDays)).ToString("d"));
                }

                if (invoicedata.Payment == "Przelew")
                {
                    k.Add("Numer konta:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                    k.Add(new Tab());
                    k.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                    k.Add(invoicedata.AccountNumber);

                    l.Add("Bank:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                    l.Add(new Tab());
                    l.AddTabStops(new TabStop(200, TabAlignment.LEFT));
                    l.Add(invoicedata.BankName);
                }

                document.Add(j);
                document.Add(k);
                document.Add(l);
                document.Add(empty);
                document.Add(empty);


                Paragraph m = new Paragraph()
                    .SetFontSize(9)
                    .SetMultipliedLeading(0.7f);
                m.Add("Uwagi:").SetHorizontalAlignment(HorizontalAlignment.LEFT);
                m.Add(new Tab());
                m.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                m.Add(invoicedata.Description);
                document.Add(m);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);
                document.Add(empty);

                Paragraph n = new Paragraph()
                        .SetMultipliedLeading(0.5f);
                LineSeparator ls1 = new LineSeparator(new SolidLine()).SetWidth(150);
                n.Add(new Tab());
                n.AddTabStops(new TabStop(20, TabAlignment.LEFT));
                n.Add(ls1);
                n.Add(new Tab());
                n.AddTabStops(new TabStop(350, TabAlignment.LEFT));
                n.Add(ls1);
                document.Add(n);

                Paragraph o = new Paragraph()
                    .SetFontSize(6);
                o.Add(new Tab());
                o.AddTabStops(new TabStop(45, TabAlignment.LEFT));
                o.Add("Osoba upoważniona do wystawienia");
                o.Add(new Tab());
                o.AddTabStops(new TabStop(385, TabAlignment.LEFT));
                o.Add("Osoba upoważniona do odbioru");
                document.Add(o);
                document.Close();
            });
        }
        public static async Task ConvertWord(string savedestination, string userdestination)
        {
            await Task.Run(async () =>
            {
                Spire.Pdf.PdfDocument doc = new Spire.Pdf.PdfDocument();
                doc.LoadFromFile(savedestination + "\\Invoice.pdf");
                doc.SaveToFile(userdestination, Spire.Pdf.FileFormat.DOCX);
                await PermaDelete.Delete(savedestination, "Invoice.pdf");
            });
        }
    }
}
