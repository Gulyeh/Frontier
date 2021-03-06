using System;

namespace Frontier.ViewModels
{
    class ArchiveGrid_ViewModel
    {
        public int ID { get; set; }
        public string InvoiceType { get; set; }
        public string InvoiceID { get; set; }
        public string Contactor { get; set; }
        public string ContactorNIP { get; set; }
        public string Netto { get; set; }
        public string VATAmount { get; set; }
        public string Brutto { get; set; }
        public string Currency { get; set; }
        public DateTime Created_Date { get; set; }
    }
}
