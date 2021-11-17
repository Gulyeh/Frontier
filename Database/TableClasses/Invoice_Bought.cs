using System;
using System.ComponentModel.DataAnnotations;

namespace Frontier.Database.TableClasses
{
    class Invoice_Bought
    {
        [Key]
        public int idinvoice_bought { get; set; }
        public string Invoice_ID { get; set; }
        public string Seller_ID { get; set; }
        public string Date_Bought { get; set; }
        public DateTime Date_Created { get; set; }
        public string Purchase_type { get; set; }
        public string Currency { get; set; }
    }
}
