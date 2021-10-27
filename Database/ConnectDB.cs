using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontier.Database
{
    class ConnectDB
    {
        public static SQLiteConnection dbConnection;
        public static void CreateConnection(string dbpath)
        {
            dbConnection = new SQLiteConnection("Data Source=" + dbpath + ";Version=3;");
            dbConnection.Open();         
        }
    }
}
