using Frontier.Classes;
using Frontier.Database.GetQuery;
using Frontier.Variables;
using Frontier.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Frontier.Methods.Invoices
{
    class Download_Invoice
    {
        public static decimal totalnetto { get; set; }
        public static decimal totalbrutto { get; set; }

        public static async Task<InvoiceData> GetData(string InvoiceType, int ID)
        {
            InvoiceData getInvoice = null;
            totalnetto = 0;
            totalbrutto = 0;

            await Task.Run(() =>
            {
                getInvoice = SoldInvoice(InvoiceType, ID);
            });
            return getInvoice;
        }
        private static InvoiceData SoldInvoice(string InvoiceType, int ID)
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
                            GTU = data.GTU
                        };
                        totalnetto += decimal.Parse(data.Netto);
                        totalbrutto += decimal.Parse(data.Brutto);

                        Collections.ArchiveInvoices_Products.Add(getProduct);
                    }
                    getInvoice.TotalPrice = totalbrutto.ToString();
                }
            }
            return getInvoice;
        }
    }
}
