using Frontier.Variables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public bool UpdateCompany(TableClasses.CompanyData data)
        {
            try
            {
                var query = CompanyData.Where(x => x.idcompanydata == 1).FirstOrDefault();
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
    }
}
