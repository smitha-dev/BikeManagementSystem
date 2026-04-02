using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FindlayBikeShop
{
    public partial class BikeDetails : Window
    {
        private string connectionString = "Data Source=BikeDatabase.db";
        private Bike currentBike;

        public BikeDetails(Bike bike)
        {
            InitializeComponent();

            currentBike = bike;
            this.DataContext = bike;

            LoadRentalHistory();
            LoadMaintenanceHistory();
            LoadBikePhoto();
        }

        // ===========================
        // Navigation
        // ===========================
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            new Inventory().Show();
            this.Close();
        }

        // ===========================
        // Rental history
        // ===========================
        private void LoadRentalHistory()
        {
            var rentals = new List<RentalRecord>();

            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            string sql = @"
                SELECT RentalID, BikeID, StudentID, SemesterRented, Year,
                       CheckoutDate, DueDate, ReturnDate,
                       CheckinDate1, CheckinDate2, CheckinDate3
                FROM Rentals
                WHERE BikeID = @bikeId
                ORDER BY CheckoutDate DESC;
            ";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@bikeId", currentBike.BikeID);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                rentals.Add(new RentalRecord
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
                });
            }

            RentalHistoryGrid.ItemsSource = rentals;
        }

        private RentalRecord? GetSelectedRental()
        {
            return RentalHistoryGrid.SelectedItem as RentalRecord;
        }

        private void EditHistory_Click(object sender, RoutedEventArgs e)
        {
            var selectedRental = GetSelectedRental();
            if (selectedRental == null)
            {
                MessageBox.Show("Please select a rental record first.");
                return;
            }

            var editWindow = new EditRentalHistory(selectedRental);
            if (editWindow.ShowDialog() == true)
                LoadRentalHistory();
        }

        // ===========================
        // Maintenance history
        // ===========================
        private void LoadMaintenanceHistory()
        {
            var maintenanceList = new List<MaintenanceRecord>();

            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            string sql = @"
                SELECT MaintenanceID, BikeID, DateFlagged, DateFixed, Notes, Cost
                FROM Maintenance
                WHERE BikeID = @bikeId
                ORDER BY DateFlagged DESC;
            ";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@bikeId", currentBike.BikeID);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                maintenanceList.Add(new MaintenanceRecord
                {
                    MaintenanceID = reader.GetInt32(0),
                    BikeID = reader.GetInt32(1),
                    DateFlagged = reader.IsDBNull(2) ? null : reader.GetString(2),
                    DateFixed = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Notes = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Cost = reader.IsDBNull(5) ? 0 : reader.GetDouble(5)
                });
            }

            MaintenanceHistoryGrid.ItemsSource = maintenanceList;
        }

        private void OpenMaintenance_Click(object sender, RoutedEventArgs e)
        {
            if (MaintenanceHistoryGrid.SelectedItem is MaintenanceRecord selected)
            {
                var window = new MaintenanceHistory(selected.MaintenanceID);
                window.Show();
                window.Closed += (s, args) => LoadMaintenanceHistory();
            }
            else
            {
                MessageBox.Show("Please select a maintenance record.");
            }
        }

        private void Maintenance_Click(object sender, RoutedEventArgs e)
        {
            int newID = CreateNewMaintenanceRecord(currentBike.BikeID);

            var window = new MaintenanceHistory(newID);
            window.Show();
            window.Closed += (s, args) => LoadMaintenanceHistory();

            MaintenanceHistoryGrid.SelectedItem = null;

            FlagMaintenanceButton.Visibility = Visibility.Visible;
            OpenMaintenanceButton.Visibility = Visibility.Collapsed;
        }

        private void MaintenanceHistoryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MaintenanceHistoryGrid.SelectedItem != null)
            {
                FlagMaintenanceButton.Visibility = Visibility.Collapsed;
                OpenMaintenanceButton.Visibility = Visibility.Visible;
            }
            else
            {
                FlagMaintenanceButton.Visibility = Visibility.Visible;
                OpenMaintenanceButton.Visibility = Visibility.Collapsed;
            }
        }

        private int CreateNewMaintenanceRecord(int bikeID)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Maintenance (BikeID, DateFlagged)
                VALUES ($bikeId, datetime('now'));
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("$bikeId", bikeID);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // ===========================
        // Rent bike
        // ===========================
        private void RentBike_Click(object sender, RoutedEventArgs e)
        {
            var rentWindow = new EditRentalHistory(currentBike);
            bool? result = rentWindow.ShowDialog();

            if (result == true)
            {
                currentBike.Status = "Rented";
                currentBike.LastUpdated = DateTime.Now.ToString("yyyy-MM-dd");

                LoadRentalHistory();

                DataContext = null;
                DataContext = currentBike;
            }
        }

        // ===========================
        // Edit bike
        // ===========================
        private void EditDetails_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new AddBike(currentBike);
            editWindow.ShowDialog();

            var refreshedWindow = new BikeDetails(currentBike);
            refreshedWindow.Show();
            this.Close();
        }

        // ===========================
        // PHOTO FEATURE
        // ===========================
        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

                if (dialog.ShowDialog() == true)
                {
                    string sourcePath = dialog.FileName;

                    string fileName = "bike_" + DateTime.Now.Ticks +
                                      System.IO.Path.GetExtension(sourcePath);

                    string folder = System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Images",
                        "BikeDetails"
                    );

                    if (!System.IO.Directory.Exists(folder))
                        System.IO.Directory.CreateDirectory(folder);

                    string destinationPath = System.IO.Path.Combine(folder, fileName);

                    System.IO.File.Copy(sourcePath, destinationPath, true);

                    BikeImage.Source = new BitmapImage(new Uri(destinationPath));

                    SaveBikePhoto("Images/BikeDetails/" + fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error uploading image:\n" + ex.Message);
            }
        }

        private void SaveBikePhoto(string relativePath)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
                INSERT INTO Photos (BikeID, FilePath, PhotoType)
                VALUES ($bikeID, $path, 'BikeDetails')
            ";

            command.Parameters.AddWithValue("$bikeID", currentBike.BikeID);
            command.Parameters.AddWithValue("$path", relativePath);

            command.ExecuteNonQuery();
        }

        private void LoadBikePhoto()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT FilePath
                FROM Photos
                WHERE BikeID = $id AND PhotoType = 'BikeDetails'
                ORDER BY PhotoID DESC
                LIMIT 1
            ";

            command.Parameters.AddWithValue("$id", currentBike.BikeID);

            var result = command.ExecuteScalar();

            if (result is string path && !string.IsNullOrWhiteSpace(path))
            {
                string fullPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    path
                );

                if (System.IO.File.Exists(fullPath))
                {
                    BikeImage.Source = new BitmapImage(new Uri(fullPath));
                }
            }
        }
    }
}