using Frontier.Methods;
using Frontier.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Frontier.Windows.CreateDB_Window
{
    public partial class CreateDB : Window
    {
        ObservableCollection<DatabaseList_ViewModel> DBList;
        public CreateDB(ObservableCollection<DatabaseList_ViewModel> DBList)
        {
            InitializeComponent();
            this.DBList = DBList;
        }

        private async void CreateDB_Clicked(object sender, RoutedEventArgs e)
        {
            bool validate = ValidateData();
            if (validate)
            {
                CreateDB_Button.IsEnabled = false;
                await CreateSQLite();
                CreateDB_Button.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Proszę wypełnić wymagane pola!");
            }
        }
        private void CheckSpace(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        private void NIP_CheckNumeric(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void ValidateNIP(object sender, TextChangedEventArgs e)
        {
            if (nip.Text.Length == 10)
            {
                bool validateNIP = Checkers.CheckNIP(nip.Text);
                if (!validateNIP)
                {
                    MessageBox.Show("NIP jest niepoprawny!");
                    nip.Text = String.Empty;
                }
            }
        }
        private void ValidateREGON(object sender, TextChangedEventArgs e)
        {
            if (regon.Text.Length == 9 || regon.Text.Length == 14)
            {
                int oldlength = regon.Text.Length;
                if (regon.Text.Length == oldlength)
                {
                    bool validateREGON = Checkers.CheckREGON(regon.Text);
                    if (!validateREGON)
                    {
                        MessageBox.Show("REGON jest niepoprawny!");
                        regon.Text = String.Empty;
                    }
                }
            }
        }
        private void CheckSyntax_PostCode(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex_Check.CheckPostCode(e.Text);
        }
        private bool ValidateData()
        {
            if (DBName.Text != string.Empty && DBLogin.Text != string.Empty && DBPassword.Password != string.Empty && CompName.Text != string.Empty && nip.Text != string.Empty && street.Text != string.Empty && postcode.Text != string.Empty && state.Text != string.Empty && country.Text != string.Empty)
            {
                return true;
            }
            return false;
        }
        private async Task CreateSQLite()
        {
            try
            {
                await Task.Run(() =>
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + "Database";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    this.Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        SQLiteConnection.CreateFile(path + "/" + DBName.Text + ".sqlite;PRAGMA journal_mode=WAL");
                        Database.ConnectDB.CreateConnection(path + "/" + DBName.Text + ".sqlite");
                        await CreateDBTables();
                        DBList.Add(new DatabaseList_ViewModel { ID = DBName.Text });
                        Database.ConnectDB.dbConnection.Close();
                        this.Close();
                    }));
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        private async Task CreateDBTables()
        {
            await Task.Run(() =>
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    List<string> query = new List<string>();
                    query.Add("CREATE TABLE `companydata` (`idcompanydata` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, `Name` varchar(45) NOT NULL, `NIP` varchar(45) NOT NULL, `Street` varchar(45) NOT NULL, `REGON` varchar(45) DEFAULT NULL, `PostCode` varchar(45) NOT NULL, `State` varchar(45) NOT NULL, `Country` varchar(45) NOT NULL)");
                    query.Add("CREATE TABLE `contactors` (`idContactors` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`Name` varchar(45) NOT NULL,`NIP` varchar(45) NOT NULL,`REGON` varchar(45) DEFAULT NULL,`Street` varchar(45) NOT NULL,`State` varchar(45) NOT NULL,`PostCode` varchar(45) NOT NULL,`Country` varchar(45) NOT NULL)");
                    query.Add("CREATE TABLE `groups` (`idgroups` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`Name` varchar(45) NOT NULL,`Description` varchar(100) DEFAULT NULL,`GTU` varchar(5) NOT NULL,`VAT` varchar(2) NOT NULL,`Type` INTEGER NOT NULL)");
                    query.Add("CREATE TABLE `invoice_bought` (`idinvoice_bought` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`Invoice_ID` varchar(45) NOT NULL,`Seller_ID` varchar(45) NOT NULL,`Date_Bought` varchar(45) NOT NULL,`Date_Created` DATETIME NOT NULL,`Purchase_type` varchar(45) NOT NULL,`Currency` varchar(45) NOT NULL)");
                    query.Add("CREATE TABLE `invoice_products` (`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, `Invoice` INTEGER NOT NULL, `Invoice_ID` varchar(100) NOT NULL, `Product_ID` varchar(10) NOT NULL, `Name` varchar(45) NOT NULL,`Amount` varchar(45) NOT NULL,`Each_Netto` varchar(45) NOT NULL,`VAT` varchar(3) NOT NULL,`Each_Brutto` varchar(45) NOT NULL, `Netto` varchar(45) NOT NULL,`VAT_Price` varchar(45) NOT NULL,`Brutto` varchar(45) NOT NULL, `GTU` varchar(3) NOT NULL)");
                    query.Add("CREATE TABLE `invoice_sold` (`idInvoice_Sold` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`Receiver` varchar(45) NOT NULL,`Invoice_ID` varchar(100) NOT NULL,`Invoice_Type` varchar(100) NOT NULL, `Date_Sold` varchar(45),`Date_Created` DATETIME NOT NULL,`Purchase_type` varchar(45) NOT NULL,`Day_Limit` varchar(45) DEFAULT NULL,`PricePaid` varchar(45),`Currency` varchar(45) NOT NULL,`Description` varchar(45) DEFAULT NULL,`AccountNumber` varchar(100) DEFAULT NULL,`BankName` varchar(45) DEFAULT NULL, `ExchangeRate` varchar(10) DEFAULT NULL)");
                    query.Add("CREATE TABLE `user` (`idUser` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`Login` varchar(200) NOT NULL,`Password` varchar(200) NOT NULL)");
                    query.Add("CREATE TABLE `warehouse` (`idwarehouse` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`group` int NOT NULL,`Name` varchar(45) NOT NULL,`Amount` varchar(10) NOT NULL,`Netto` decimal(15,2) NOT NULL,`Brutto` decimal(15,2) NOT NULL,`Margin` varchar(3) DEFAULT NULL, `VAT` varchar(3) NOT NULL)");
                    query.Add($"INSERT INTO `user` (Login, Password) VALUES ('{Convert.ToBase64String(Encoding.ASCII.GetBytes(DBLogin.Text))}', '{Convert.ToBase64String(Encoding.ASCII.GetBytes(DBPassword.Password))}')");
                    query.Add($"INSERT INTO `companydata` (Name, NIP, Street, REGON, PostCode, State, Country) VALUES ('{CompName.Text}', '{nip.Text}', '{street.Text}', '{regon.Text}', '{postcode.Text}', '{state.Text}', '{country.Text}')");
                    foreach (string data in query)
                    {
                        using (SQLiteCommand command = new SQLiteCommand(data, Database.ConnectDB.dbConnection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }));
            });
        }
    }
}
