using System.ComponentModel.DataAnnotations;

namespace Frontier.Database.TableClasses
{
    class User
    {
        [Key]
        public int idUser { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
