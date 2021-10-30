using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontier.Database.TableClasses
{
    class Warehouse
    {
        [Key]
        public int idWarehouse { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
        public double Netto { get; set; }
        public double Brutto { get; set; }
        public int Margin { get; set; }
    }
}
