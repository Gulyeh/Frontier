using System.ComponentModel.DataAnnotations;

namespace Frontier.Database.TableClasses
{
    class Warehouse
    {
        [Key]
        public int idWarehouse { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
        public decimal Netto { get; set; }
        public decimal Brutto { get; set; }
        public int Margin { get; set; }
        public string VAT { get; set; }
    }
}
