﻿using System.ComponentModel;

namespace Frontier.ViewModels
{
    public class Contactors_ViewModel : INotifyPropertyChanged
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

        private string nip { get; set; }
        public string NIP
        {
            get { return nip; }
            set
            {
                if (nip == value) return;
                nip = value;
                NotifyPropertyChanged("NIP");
            }
        }

        private string address { get; set; }
        public string Address
        {
            get { return address; }
            set
            {
                if (address == value) return;
                address = value;
                NotifyPropertyChanged("Address");
            }
        }

        private string regon { get; set; }
        public string Regon
        {
            get { return regon; }
            set
            {
                if (regon == value) return;
                regon = value;
                NotifyPropertyChanged("Regon");
            }
        }

        private string state { get; set; }
        public string State
        {
            get { return state; }
            set
            {
                if (state == value) return;
                state = value;
                NotifyPropertyChanged("State");
            }
        }

        private string postcode { get; set; }
        public string PostCode
        {
            get { return postcode; }
            set
            {
                if (postcode == value) return;
                postcode = value;
                NotifyPropertyChanged("PostCode");
            }
        }

        private string country { get; set; }
        public string Country
        {
            get { return country; }
            set
            {
                if (country == value) return;
                country = value;
                NotifyPropertyChanged("Address");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
