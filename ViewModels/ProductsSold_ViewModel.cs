using System;
using System.ComponentModel;

namespace Frontier.ViewModels
{
    public class ProductsSold_ViewModel : INotifyPropertyChanged
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

        private decimal piecenetto { get; set; }
        public decimal PieceNetto
        {
            get { return piecenetto; }
            set
            {
                if (piecenetto == value) return;
                piecenetto = Math.Round(decimal.Parse(value.ToString("F2")), 2);
                NotifyPropertyChanged("PieceNetto");
            }
        }

        private decimal piecebrutto { get; set; }
        public decimal PieceBrutto
        {
            get { return piecebrutto; }
            set
            {
                if (piecebrutto == value) return;
                piecebrutto = Math.Round(decimal.Parse(value.ToString("F2")), 2);
                NotifyPropertyChanged("PieceBrutto");
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

        private decimal vatamount { get; set; }
        public decimal VATAmount
        {
            get { return vatamount; }
            set
            {
                if (vatamount == value) return;
                vatamount = Math.Round(decimal.Parse(value.ToString("F2")), 2);
                NotifyPropertyChanged("VATAmount");
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

        public string GroupType { get; set; }
        public int Margin { get; set; }
        public string GTU { get; set; }
        public int ID_SoldProduct { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
