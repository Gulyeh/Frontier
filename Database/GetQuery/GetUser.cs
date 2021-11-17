using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontier.Database.GetQuery
{
    class GetUser : DbContext
    {
        private string db_name { get; set; }
        public GetUser(string dbname)
        {
            db_name = dbname;
        }
        
        public DbSet<TableClasses.User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

                optionsBuilder.UseSqlite("Data Source=./Database/" + db_name + ".sqlite");
        }

        public async Task<bool> UpdateLogin(string newlogin)
        {
            try
            {
                var query = await User.Where(x => x.idUser == 1).FirstOrDefaultAsync();
                query.Login = Convert.ToBase64String(Encoding.ASCII.GetBytes(newlogin));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> UpdatePassword(string newpassword)
        {
            try
            {
                var query = await User.Where(x => x.idUser == 1).FirstOrDefaultAsync();
                query.Password = Convert.ToBase64String(Encoding.ASCII.GetBytes(newpassword));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
