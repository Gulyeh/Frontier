using Frontier.Variables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontier.Database.GetQuery
{
    class GetWarehouse : DbContext
    {
        public DbSet<TableClasses.Warehouse> Warehouse { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./Database/" + GlobalVariables.DatabaseName + ".sqlite");
        }

        public async Task<bool> AddItem(TableClasses.Warehouse data)
        {
            try
            {
                await Warehouse.AddAsync(data);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> EditItem(TableClasses.Warehouse data)
        {
            try
            {
                var query = await Warehouse.Where(x => x.idWarehouse == data.idWarehouse).FirstOrDefaultAsync();
                query.Name = data.Name;
                query.Group = data.Group;
                query.Amount = data.Amount;
                query.Netto = data.Netto;
                query.Brutto = data.Brutto;
                query.Margin = data.Margin;
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
                Warehouse.Remove(Warehouse.Where(x => x.idWarehouse == ID).FirstOrDefault());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
