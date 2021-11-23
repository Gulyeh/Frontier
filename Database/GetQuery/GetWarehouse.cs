using Frontier.Variables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Linq;
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
        public async Task<bool> SoldWarehouse_Item(TableClasses.Warehouse data)
        {
            try
            {
                var amount = await Warehouse.Where(x => x.Name == data.Name).CountAsync();
                var query = await Warehouse.Where(x => x.idWarehouse == data.idWarehouse).FirstOrDefaultAsync();

                if(query.Amount == data.Amount)
                {
                    if (amount > 1)
                    {
                        DeleteItem(data.idWarehouse);
                        Collections.WarehouseData.Remove(Collections.WarehouseData.Where(x => x.ID == data.idWarehouse).FirstOrDefault());
                    }
                    else if(amount == 1)
                    {
                        var item = Collections.WarehouseData.Where(x => x.ID == data.idWarehouse).FirstOrDefault();
                        query.Amount = 0;
                        query.Netto = 0;
                        query.Brutto = 0;
                        query.VAT = "0";
                        item.Amount = 0;
                        item.Netto = 0;
                        item.Brutto = 0;
                        item.VAT = "0%";
                    }
                }
                else if(query.Amount > data.Amount)
                {
                    query.Amount -= data.Amount;
                }
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
