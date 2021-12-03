using System;
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
        public int Margin { get; set; }
        public string VAT { get; set; }
    }
}
