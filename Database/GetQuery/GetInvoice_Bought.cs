using Frontier.Variables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Frontier.Database.GetQuery
{
    class GetInvoice_Bought : DbContext
    {
        public DbSet<TableClasses.Invoice_Bought> Invoice_Bought { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./Database/" + GlobalVariables.DatabaseName + ".sqlite");
        }

        public async Task<bool> AddInvoice(TableClasses.Invoice_Bought data)
        {
            try
            {
                await Invoice_Bought.AddAsync(data);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> EditProduct(TableClasses.Invoice_Bought data)
        {
            try
            {
                var query = await Invoice_Bought.Where(x => x.idinvoice_bought == data.idinvoice_bought).FirstOrDefaultAsync();
                query.Invoice_ID = data.Invoice_ID;
                query.Seller_ID = data.Seller_ID;
                query.Purchase_type = data.Purchase_type;
                query.Date_Created = data.Date_Created;
                query.Date_Bought = data.Date_Bought;
                query.Currency = data.Currency;
                query.ExchangeRate = data.ExchangeRate;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
