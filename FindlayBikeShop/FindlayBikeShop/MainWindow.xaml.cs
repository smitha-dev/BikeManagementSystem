using Microsoft.Data.Sqlite;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace FindlayBikeShop
{
    public partial class MainWindow : Window
    {
        private string connectionString = "Data Source=BikeDatabase.db";

        public MainWindow()
        {
            InitializeComponent();

            // Load all bike lists on startup
            LoadMaintenanceBikes();
            LoadAvailableBikes();
            LoadRentedBikes();
        }

        // ==============================
        // Maintenance Bikes
        // ==============================
        private void LoadMaintenanceBikes()
        {
            var bikes = new List<Bike>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string sql = @"SELECT b.BikeID, b.Brand, b.Size, b.MaxHeight, b.MinHeight, b.Color, b.Status, b.LastUpdated, m.Notes
                                FROM Bikes b
                                LEFT JOIN Maintenance m ON b.BikeID = m.BikeID
                                WHERE b.Status = 'Maintenance'
                                  AND m.MaintenanceID = (
                                      SELECT MAX(MaintenanceID)
                                      FROM Maintenance
                                      WHERE BikeID = b.BikeID
                                  )
                                ORDER BY b.BikeID;";

                using (var cmd = new SqliteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bikes.Add(new Bike
                        {
                            BikeID = reader.GetInt32(0),
                            Brand = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Size = reader.IsDBNull(2) ? null : reader.GetString(2),
                            MaxHeight = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                            MinHeight = reader.IsDBNull(3) ? 0 : reader.GetDouble(4),
                            Color = reader.IsDBNull(4) ? null : reader.GetString(5),
                            Status = reader.IsDBNull(5) ? null : reader.GetString(6),
                            LastUpdated = reader.IsDBNull(6) ? null : reader.GetDateTime(7).ToString("MMM-dd-yyyy"),
                            
                        });
                    }
                }
            }
            BikeNeedToRepair.ItemsSource = bikes;
        }

        // ==============================
        // Available bikes
        // ==============================
        private void LoadAvailableBikes()
        {
            var bikes = new List<Bike>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string sql = @"SELECT BikeID, Brand, Size, MaxHeight, MinHeight, Color, Status, LastUpdated
                       FROM Bikes
                       WHERE Status = 'Available'
                       ORDER BY BikeID;";

                using (var cmd = new SqliteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bikes.Add(new Bike
                        {
                            BikeID = reader.GetInt32(0),
                            Brand = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Size = reader.IsDBNull(2) ? null : reader.GetString(2),
                            MaxHeight = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                            MinHeight = reader.IsDBNull(3) ? 0 : reader.GetDouble(4),
                            Color = reader.IsDBNull(4) ? null : reader.GetString(5),
                            Status = reader.IsDBNull(5) ? null : reader.GetString(6),
                            LastUpdated = reader.IsDBNull(6) ? null : reader.GetDateTime(7).ToString("MMM-dd-yyyy"),
                            
                        });
                    }
                }
            }
            AvailableBikeList.ItemsSource = bikes;
        }

        // ==============================
        // Unavailable bikes
        // ==============================
        private void LoadRentedBikes()
        {
            var bikes = new List<Bike>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string sql = @"SELECT BikeID, Brand, Size, MaxHeight, MinHeight, Color, Status, LastUpdated
                       FROM Bikes 
                       WHERE Status = 'Rented' OR Status = 'Retired'
                       ORDER BY BikeID;";

                using (var cmd = new SqliteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bikes.Add(new Bike
                        {
                            BikeID = reader.GetInt32(0),
                            Brand = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Size = reader.IsDBNull(2) ? null : reader.GetString(2),
                            MaxHeight = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                            MinHeight = reader.IsDBNull(3) ? 0 : reader.GetDouble(4),
                            Color = reader.IsDBNull(4) ? null : reader.GetString(5),
                            Status = reader.IsDBNull(5) ? null : reader.GetString(6),
                            LastUpdated = reader.IsDBNull(6) ? null : reader.GetDateTime(7).ToString("MMM -dd-yyyy"),
                            
                        });
                    }
                }
            }
            RentedBikeList.ItemsSource = bikes;
        }

        // ==============================
        // Navigation buttons
        // ==============================
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            var inventoryWindow = new Inventory();
            inventoryWindow.Show();
            this.Close();
        }

        private void BikeNeedToRepair_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BikeNeedToRepair.SelectedItem is Bike selectedBike)
            {
                var detailsWindow = new BikeDetails(selectedBike);
                detailsWindow.Show();
                this.Close();
            }
        }

        private void AvailableBikeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AvailableBikeList.SelectedItem is Bike selectedBike)
            {
                var detailsWindow = new BikeDetails(selectedBike);
                detailsWindow.Show();
                this.Close();
            }
        }

        private void RentedBikeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RentedBikeList.SelectedItem is Bike selectedBike)
            {
                var detailsWindow = new BikeDetails(selectedBike);
                detailsWindow.Show();
                this.Close();
            }

        }

        private void Backup_Click(object sender, RoutedEventArgs e)
        {
            BackupHelper.BackupBikeData();
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
        "Restoring will overwrite your current database and images. Continue?",
        "Confirm Restore",
        MessageBoxButton.YesNo,
        MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
                BackupHelper.RestoreBikeData(); // calls the restore function
        }
    }
}
