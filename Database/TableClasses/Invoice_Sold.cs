using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontier.Database.TableClasses
{
    class Invoice_Sold
    {
        public string Receiver { get; set; }
        public string Invoice_ID { get; set; }
        public string Date_Sold { get; set; }
        public string Date_Created { get; set; }
        public string Purchase_type { get; set; }
        public string Day_Limit { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
    }
}
