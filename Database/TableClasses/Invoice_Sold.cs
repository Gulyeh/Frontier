using System;
using System.ComponentModel.DataAnnotations;

namespace Frontier.Database.TableClasses
{
    class Invoice_Sold
    {
        [Key]
        public int idInvoice_Sold { get; set; }
        public string Receiver { get; set; }
        public string Invoice_ID { get; set; }
        public string Invoice_Type { get; set; }
        public string Date_Sold { get; set; }
        public DateTime Date_Created { get; set; }
        public string Purchase_type { get; set; }
        public string PricePaid { get; set; }
        public string Day_Limit { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string ExchangeRate { get; set; }
    }
}
