using Frontier.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Frontier.Variables
{
    static class Collections
    {
        public static ObservableCollection<Warehouse_ViewModel> WarehouseData = new ObservableCollection<Warehouse_ViewModel>();
        public static Dictionary<string, string> CompanyData = new Dictionary<string, string>();
        public static ObservableCollection<Contactors_ViewModel> ContactorsData = new ObservableCollection<Contactors_ViewModel>();
        public static ObservableCollection<Groups_ViewModel> GroupsData = new ObservableCollection<Groups_ViewModel>();
        public static ObservableCollection<ProductsSold_ViewModel> ProductsSold = new ObservableCollection<ProductsSold_ViewModel>();
        public static ObservableCollection<ProductsSold_ViewModel> Products_Correction = new ObservableCollection<ProductsSold_ViewModel>();
        public static ObservableCollection<ProductsSold_ViewModel> ProductsBought = new ObservableCollection<ProductsSold_ViewModel>();
        public static ObservableCollection<ArchiveGrid_ViewModel> Archive_Invoices = new ObservableCollection<ArchiveGrid_ViewModel>();
        public static List<ProductsSold_ViewModel> ArchiveInvoices_Products = new List<ProductsSold_ViewModel>();
        public static void ResetCollections()
        {
            WarehouseData.Clear();
            CompanyData.Clear();
            ContactorsData.Clear();
            GroupsData.Clear();
            ProductsSold.Clear();
            ProductsBought.Clear();
            Archive_Invoices.Clear();
            ArchiveInvoices_Products.Clear();
            Products_Correction.Clear();
        }
    }
}
