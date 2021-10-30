using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontier.ViewModels
{
    class Groups_ViewModel : INotifyPropertyChanged
    {
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

        public int ID { get; set; }
        public string Description { get; set; }
        public string VAT { get; set; }
        public string GTU { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
