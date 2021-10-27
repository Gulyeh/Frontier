using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontier.Database.TableClasses
{
    class Invoice_Bought
    {
        public string Invoice_ID { get; set; }
        public string Seller_ID { get; set; }
        public string Date_Bought { get; set; }
        public string Date_Created { get; set; }
        public string Purchase_type { get; set; }
        public string Currency { get; set; }
    }
}
