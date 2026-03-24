using Microsoft.Data.Sqlite;
using System.Collections.Generic;
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
        // Bikes needing repair
        // ==============================
        private void LoadMaintenanceBikes()
        {
            var bikes = new List<Bike>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string sql = @"SELECT b.BikeID, m.Notes, b.LastUpdated
                               FROM Bikes b
                               JOIN Maintenance m ON b.BikeID = m.BikeID
                               WHERE b.Status = 'Maintenance'
                               ORDER BY b.BikeID;";

                using (var cmd = new SqliteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bikes.Add(new Bike
                        {
                            BikeID = reader.GetInt32(0),
                            Notes = reader.IsDBNull(1) ? null : reader.GetString(1),
                            LastUpdated = reader.GetDateTime(2).ToString("MM-dd-yyyy")

                        });
                    }
                }
            }

            BikeNeedToRepair.ItemsSource = bikes;
            BikeNeedToRepair.DisplayMemberPath = "Display"; // shows BikeID - Status - LastUpdated
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

                string sql = @"SELECT BikeID, Brand, Size, Color
                               FROM Bikes
                               WHERE Status = 'Available'";

                using (var cmd = new SqliteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bikes.Add(new Bike
                        {
                            BikeID = reader.GetInt32(0),
                            Brand = reader.GetString(1),
                            Size = reader.GetString(2),
                            Color = reader.GetString(3)
                        });
                    }
                }
            }

            AvailableBikeList.ItemsSource = bikes;
            AvailableBikeList.DisplayMemberPath = "Display";
        }

        // ==============================
        // Rented bikes
        // ==============================
        private void LoadRentedBikes()
        {
            var bikes = new List<Bike>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string sql = @"SELECT BikeID, Brand, Color
                               FROM Bikes
                               WHERE Status = 'Rented'";

                using (var cmd = new SqliteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bikes.Add(new Bike
                        {
                            BikeID = reader.GetInt32(0),
                            Brand = reader.GetString(1),
                            Color = reader.GetString(2)
                        });
                    }
                }
            }

            RentedBikeList.ItemsSource = bikes;
            RentedBikeList.DisplayMemberPath = "Display";
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
    }

    // ==============================
    // Bike object
    // ==============================
}
