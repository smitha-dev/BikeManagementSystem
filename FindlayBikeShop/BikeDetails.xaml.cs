using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        // MASTER REFRESH METHOD
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
            UpdateRentalButtonState();
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
                currentBike.MaxHeight = reader.GetDouble(2);
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
            {
                RefreshBikeDetails();
            }
        }

        // ===========================
        // Maintenance history
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

        // maintenance history button logic

        private void OpenMaintenance_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCreateOrOpenMaintenance(out string message))
            {
                MessageBox.Show(message,
                                "Action Blocked",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            if (MaintenanceHistoryGrid.SelectedItem is MaintenanceRecord selected)
            {
                var window = new MaintenanceHistory(selected.MaintenanceID, currentBike.BikeID);
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
            if (!CanCreateOrOpenMaintenance(out string message))
            {
                MessageBox.Show(message,
                                "Action Blocked",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            int newID = CreateNewMaintenanceRecord(currentBike.BikeID);

            var window = new MaintenanceHistory(newID, currentBike.BikeID);
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
        // Rent bike
        // ===========================
        private void RentBike_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRentBike(out string message))
            {
                MessageBox.Show(message,
                                "Action Blocked",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            var rentWindow = new EditRentalHistory(currentBike);

            if (rentWindow.ShowDialog() == true)
            {
                RefreshBikeDetails();
            }
        }

        // ===========================
        // Edit bike
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
                string sourcepath = dialog.FileName;
                string timestamp = DateTime.Now.ToString("mm-dd-yyyy_hh-mm-ss");
                string fileName = $"bike_{currentBike.BikeID}_{timestamp}{System.IO.Path.GetExtension(sourcepath)}";

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
        // Rental button state logic
        // ===========================
        private void RentalHistoryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRentalButtonState();
        }

        private void RentalHistoryGrid_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                var row = ItemsControl.ContainerFromElement(RentalHistoryGrid, source) as DataGridRow;

                if (row == null)
                {
                    RentalHistoryGrid.SelectedItem = null;
                }
            }
        }

        private void UpdateRentalButtonState()
        {
            bool hasSelection = RentalHistoryGrid.SelectedItem != null;

            RentBikeButton.Visibility = hasSelection ? Visibility.Collapsed : Visibility.Visible;
            EditRentalButton.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;
        }

        // ===========================
        // Maintenance button state logic
        // ===========================
        private void MaintenanceHistoryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMaintenanceButtonState();
        }

        private void MaintenanceHistoryGrid_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                var row = ItemsControl.ContainerFromElement(MaintenanceHistoryGrid, source) as DataGridRow;

                if (row == null)
                {
                    MaintenanceHistoryGrid.SelectedItem = null;
                }
            }
        }
        private void UpdateMaintenanceButtonState()
        {
            bool hasSelection = MaintenanceHistoryGrid.SelectedItem != null;

            FlagMaintenanceButton.Visibility = hasSelection ? Visibility.Collapsed : Visibility.Visible;
            OpenMaintenanceButton.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;

            // Keep buttons clickable so handlers can show warning messages
            FlagMaintenanceButton.IsEnabled = true;
            OpenMaintenanceButton.IsEnabled = true;

            FlagMaintenanceButton.ToolTip = null;
            OpenMaintenanceButton.ToolTip = null;
        }


        // Centralized Rule Helpers
        private string? GetBikeStatus(SqliteConnection connection, int bikeId)
        {
            using var cmd = new SqliteCommand(
                "SELECT Status FROM Bikes WHERE BikeID = @bikeId;",
                connection);

            cmd.Parameters.AddWithValue("@bikeId", bikeId);

            var result = cmd.ExecuteScalar();
            return result?.ToString();
        }

        private bool HasOpenMaintenance(SqliteConnection connection, int bikeId)
        {
            using var cmd = new SqliteCommand(@"
        SELECT COUNT(1)
        FROM Maintenance
        WHERE BikeID = @bikeId
          AND DateFixed IS NULL;", connection);

            cmd.Parameters.AddWithValue("@bikeId", bikeId);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private bool CanRentBike(out string message)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string? bikeStatus = GetBikeStatus(connection, currentBike.BikeID);
            bool hasOpenMaintenance = HasOpenMaintenance(connection, currentBike.BikeID);

            if (string.Equals(bikeStatus, "Rented", StringComparison.OrdinalIgnoreCase))
            {
                message = "This bike is already rented and cannot be rented again until it is returned.";
                return false;
            }

            if (string.Equals(bikeStatus, "Maintenance", StringComparison.OrdinalIgnoreCase))
            {
                message = "This bike is currently in maintenance and cannot be rented.";
                return false;
            }

            if (string.Equals(bikeStatus, "Retired", StringComparison.OrdinalIgnoreCase))
            {
                message = "This bike is retired and cannot be rented.";
                return false;
            }

            if (hasOpenMaintenance)
            {
                message = "This bike has an open maintenance record and cannot be rented until maintenance is completed.";
                return false;
            }

            message = "";
            return true;
        }

        private bool CanCreateOrOpenMaintenance(out string message)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string? bikeStatus = GetBikeStatus(connection, currentBike.BikeID);

            if (string.Equals(bikeStatus, "Rented", StringComparison.OrdinalIgnoreCase))
            {
                message = "This bike is currently rented. You cannot create or edit maintenance until it is returned.";
                return false;
            }

            if (string.Equals(bikeStatus, "Retired", StringComparison.OrdinalIgnoreCase))
            {
                message = "This bike is retired. Maintenance records should not be created for retired bikes.";
                return false;
            }

            message = "";
            return true;
        }
    }
}