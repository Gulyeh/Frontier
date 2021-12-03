using Frontier.Variables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Frontier.Database.GetQuery
{
    class GetInvoice_Products : DbContext
    {
        public DbSet<TableClasses.Invoice_Products> Invoice_Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./Database/" + GlobalVariables.DatabaseName + ".sqlite");
        }

        public async Task<bool> AddProduct(TableClasses.Invoice_Products data)
        {
            try
            {
                await Invoice_Products.AddAsync(data);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> EditProduct(TableClasses.Invoice_Products data)
        {
            try
            {
                var query = await Invoice_Products.FirstOrDefaultAsync(x => x.id == data.id);
                query.Invoice_ID = data.Invoice_ID;
                query.Invoice = data.Invoice;
                query.Product_ID = data.Product_ID;
                query.Name = data.Name;
                query.Amount = data.Amount;
                query.Each_Netto = data.Each_Netto;
                query.VAT = data.VAT;
                query.Each_Brutto = data.Each_Brutto;
                query.Netto = data.Netto;
                query.VAT_Price = data.VAT_Price;
                query.Brutto = data.Brutto;
                query.GTU = data.GTU;
                query.BoughtNetto = data.BoughtNetto;
                query.BoughtVAT = data.BoughtVAT;
                query.BoughtBrutto = data.BoughtBrutto;
                query.GroupType = data.GroupType;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool DeleteItem(int idSold)
        {
            try
            {
                Invoice_Products.Remove(Invoice_Products.FirstOrDefault(x => x.id == idSold));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
