using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
