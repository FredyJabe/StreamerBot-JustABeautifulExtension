using System;
using System.IO;
using System.Diagnostics;
using System.Data.SQLite;

namespace JabeDll {
    public static class Data {
        private static string connectionString = "";
        private static SQLiteConnection connection;

        public static void Initialize() {
            connectionString = @"Data Source=test.db;Version=3;New=True;Compress=True;";
            connection = new SQLiteConnection(connectionString);
        }

        public static object[] Read(string request) {
            object[] retVal = {};
            connection.Open();

            // TODO sqlite read data

            connection.Close();
            return retVal;
        }

        public static void Write(string request) {
            connection.Open();

            // TODO sqlite write data

            connection.Close();
        }

        // Logs a line
        public static void Log(string line) {
            File.AppendAllText(Settings.PathLog, DateTime.Now.ToString("hh:mm tt") + " | " + line + "\n");
        }
    } 
}