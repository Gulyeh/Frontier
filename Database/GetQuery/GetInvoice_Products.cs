using Frontier.Variables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                var query = await Invoice_Products.Where(x => x.Invoice_ID == data.Invoice_ID && x.Product_ID == data.Product_ID).FirstOrDefaultAsync();
                query.Invoice_ID = data.Invoice_ID;
                query.Product_ID = data.Product_ID;
                query.Name = data.Name;
                query.Amount = data.Amount;
                query.Each_Netto = data.Each_Netto;
                query.VAT = data.VAT;
                query.Each_Brutto = data.Each_Brutto;
                query.Netto = data.Netto;
                query.VAT_Price = data.VAT_Price;
                query.Brutto = data.Brutto;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool DeleteItem(string invoiceID, string productID)
        {
            try
            {
                Invoice_Products.Remove(Invoice_Products.Where(x => x.Invoice_ID == invoiceID && x.Product_ID == productID).FirstOrDefault());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
