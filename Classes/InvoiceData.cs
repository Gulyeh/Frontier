namespace Frontier.Classes
{
    internal class InvoiceData
    {
        public string InvoiceType { get; set; }
        public string InvoiceNumber { get; set; }
        public int BuyerID { get; set; }
        public string Payment { get; set; }
        public string SellDate { get; set; }
        public string InvoiceDate { get; set; }
        public string Description { get; set; }
        public string PaymentDays { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string TotalPrice { get; set; }
        public string PaidPrice { get; set; }
        public string Currency { get; set; }
        public string Signature { get;set; }
    }
}
