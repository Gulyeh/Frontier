using Frontier.Variables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontier.Database.GetQuery
{
    class GetInvoices : DbContext
    {
        public DbSet<TableClasses.Invoice_Bought> Invoice_Bought { get; set; }
        public DbSet<TableClasses.Invoice_Sold> Invoice_Sold { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./Database/" + GlobalVariables.DatabaseName + ".sqlite");
        }

        public async Task<bool> Edit_Bought(TableClasses.Invoice_Bought data)
        {
            try
            {
                var query = await Invoice_Bought.Where(x => x.Invoice_ID == data.Invoice_ID).FirstOrDefaultAsync();
                query.Invoice_ID = data.Invoice_ID;
                query.Date_Created = data.Date_Created;
                query.Purchase_type = data.Purchase_type;
                query.Currency = data.Currency;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> Edit_Sold(TableClasses.Invoice_Sold data)
        {
            try
            {
                var query = await Invoice_Sold.Where(x => x.idInvoice_Sold == data.idInvoice_Sold).FirstOrDefaultAsync();
                query.Receiver = data.Receiver;
                query.Invoice_ID = data.Invoice_ID;
                query.Date_Sold = data.Date_Sold;
                query.Date_Created = data.Date_Created;
                query.Purchase_type = data.Purchase_type;
                query.Day_Limit = data.Day_Limit;
                query.Currency = data.Currency;
                query.Description = data.Description;
                query.AccountNumber = data.AccountNumber;
                query.BankName = data.BankName;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
