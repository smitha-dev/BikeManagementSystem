using System.Data.SQLite;

namespace FindlayBikeShop
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Data Source=BikeDatabase.db;Version=3;";

        public static void Initialize()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Bikes (
                    BikeID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Brand TEXT,
                    Size TEXT,
                    MinHeight REAL,
                    MaxHeight REAL,
                    Color TEXT,
                    Status TEXT,
                    DateAdded TEXT,
                    LastUpdated TEXT
                );
            ";

                using (var cmd = new SQLiteCommand(createTableQuery, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
