using Frontier.Variables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontier.Database.GetQuery
{
    class GetContactors : DbContext
    {
        public DbSet<TableClasses.Contactors> Contactors { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./Database/" + GlobalVariables.DatabaseName + ".sqlite");
        }
        public async Task<bool> AddContractor(TableClasses.Contactors data)
        {
            try
            {
                await Contactors.AddAsync(data);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> EditContactor(TableClasses.Contactors data)
        {
            try
            {
                var query = await Contactors.Where(x => x.idContactors == data.idContactors).FirstOrDefaultAsync();
                query.Name = data.Name;
                query.NIP = data.NIP;
                query.PostCode = data.PostCode;
                query.REGON = data.REGON;
                query.State = data.State;
                query.Street = data.Street;
                query.Country = data.Country;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool DeleteContactor(int ID)
        {
            try
            {
                Contactors.Remove(Contactors.Where(x => x.idContactors == ID).FirstOrDefault());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
