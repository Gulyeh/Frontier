using System.ComponentModel.DataAnnotations;

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
        public int Type { get; set; }
    }
}
