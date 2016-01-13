using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using System.Diagnostics;

namespace SecureboxWPF
{
    
    class DatabaseManagement
    {
        //static string dbname = "locker.sbox";
        static string dbname = Path.Combine(
                                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                            "Securebox","locker.sbox");
        static string connstring = "Data Source=" + dbname + ";Version=3;";

        static string authDBname = Path.Combine(
                                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                            "Securebox", "auth.sbox");
        static string authConnString = "Data Source=" + dbname + ";Version=3;";
        public static void CreateDatabase()
        {

            if (!DatabaseManagement.CheckDatabaseExists())
            {
                DatabaseManagement.CreateSecureboxDirectory(@"Securebox");
                SQLiteConnection.CreateFile(dbname);
                SQLiteConnection.CreateFile(authDBname);
                DatabaseManagement.CreateKeyTable();
            }
        }

        public static string GetDBFullPath()
        {
            return DatabaseManagement._GetDBFullPath();
        }
        private static string _GetDBFullPath()
        {
            return dbname;
        }
        public static void CreateSecureboxDirectory(string dirName)
        {
            string directoryPath = Path.Combine(
                                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                            dirName);
            if( !Directory.Exists(directoryPath) )
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private static bool CheckDatabaseExists()
        {
            return File.Exists(dbname) && File.Exists(authDBname);
        }

        private void CreateLoginTable()
        {

            SQLiteConnection m_dbConnection = new SQLiteConnection(authConnString);
            m_dbConnection.Open();

            string sql = "create table authtokens ( id INTEGER PRIMARY KEY, authtoken blob )";

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();

            m_dbConnection.Close();
        }

        private void StoreAuthenticationToken(byte[] authToken)
        {

            SQLiteConnection m_dbConnection = new SQLiteConnection(connstring);
            m_dbConnection.Open();

            string sql = "insert into loginkeys (authtoken) values (@authtoken)";

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.Parameters.AddWithValue("@authtoken", authToken);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            m_dbConnection.Close();
        }


        public static SecureboxKey GetSecureboxKey(string GUID)
        {
            SecureboxKey el = new SecureboxKey();

            byte[] IV = new byte[16];
            byte[] KEY = new byte[16];
            SQLiteConnection m_dbConnection = new SQLiteConnection(connstring);
            m_dbConnection.Open();

            string sql = "select IV, KEY from lookup WHERE GUID=@guid";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.Parameters.AddWithValue("@guid", GUID);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                IV = (byte[])reader["IV"];
                KEY = (byte[])reader["KEY"];
                
            }
            reader.Close();
            m_dbConnection.Close();
            el.IV = IV;
            el.KEY = KEY;
            el.GUID = GUID;
            return el;
        }
        public static void AppendKeyFile(string importedDBFileLocation)
        {
            DatabaseManagement._AppendKeyFile(importedDBFileLocation);
        }
        private static void _AppendKeyFile(string importedDBFileLocation)
        {
            string importedConnString = "Data Source=" + importedDBFileLocation + ";Version=3;";
            SQLiteConnection importedDBConnection = new SQLiteConnection(importedConnString);
            importedDBConnection.Open();
            string sql = "select GUID, IV, KEY from lookup";
            SQLiteCommand command = new SQLiteCommand(sql, importedDBConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            string GUID;
            byte[] IV = new byte[16];
            byte[] KEY = new byte[16];
            SQLiteConnection m_dbConnection = new SQLiteConnection(connstring);
            m_dbConnection.Open();
            while (reader.Read())
            {
                GUID = reader["GUID"].ToString();
                IV = (byte[])reader["IV"];
                KEY = (byte[])reader["KEY"];
                DatabaseManagement._InsertKeyNoOpen(GUID, IV, KEY, m_dbConnection);
            }
            m_dbConnection.Close();

        }
        private static void CreateKeyTable()
        {
            SQLiteConnection m_dbConnection = new SQLiteConnection(connstring);
            m_dbConnection.Open();

            string sql = "create table lookup ( GUID TEXT, IV BLOB, KEY BLOB )";

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();

            m_dbConnection.Close();
        }
        public static void InsertKey(string GUID, byte[] IV, byte[] KEY)
        {
            DatabaseManagement._InsertKey(GUID, IV, KEY);
        }
        private static void _InsertKeyNoOpen(string GUID, byte[] IV, byte[] KEY, SQLiteConnection m_dbConnection)
        {
            string sql = "insert into lookup (GUID, IV, KEY) values (@guid, @iv, @key)";

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.Parameters.AddWithValue("@guid", GUID);
            command.Parameters.AddWithValue("@iv", IV);
            command.Parameters.AddWithValue("@key", KEY);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        private static void _InsertKey(string GUID, byte[] IV, byte[] KEY)
        {

            SQLiteConnection m_dbConnection = new SQLiteConnection(connstring);
            m_dbConnection.Open();

            string sql = "insert into lookup (GUID, IV, KEY) values (@guid, @iv, @key)";

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.Parameters.AddWithValue("@guid", GUID);
            command.Parameters.AddWithValue("@iv", IV);
            command.Parameters.AddWithValue("@key", KEY);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            m_dbConnection.Close();
        }

    }
}
