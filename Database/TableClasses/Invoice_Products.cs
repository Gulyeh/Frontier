using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontier.Database.TableClasses
{
    class Invoice_Products
    {
        public string Name { get; set; }
        public string Amount { get; set; }
        public string Price_each { get; set; }
        public string VAT { get; set; }
        public string Netto { get; set; }
        public string VAT_Price { get; set; }
        public string Brutto { get; set; }
    }
}
