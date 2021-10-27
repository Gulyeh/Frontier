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
        private int group { get; set; }
        public int Group
        {
            get { return group; }
            set
            {
                if (group == value) return;
                group = value;
                NotifyPropertyChanged("Group");
            }
        }

        private int count { get; set; }
        public int Count
        {
            get { return count; }
            set
            {
                if (count == value) return;
                count = value;
                NotifyPropertyChanged("Count");
            }
        }

        private double brutto { get; set; }
        public double Brutto
        {
            get { return brutto; }
            set
            {
                if (brutto == value) return;
                brutto = value;
                NotifyPropertyChanged("Brutto");
            }
        }

        private int vat { get; set; }
        public int VAT
        {
            get { return vat; }
            set
            {
                if (vat == value) return;
                vat = value;
                NotifyPropertyChanged("VAT");
            }
        }

        private double netto { get; set; }
        public double Netto
        {
            get { return netto; }
            set
            {
                if (netto == value) return;
                netto = value;
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
