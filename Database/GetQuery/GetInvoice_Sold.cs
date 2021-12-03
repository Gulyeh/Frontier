using Frontier.Variables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Frontier.Database.GetQuery
{
    class GetInvoice_Sold : DbContext
    {
        public DbSet<TableClasses.Invoice_Sold> Invoice_Sold { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./Database/" + GlobalVariables.DatabaseName + ".sqlite");
        }

        public async Task<bool> AddInvoice(TableClasses.Invoice_Sold data)
        {
            try
            {
                await Invoice_Sold.AddAsync(data);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> EditInvoice(TableClasses.Invoice_Sold data)
        {
            try
            {
                var query = await Invoice_Sold.Where(x => x.idInvoice_Sold == data.idInvoice_Sold).FirstOrDefaultAsync();
                query.Receiver = data.Receiver;
                query.Invoice_ID = data.Invoice_ID;
                query.Invoice_Type = data.Invoice_Type;
                query.Date_Sold = data.Date_Sold;
                query.Date_Created = data.Date_Created;
                query.Purchase_type = data.Purchase_type;
                query.Day_Limit = data.Day_Limit;
                query.Currency = data.Currency;
                query.Description = data.Description;
                query.AccountNumber = data.AccountNumber;
                query.BankName = data.BankName;
                query.PricePaid = data.PricePaid;
                query.ExchangeRate = data.ExchangeRate;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool DeleteItem(int ID)
        {
            try
            {
                Invoice_Sold.Remove(Invoice_Sold.Where(x => x.idInvoice_Sold == ID).FirstOrDefault());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
