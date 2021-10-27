using Frontier.Database.GetQuery;
using Frontier.Variables;
using Frontier.ViewModels;
using Frontier.Windows.CreateDB_Window;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Frontier.Windows.Auth_Window
{
    public partial class Auth : Page, INotifyPropertyChanged
    {
        private readonly Login_ViewModel LoginModel;
        public ObservableCollection<DatabaseList_ViewModel> DatabaseList { get; set; } = new ObservableCollection<DatabaseList_ViewModel>();
        private bool selectedDB { get; set; }
        public bool SelectedDB
        {
            get { return selectedDB; }
            set
            {
                if (selectedDB == value) return;
                selectedDB = value;
                NotifyPropertyChanged("SelectedDB");
            }
        }

        public Auth(Login_ViewModel LoginData)
        {
            InitializeComponent();
            GetDBList();
            LoginModel = LoginData;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoginModel.isLogged = false;
            Check_Selection(Database_List.SelectedIndex);
        }
        private void Login_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Login.Text != string.Empty && Password.Password != string.Empty)
                {
                    SelectedDB = false;
                    GetUser User = new GetUser(DatabaseList[Database_List.SelectedIndex].ID);
                    var query = User.User.FirstOrDefault(x => x.idUser == 1);
                    if (query.Login == Convert.ToBase64String(Encoding.ASCII.GetBytes(Login.Text)) && query.Password == Convert.ToBase64String(Encoding.ASCII.GetBytes(Password.Password)))
                    {
                        GlobalVariables.DatabaseName = DatabaseList[Database_List.SelectedIndex].ID;
                        ((MainWindow)Application.Current.MainWindow).LoadPages();
                        LoginModel.isLogged = true;
                        ((MainWindow)Application.Current.MainWindow).Menu_List.SelectedIndex = 0;
                    }
                    else
                    {
                        MessageBox.Show("Wrong login or password");
                    }
                    SelectedDB = true;
                }
            } catch (Exception)
            {
                MessageBox.Show("Wrong login or password");
                SelectedDB = true;
            }
        }
        private void NewDB_Clicked(object sender, RoutedEventArgs e)
        {
            CreateDB dbwindow = new CreateDB(DatabaseList);
            dbwindow.Owner = Application.Current.MainWindow;
            dbwindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dbwindow.ShowDialog();
        }
        private void Select_Database(object sender, RoutedEventArgs e)
        {
            Check_Selection(Database_List.SelectedIndex);
        }
        private void Check_Selection(int index)
        {
            if (index > -1)
            {
                SelectedDB = true;
            }
            else
            {
                SelectedDB = false;
            }
        }
        private void GetDBList()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Database";
            if (Directory.Exists(path))
            {
                DirectoryInfo d = new DirectoryInfo(path);
                foreach(var file in d.GetFiles("*.sqlite"))
                {
                    DatabaseList.Add(new DatabaseList_ViewModel { ID = file.Name.Replace(".sqlite", "") });
                }
            }
        }
    }
}
