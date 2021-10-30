using Frontier.Variables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontier.Database.GetQuery
{
    class GetGroups : DbContext
    {
        public DbSet<TableClasses.Groups> Groups { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./Database/" + GlobalVariables.DatabaseName + ".sqlite");
        }

        public async Task<bool> AddGroups(TableClasses.Groups data)
        {
            try
            {
                await Groups.AddAsync(data);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteGroup(int ID)
        {
            try
            {
                Groups.Remove(Groups.Where(x => x.idgroups == ID).FirstOrDefault());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> EditGroup(TableClasses.Groups data)
        {
            try
            {
                var query = await Groups.Where(x => x.idgroups == data.idgroups).FirstOrDefaultAsync();
                query.Name = data.Name;
                query.Description = data.Description;
                query.VAT = data.VAT;
                query.GTU = data.GTU;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
