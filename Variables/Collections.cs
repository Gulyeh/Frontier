using Frontier.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontier.Variables
{
    static class Collections
    {
        public static ObservableCollection<Warehouse_ViewModel> WarehouseData { get; set; } = new ObservableCollection<Warehouse_ViewModel>();
        public static ObservableCollection<CompanyData_ViewModel> CompanyData { get; set; } = new ObservableCollection<CompanyData_ViewModel>();
        public static ObservableCollection<Contactors_ViewModel> ContactorsData { get; set; } = new ObservableCollection<Contactors_ViewModel>();
        public static ObservableCollection<Groups_ViewModel> GroupsData { get; set; } = new ObservableCollection<Groups_ViewModel>();
    }
}
