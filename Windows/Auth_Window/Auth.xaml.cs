using Frontier.Database.GetQuery;
using Frontier.Methods.XML;
using Frontier.Variables;
using Frontier.ViewModels;
using Frontier.Windows.CreateDB_Window;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoginModel.isLogged = false;
            await Credentials.CheckXML();
            Dictionary<string, string> data = Credentials.ReadXML("");
            if (data != null)
            {
                Database_List.SelectedIndex = DatabaseList.IndexOf(DatabaseList.FirstOrDefault(x => x.ID.ToString() == data["Database"]));
            }
        }
        private async void Login_Clicked(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                await Dispatcher.BeginInvoke(new Action(async () =>
                {
                    try
                    {
                        if (Login.Text != string.Empty && Password.Password != string.Empty)
                        {
                            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                            await Task.Delay(500);
                            using (GetUser User = new GetUser(DatabaseList[Database_List.SelectedIndex].ID))
                            {
                                var query = User.User.FirstOrDefault();
                                if (query.Login == Convert.ToBase64String(Encoding.ASCII.GetBytes(Login.Text)) && query.Password == Convert.ToBase64String(Encoding.ASCII.GetBytes(Password.Password)))
                                {
                                    GlobalVariables.DatabaseName = DatabaseList[Database_List.SelectedIndex].ID;
                                    await ((MainWindow)Application.Current.MainWindow).LoadPages();
                                    LoginModel.isLogged = true;
                                    Credentials.SaveXML(new Dictionary<string, string>
                                    {
                                        { "Database", Database_List.Text },
                                        { "Login",  Convert.ToBase64String(Encoding.ASCII.GetBytes(Login.Text)) },
                                        { "Password", Convert.ToBase64String(Encoding.ASCII.GetBytes(Password.Password)) },
                                        { "Logged", KeepLogged.IsChecked.ToString() }
                                    });
                                    ((MainWindow)Application.Current.MainWindow).Menu_List.SelectedIndex = 0;
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
                            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            MessageBox.Show("Proszę podać wymagane dane");
                        }
                    }
                    catch (Exception)
                    {
                        ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                        MessageBox.Show("Zły login lub hasło");
                    }
                }));
            });
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
            Password.Password = string.Empty;
            Login.Text = string.Empty;
            KeepLogged.IsChecked = false;

            Dictionary<string, string> data = Credentials.ReadXML(DatabaseList[Database_List.SelectedIndex].ID);
            if (data != null)
            {
                Password.Password = data["Password"];
                Login.Text = data["Login"];
                KeepLogged.IsChecked = true;
            }
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
                foreach (var file in d.GetFiles("*.sqlite"))
                {
                    if(DatabaseList.FirstOrDefault(x => x.ID == file.Name.Replace(".sqlite", "")) == null)
                    {
                        DatabaseList.Add(new DatabaseList_ViewModel { ID = file.Name.Replace(".sqlite", "") });
                    }
                }
            }
        }
        private async void ImportDB_Clicked(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        System.Windows.Forms.OpenFileDialog importOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
                        importOpenFileDialog.Title = "Select File";
                        importOpenFileDialog.Filter = "SQLite(*.sqlite)|*.sqlite";

                        if (System.Windows.Forms.DialogResult.OK == importOpenFileDialog.ShowDialog())
                        {
                            ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Visible;
                            if (Path.GetExtension(importOpenFileDialog.FileName) != ".sqlite") { throw new Exception(); }
                            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Database/" + importOpenFileDialog.SafeFileName))
                            {
                                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                                MessageBox.Show("Baza danych o takiej nazwie już istnieje");
                            }
                            else
                            {
                                File.Copy(importOpenFileDialog.FileName, AppDomain.CurrentDomain.BaseDirectory + "/Database/" + importOpenFileDialog.SafeFileName, true);
                                GetDBList();
                                ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                                MessageBox.Show("Baza danych została zaimportowana pomyślnie");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        ((MainWindow)Application.Current.MainWindow).Loading.Visibility = Visibility.Hidden;
                        MessageBox.Show("Wystąpił błąd podczas importowania bazy danych");
                    }
                }));
            });
        }
    }
}
