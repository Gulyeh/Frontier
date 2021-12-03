using Frontier.Variables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Frontier.Database.GetQuery
{
    class GetCompanydata : DbContext
    {
        public DbSet<TableClasses.CompanyData> CompanyData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./Database/" + GlobalVariables.DatabaseName + ".sqlite");
        }

        public async Task<bool> UpdateCompany(TableClasses.CompanyData data)
        {
            try
            {
                var query = await CompanyData.FirstOrDefaultAsync();
                query.Name = data.Name;
                query.NIP = data.NIP;
                query.PostCode = data.PostCode;
                query.REGON = data.REGON;
                query.State = data.State;
                query.Street = data.Street;
                query.Country = data.Country;
                query.BDO = data.BDO;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
