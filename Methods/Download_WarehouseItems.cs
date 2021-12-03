using Frontier.Database.GetQuery;
using Frontier.Variables;
using Frontier.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Frontier.Methods
{
    internal class Download_WarehouseItems
    {
        public static Task Download()
        {
            using (GetWarehouse warehouse = new GetWarehouse())
            {
                Collections.WarehouseData.Clear();
                var query = warehouse.Warehouse;
                foreach (var data in query)
                {
                    Collections.WarehouseData.Add(new Warehouse_ViewModel
                    {
                        ID = data.idWarehouse,
                        GroupID = Int32.Parse(data.Group),
                        GroupName = Collections.GroupsData.Where(x => x.ID == Int32.Parse(data.Group)).FirstOrDefault().Name,
                        GroupType = Collections.GroupsData.Where(x => x.ID == Int32.Parse(data.Group)).FirstOrDefault().Type == 0 ? "Towar" : "Usługa",
                        Name = data.Name,
                        Amount = data.Amount,
                        Brutto = data.Brutto,
                        Netto = data.Netto,
                        VAT = data.VAT.ToString(),
                        Margin = data.Margin
                    });
                }
            }
            return Task.CompletedTask;
        }
    }
}
