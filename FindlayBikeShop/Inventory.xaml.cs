using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace FindlayBikeShop
{
    public partial class Inventory : Window
    {
        private string connectionString = "Data Source=BikeDatabase.db";
        private ICollectionView? bikesView;

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
            bikesView = CollectionViewSource.GetDefaultView(BikesListView.ItemsSource);
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
            LoadAllBikes();
        }

            private void BikesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BikesListView.SelectedItem is Bike selectedBike)
        {
            var detailsWindow = new BikeDetails(selectedBike);
            detailsWindow.Show();
            this.Close();
        }
    }
}
        private void RentReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Bike bike)
            {
                string newStatus = bike.Status == "Rented" ? "Available" : "Rented";

                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                string sql = "UPDATE Bikes SET Status = @status WHERE BikeID = @id";
                using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@status", newStatus);
                cmd.Parameters.AddWithValue("@id", bike.BikeID);
                cmd.ExecuteNonQuery();

                bike.Status = newStatus;
                BikesListView.Items.Refresh();
            }
        }

         private void FilterByStatus(string status)
 {
     if (bikesView == null) return;

     bikesView.Filter = obj =>
     {
         if (obj is Bike bike)
         {
             // Default "All" → hide retired bikes
             if (status == "All")
                 return bike.Status != "Retired";

             // Specific filter → show only that status
             return bike.Status == status;
         }

         return false;
     };
 }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusFilterComboBox.SelectedItem is ComboBoxItem item)
            {
                string selectedStatus = item.Content.ToString() ?? "All";
                FilterByStatus(selectedStatus);
            }
        }
    }

    public class StatusToButtonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? status = value?.ToString();

            // button text
            if (status == "Rented")
                return "Return";

            if (status == "Retired")
                return "Retired";

            return "Rent";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToIsEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // disable rent button for bikes that need maintenance or are retired
            string? status = value?.ToString();
            return status != "Maintenance" && status != "Retired";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
