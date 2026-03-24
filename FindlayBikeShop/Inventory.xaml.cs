using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System;
using System.Collections.Generic;

namespace FindlayBikeShop
{
    /// <summary>
    /// Interaction logic for Inventory.xaml
    /// </summary>
    public partial class Inventory : Window
    {
        private string connectionString = "Data Source=BikeDatabase.db";

    public Inventory()
        {
            InitializeComponent();
            LoadAllBikes();
        }

        private void LoadAllBikes()
        {
            var bikes = new List<Bike>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string sql = @"SELECT b.BikeID, b.Brand, b.Size, b.Color, b.Status, b.LastUpdated,
                      p.FilePath
               FROM Bikes b
               LEFT JOIN Photos p ON b.BikeID = p.BikeID
               GROUP BY b.BikeID
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
                            Color = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Status = reader.IsDBNull(4) ? null : reader.GetString(4),
                            LastUpdated = reader.IsDBNull(5)
                                ? null
                                : reader.GetDateTime(5).ToString("yyyy-MM-dd"),
                            Photo = reader.IsDBNull(6) ? null : reader.GetString(6)
                    });
                    }
                }
            }

            BikesListView.ItemsSource = bikes;
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            // Do nothing since already here
        }

        private void AddBike_Click(object sender, RoutedEventArgs e)
        {
            var addBikeWindow = new AddBike();
            addBikeWindow.ShowDialog();

            // Refresh list after adding a bike
            LoadAllBikes();
        }
    }

}
