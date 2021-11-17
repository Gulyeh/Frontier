using Frontier.Database.GetQuery;
using Frontier.Variables;
using Frontier.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using static Frontier.Windows.Invoices_Window.NewInvoice_Window.NewInvoice;

namespace Frontier.Methods
{
    class DownloadInvoice
    {
        public static decimal totalnetto { get; set; }
        public static decimal totalbrutto { get; set; }

        public static async Task<InvoiceData> GetData(string InvoiceType, int ID, string Page)
        {
            InvoiceData getInvoice = null;
            totalnetto = 0;
            totalbrutto = 0;

            await Task.Run(() =>
            {
                if (InvoiceType == "Kupno")
                {
                    getInvoice = BoughtInvoice(InvoiceType, ID, Page);
                }
                else
                {
                    getInvoice = SoldInvoice(InvoiceType, ID, Page);
                }
            });
            return getInvoice;
        }
        private static InvoiceData BoughtInvoice(string InvoiceType, int ID, string Page)
        {
            InvoiceData getInvoice = null;
            using (GetInvoices invoice = new GetInvoices())
            {
                using (GetInvoice_Products products = new GetInvoice_Products())
                {
                    var query = invoice.Invoice_Bought.FirstOrDefault(x => x.idinvoice_bought == ID);
                    getInvoice = new InvoiceData
                    {
                        InvoiceType = InvoiceType,
                        InvoiceNumber = query.Invoice_ID,
                        BuyerID = int.Parse(query.Seller_ID),
                        Payment = query.Purchase_type,
                        PaymentDays = null,
                        AccountNumber = null,
                        BankName = null,
                        SellDate = query.Date_Bought,
                        InvoiceDate = query.Date_Created.ToShortDateString(),
                        Currency = query.Currency,
                        Description = null
                    };

                    var queryProducts = products.Invoice_Products.Where(x => x.Invoice_ID == query.Invoice_ID).ToList();

                    foreach (var data in queryProducts)
                    {
                        var getProduct = new ProductsSold_ViewModel
                        {
                            Name = data.Name,
                            Amount = int.Parse(data.Amount),
                            PieceNetto = decimal.Parse(data.Each_Netto),
                            VAT = data.VAT,
                            PieceBrutto = decimal.Parse(data.Each_Brutto),
                            Netto = decimal.Parse(data.Netto),
                            VATAmount = decimal.Parse(data.VAT_Price),
                            Brutto = decimal.Parse(data.Brutto),
                            GTU = Collections.GroupsData.FirstOrDefault(x => x.ID == Collections.WarehouseData.FirstOrDefault(z => z.Name == data.Name).GroupID).GTU
                        };
                        totalnetto += decimal.Parse(data.Netto);
                        totalbrutto += decimal.Parse(data.Brutto);

                        if (Page == "Archive")
                        {
                            Collections.ArchiveInvoices_Products.Add(getProduct);
                        }
                        else
                        {
                            Collections.ProductsSold_Correction.Add(getProduct);
                        }
                    }
                    getInvoice.TotalPrice = totalbrutto.ToString();
                }
            }
            return getInvoice;
        }
        private static InvoiceData SoldInvoice(string InvoiceType, int ID, string Page)
        {
            InvoiceData getInvoice = null;
            using (GetInvoices invoice = new GetInvoices())
            {
                using (GetInvoice_Products products = new GetInvoice_Products())
                {
                    var query = invoice.Invoice_Sold.FirstOrDefault(x => x.idInvoice_Sold == ID);
                    getInvoice = new InvoiceData
                    {
                        InvoiceType = InvoiceType,
                        InvoiceNumber = query.Invoice_ID,
                        BuyerID = int.Parse(query.Receiver),
                        Payment = query.Purchase_type,
                        PaidPrice = query.PricePaid,
                        PaymentDays = query.Day_Limit,
                        AccountNumber = query.AccountNumber,
                        BankName = query.BankName,
                        SellDate = query.Date_Sold,
                        InvoiceDate = query.Date_Created.ToShortDateString(),
                        Currency = query.Currency,
                        Description = query.Description
                    };

                    var queryProducts = products.Invoice_Products.Where(x => x.Invoice_ID == query.Invoice_ID).ToList();

                    foreach (var data in queryProducts)
                    {
                        var getProduct = new ProductsSold_ViewModel
                        {
                            Name = data.Name,
                            Amount = int.Parse(data.Amount),
                            PieceNetto = decimal.Parse(data.Each_Netto),
                            VAT = data.VAT,
                            PieceBrutto = decimal.Parse(data.Each_Brutto),
                            Netto = decimal.Parse(data.Netto),
                            VATAmount = decimal.Parse(data.VAT_Price),
                            Brutto = decimal.Parse(data.Brutto),
                            GTU = Collections.GroupsData.FirstOrDefault(x => x.ID == Collections.WarehouseData.FirstOrDefault(z => z.Name == data.Name).GroupID).GTU
                        };
                        totalnetto += decimal.Parse(data.Netto);
                        totalbrutto += decimal.Parse(data.Brutto);

                        if (Page == "Archive")
                        {
                            Collections.ArchiveInvoices_Products.Add(getProduct);
                        }
                        else
                        {
                            Collections.ProductsSold_Correction.Add(getProduct);
                        }
                    }
                    getInvoice.TotalPrice = totalbrutto.ToString();
                }
            }
            return getInvoice;
        }
    }
}
