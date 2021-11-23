using System.ComponentModel.DataAnnotations;

namespace Frontier.Database.TableClasses
{
    class Invoice_Products
    {
        [Key]
        public int id { get; set; }
        public string Invoice_ID { get; set; }
        public int Invoice { get; set; }
        public string Product_ID { get; set; }
        public string Name { get; set; }
        public string Amount { get; set; }
        public string Each_Netto { get; set; }
        public string VAT { get; set; }
        public string Each_Brutto { get; set; }
        public string Netto { get; set; }
        public string VAT_Price { get; set; }
        public string Brutto { get; set; }
        public string GTU { get; set; }
    }
}
