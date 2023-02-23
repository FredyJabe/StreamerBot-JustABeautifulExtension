using System;
using System.IO;
using System.Data.SQLite;

namespace JabeDLL {
    public static class Data {
        private static string connectionString = "";
        private static SQLiteConnection connection;

        public static void Initialize() {
            connectionString = @"Data Source=test.db;Version=3;New=True;Compress=True;";
            connection = new SQLiteConnection(connectionString);
        }

        public static object[] Read(string request) {
            // TODO sqlite read data
            object[] retVal = {};
            connection.Open();

            

            connection.Close();
            return retVal;
        }

        public static void Write(string request) {
            // TODO sqlite write data
            connection.Open();



            connectionString.Close();
        }
    } 
}