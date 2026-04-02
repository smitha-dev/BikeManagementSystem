using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

                string sql = @"
                    SELECT b.BikeID, b.Brand, b.Size, b.Color, b.Status, b.LastUpdated,
                           (
                               SELECT FilePath
                               FROM Photos
                               WHERE BikeID = b.BikeID AND PhotoType = 'BikeDetails'
                               ORDER BY PhotoID DESC
                               LIMIT 1
                           ) AS FilePath
                    FROM Bikes b
                    ORDER BY b.BikeID;
                ";

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
        }

        private void AddBike_Click(object sender, RoutedEventArgs e)
        {
            var addBikeWindow = new AddBike();
            addBikeWindow.ShowDialog();
            LoadAllBikes();
        }

        private void RentReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not Bike bike)
                return;

            if (bike.Status == "Available")
            {
                var rentWindow = new EditRentalHistory(bike);
                bool? result = rentWindow.ShowDialog();

                if (result == true)
                    LoadAllBikes();
            }
            else if (bike.Status == "Rented")
            {
                RentalRecord? activeRental = GetActiveRentalForBike(bike.BikeID);

                if (activeRental == null)
                {
                    MessageBox.Show("No active rental record was found for this bike.");
                    return;
                }

                var returnWindow = new EditRentalHistory(activeRental);
                bool? result = returnWindow.ShowDialog();

                if (result == true)
                    LoadAllBikes();
            }
        }

        private RentalRecord? GetActiveRentalForBike(int bikeId)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string sql = @"
                SELECT RentalID, BikeID, StudentID, SemesterRented, Year,
                       CheckoutDate, DueDate, ReturnDate,
                       CheckinDate1, CheckinDate2, CheckinDate3
                FROM Rentals
                WHERE BikeID = @bikeId
                  AND (ReturnDate IS NULL OR ReturnDate = '')
                ORDER BY CheckoutDate DESC
                LIMIT 1;";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@bikeId", bikeId);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new RentalRecord
                {
                    RentalID = reader.GetInt32(0),
                    BikeID = reader.GetInt32(1),
                    StudentID = reader.IsDBNull(2) ? null : reader.GetString(2),
                    SemesterRented = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Year = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    CheckoutDate = reader.IsDBNull(5) ? null : reader.GetString(5),
                    DueDate = reader.IsDBNull(6) ? null : reader.GetString(6),
                    ReturnDate = reader.IsDBNull(7) ? null : reader.GetString(7),
                    CheckinDate1 = reader.IsDBNull(8) ? null : reader.GetString(8),
                    CheckinDate2 = reader.IsDBNull(9) ? null : reader.GetString(9),
                    CheckinDate3 = reader.IsDBNull(10) ? null : reader.GetString(10)
                };
            }

            return null;
        }

        private void FilterByStatus(string status)
        {
            if (bikesView == null) return;

            bikesView.Filter = obj =>
            {
                if (obj is Bike bike)
                {
                    if (status == "All")
                        return bike.Status != "Retired";

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

    public class StatusToButtonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? status = value?.ToString();

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
            string? status = value?.ToString();
            return status != "Maintenance" && status != "Retired";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}