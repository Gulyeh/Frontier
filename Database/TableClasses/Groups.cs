using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontier.Database.TableClasses
{
    class Groups
    {
        [Key]
        public int idgroups { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string GTU { get; set; }
        public string VAT { get; set; }
    }
}
