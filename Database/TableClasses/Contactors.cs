using System.ComponentModel.DataAnnotations;

namespace Frontier.Database.TableClasses
{
    class Contactors
    {
        [Key]
        public int idContactors { get; set; }
        public string Name { get; set; }
        public string NIP { get; set; }
        public string REGON { get; set; }
        public string Street { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
    }
}
