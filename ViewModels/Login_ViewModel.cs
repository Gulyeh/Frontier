using System.ComponentModel;

namespace Frontier.ViewModels
{
    public class Login_ViewModel : INotifyPropertyChanged
    {

        private bool _isLogged { get; set; }
        public bool isLogged
        {
            get { return _isLogged; }
            set
            {
                if (_isLogged == value) return;
                _isLogged = value;
                NotifyPropertyChanged("isLogged");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
