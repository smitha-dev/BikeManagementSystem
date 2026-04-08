using Microsoft.Data.Sqlite;

namespace FindlayBikeShop
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Data Source=BikeDatabase.db";

        public static void Initialize()
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                // Bikes
                Execute(conn, @"
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
                ");

                // Maintenance
                Execute(conn, @"
                    CREATE TABLE IF NOT EXISTS Maintenance (
                        MaintenanceID INTEGER PRIMARY KEY AUTOINCREMENT,
                        BikeID INTEGER,
                        DateFlagged TEXT,
                        DateFixed TEXT,
                        Notes TEXT,
                        Cost REAL,
                        PartNeeded TEXT,
                        FOREIGN KEY (BikeID) REFERENCES Bikes(BikeID)
                    );
                ");

                // Photos
                Execute(conn, @"
                    CREATE TABLE IF NOT EXISTS Photos (
                        PhotoID INTEGER PRIMARY KEY AUTOINCREMENT,
                        BikeID INTEGER,
                        MaintenanceID INTEGER,
                        FilePath TEXT,
                        PhotoType TEXT,
                        FOREIGN KEY (BikeID) REFERENCES Bikes(BikeID),
                        FOREIGN KEY (MaintenanceID) REFERENCES Maintenance(MaintenanceID)
                    );
                ");

                // Rentals
                Execute(conn, @"
                    CREATE TABLE IF NOT EXISTS Rentals (
                        RentalID INTEGER PRIMARY KEY AUTOINCREMENT,
                        BikeID INTEGER,
                        StudentID TEXT,
                        SemesterRented TEXT,
                        Year INTEGER,
                        CheckoutDate TEXT,
                        DueDate TEXT,
                        ReturnDate TEXT,
                        CheckinDate1 TEXT,
                        CheckinDate2 TEXT,
                        CheckinDate3 TEXT,
                        FOREIGN KEY (BikeID) REFERENCES Bikes(BikeID)
                    );
                ");
            }
        }

        private static void Execute(SqliteConnection conn, string query)
        {
            using (var cmd = new SqliteCommand(query, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
