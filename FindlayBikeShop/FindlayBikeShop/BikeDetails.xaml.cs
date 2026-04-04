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
            this.DataContext = currentBike;

            RefreshBikeDetails();
        }

        // ===========================
        // 🔥 MASTER REFRESH METHOD
        // ===========================
        private void RefreshBikeDetails()
        {
            ReloadBikeFromDatabase();
            LoadRentalHistory();
            LoadMaintenanceHistory();
            LoadBikePhoto();

            DataContext = null;
            DataContext = currentBike;

            UpdateMaintenanceButtonState();
        }

        // ===========================
        // Reload bike from DB
        // ===========================
        private void ReloadBikeFromDatabase()
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Brand, Size, MaxHeight, MinHeight, Color, Status, LastUpdated
                FROM Bikes
                WHERE BikeID = $id
            ";
            cmd.Parameters.AddWithValue("$id", currentBike.BikeID);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                currentBike.Brand = reader.GetString(0);
                currentBike.Size = reader.GetString(1);
                currentBike.MaxHeight = reader.GetDouble (2);
                currentBike.MinHeight = reader.GetDouble(3);
                currentBike.Color = reader.GetString(4);
                currentBike.Status = reader.GetString(5);
                currentBike.LastUpdated = FormatDate(reader.GetString(6));
            }
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
        // Rental History
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
                    CheckoutDate = reader.IsDBNull(5) ? null : FormatDate(reader.GetString(5)),
                    DueDate = reader.IsDBNull(6) ? null : FormatDate(reader.GetString(6)),
                    ReturnDate = reader.IsDBNull(7) ? null : FormatDate(reader.GetString(7)),
                    CheckinDate1 = reader.IsDBNull(8) ? null : FormatDate(reader.GetString(8)),
                    CheckinDate2 = reader.IsDBNull(9) ? null : FormatDate(reader.GetString(9)),
                    CheckinDate3 = reader.IsDBNull(10) ? null : FormatDate(reader.GetString(10))
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
            {
                RefreshBikeDetails();
            }
        }

        // ===========================
        // Maintenance History
        // ===========================
        private void LoadMaintenanceHistory()
        {
            var list = new List<MaintenanceRecord>();

            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            string sql = @"
                SELECT MaintenanceID, Notes, PartNeeded, Cost, DateFlagged, DateFixed
                FROM Maintenance
                WHERE BikeID = @bikeId
                ORDER BY DateFlagged DESC;
            ";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@bikeId", currentBike.BikeID);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new MaintenanceRecord
                {
                    MaintenanceID = reader.GetInt32(0),
                    PartNeeded = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Notes = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Cost = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                    DateFlagged = reader.IsDBNull(4) ? null : FormatDate(reader.GetString(4)),
                    DateFixed = reader.IsDBNull(5) ? null : FormatDate(reader.GetString(5))

                });
            }

            MaintenanceHistoryGrid.ItemsSource = list;
        }

        private void OpenMaintenance_Click(object sender, RoutedEventArgs e)
        {
            if (string.Equals(currentBike.Status, "Rented", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("This bike is currently rented. Cannot open maintenance records.",
                                "Action Blocked",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            if (MaintenanceHistoryGrid.SelectedItem is MaintenanceRecord selected)
            {
                var window = new MaintenanceHistory(selected.MaintenanceID);
                window.Show();
                window.Closed += (s, args) => RefreshBikeDetails();
            }
            else
            {
                MessageBox.Show("Please select a maintenance record.");
            }
        }

        private void Maintenance_Click(object sender, RoutedEventArgs e)
        {
            if (string.Equals(currentBike.Status, "Rented", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("This bike is currently rented. Cannot create maintenance record.",
                                "Action Blocked",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }


            int newID = CreateNewMaintenanceRecord(currentBike.BikeID);

            var window = new MaintenanceHistory(newID);
            window.Show();
            window.Closed += (s, args) => RefreshBikeDetails();

            MaintenanceHistoryGrid.SelectedItem = null;
        }

        private int CreateNewMaintenanceRecord(int bikeID)
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Maintenance (BikeID, DateFlagged)
                VALUES ($bikeId, datetime('now'));
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("$bikeId", bikeID);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // ===========================
        // Rent Bike
        // ===========================
        private void RentBike_Click(object sender, RoutedEventArgs e)
        {
            var rentWindow = new EditRentalHistory(currentBike);

            if (rentWindow.ShowDialog() == true)
            {
                RefreshBikeDetails();
            }
        }

        // ===========================
        // Edit Bike
        // ===========================
        private void EditDetails_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new AddBike(currentBike);

            if (editWindow.ShowDialog() == true)
            {
                RefreshBikeDetails();
            }
        }

        // ===========================
        // Photo
        // ===========================
        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files (*.png;*.jpg)|*.png;*.jpg";

            if (dialog.ShowDialog() == true)
            {
                string fileName = "bike_" + DateTime.Now.Ticks +
                                  System.IO.Path.GetExtension(dialog.FileName);

                string folder = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Images", "BikeDetails");

                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                string dest = System.IO.Path.Combine(folder, fileName);

                System.IO.File.Copy(dialog.FileName, dest, true);

                BikeImage.Source = new BitmapImage(new Uri(dest));

                SaveBikePhoto("Images/BikeDetails/" + fileName);
            }
        }

        private void SaveBikePhoto(string path)
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Photos (BikeID, FilePath, PhotoType)
                VALUES ($bikeID, $path, 'BikeDetails')
            ";

            cmd.Parameters.AddWithValue("$bikeID", currentBike.BikeID);
            cmd.Parameters.AddWithValue("$path", path);

            cmd.ExecuteNonQuery();
        }

        private void LoadBikePhoto()
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT FilePath
                FROM Photos
                WHERE BikeID = $id AND PhotoType = 'BikeDetails'
                ORDER BY PhotoID DESC
                LIMIT 1
            ";

            cmd.Parameters.AddWithValue("$id", currentBike.BikeID);

            var result = cmd.ExecuteScalar();

            if (result is string path)
            {
                string full = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    path);

                if (System.IO.File.Exists(full))
                {
                    BikeImage.Source = new BitmapImage(new Uri(full));
                }
            }
        }

        // ===========================
        // Maintenance Button Visibility
        // ===========================
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

        // ===========================
        // Date Formatting Helper
        // ===========================
        private string FormatDate(string? dbDate)
        {
            if (string.IsNullOrEmpty(dbDate))
                return "";

            if (DateTime.TryParse(dbDate, out DateTime parsed))
                return parsed.ToString("MMM-dd-yyyy");

            return dbDate; // fallback
        }

        // ===========================
        // Disable maintenance buttons if bike is rented
        // ===========================
        private void UpdateMaintenanceButtonState()
        {
            bool isRented = string.Equals(currentBike.Status, "Rented", StringComparison.OrdinalIgnoreCase);

            FlagMaintenanceButton.IsEnabled = !isRented;
            OpenMaintenanceButton.IsEnabled = !isRented;

            // Optional: tooltip to show why buttons are disabled
            if (isRented)
            {
                FlagMaintenanceButton.ToolTip = "Cannot flag for maintenance while bike is rented";
                OpenMaintenanceButton.ToolTip = "Cannot open maintenance while bike is rented";
            }
            else
            {
                FlagMaintenanceButton.ToolTip = null;
                OpenMaintenanceButton.ToolTip = null;
            }
        }
    }
}