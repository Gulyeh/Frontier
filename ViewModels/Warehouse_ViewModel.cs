using System;
using System.ComponentModel;

namespace Frontier.ViewModels
{
    public class Warehouse_ViewModel : INotifyPropertyChanged
    {
        private int id { get; set; }
        public int ID
        {
            get { return id; }
            set
            {
                if (id == value) return;
                id = value;
                NotifyPropertyChanged("ID");
            }
        }

        private string name { get; set; }
        public string Name
        {
            get { return name; }
            set
            {
                if (name == value) return;
                name = value;
                NotifyPropertyChanged("Name");
            }
        }
        private string groupname { get; set; }
        public string GroupName
        {
            get { return groupname; }
            set
            {
                if (groupname == value) return;
                groupname = value;
                NotifyPropertyChanged("GroupName");
            }
        }
        private string grouptype { get; set; }
        public string GroupType
        {
            get { return grouptype; }
            set
            {
                if (grouptype == value) return;
                grouptype = value;
                NotifyPropertyChanged("GroupType");
            }
        }

        private int amount { get; set; }
        public int Amount
        {
            get { return amount; }
            set
            {
                if (amount == value) return;
                amount = value;
                NotifyPropertyChanged("Amount");
            }
        }

        private decimal brutto { get; set; }
        public decimal Brutto
        {
            get { return brutto; }
            set
            {
                if (brutto == value) return;
                brutto = Math.Round(decimal.Parse(value.ToString("F2")), 2);
                NotifyPropertyChanged("Brutto");
            }
        }

        private string vat { get; set; }
        public string VAT
        {
            get { return vat; }
            set
            {
                if (vat == value) return;
                vat = value;
                NotifyPropertyChanged("VAT");
            }
        }

        private decimal netto { get; set; }
        public decimal Netto
        {
            get { return netto; }
            set
            {
                if (netto == value) return;
                netto = Math.Round(decimal.Parse(value.ToString("F2")), 2);
                NotifyPropertyChanged("Netto");
            }
        }

        private int margin { get; set; }
        public int Margin
        {
            get { return margin; }
            set
            {
                if (margin == value) return;
                margin = value;
                NotifyPropertyChanged("Margin");
            }
        }
        public int GroupID { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
