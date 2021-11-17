using System.Data.SQLite;

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
