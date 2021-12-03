using System.ComponentModel.DataAnnotations;

namespace Frontier.Database.TableClasses
{
    class CompanyData
    {
        [Key]
        public int idcompanydata { get; set; }
        public string Name { get; set; }
        public string NIP { get; set; }
        public string REGON { get; set; }
        public string Street { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
        public string BDO { get; set; }
    }
}
