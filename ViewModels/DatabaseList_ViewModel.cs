using System.ComponentModel;

namespace Frontier.ViewModels
{
    public class DatabaseList_ViewModel : INotifyPropertyChanged
    {
        private string id { get; set; }
        public string ID
        {
            get { return id; }
            set
            {
                if (id == value) return;
                id = value;
                NotifyPropertyChanged("ID");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
