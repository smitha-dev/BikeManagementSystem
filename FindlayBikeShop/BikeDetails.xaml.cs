using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
        }

        // ===========================
        // Navigation buttons
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
            bool? result = editWindow.ShowDialog();
            if (result == true)
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
                SELECT MaintenanceID, BikeID, Notes, Cost, DateFlagged
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
                    Notes = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Cost = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                    DateFlagged = reader.IsDBNull(4) ? null : reader.GetString(4)
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
            // Create a new maintenance record
            int newID = CreateNewMaintenanceRecord(currentBike.BikeID);

            // Open the maintenance window
            var window = new MaintenanceHistory(newID);
            window.Show();

            // Refresh the grid after closing
            window.Closed += (s, args) => LoadMaintenanceHistory();

            // Clear selection so no record is highlighted
            MaintenanceHistoryGrid.SelectedItem = null;

            // Update button visibility
            FlagMaintenanceButton.Visibility = Visibility.Visible;
            OpenMaintenanceButton.Visibility = Visibility.Collapsed;

        }

        private void MaintenanceHistoryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MaintenanceHistoryGrid.SelectedItem != null)
            {
                // If a record is selected, hide "Flag for Maintenance" and show "Open Maintenance"
                FlagMaintenanceButton.Visibility = Visibility.Collapsed;
                OpenMaintenanceButton.Visibility = Visibility.Visible;
            }
            else
            {
                // No record selected, show "Flag for Maintenance" and hide "Open Maintenance"
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
        // Edit bike details
        // ===========================
        private void EditDetails_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new AddBike(currentBike);
            editWindow.ShowDialog();

            // Refresh page after editing
            var refreshedWindow = new BikeDetails(currentBike);
            refreshedWindow.Show();
            this.Close();
        }
    }

    // ===========================
    // Maintenance record class
    // ===========================
    public class MaintenanceRecord
    {
        public int MaintenanceID { get; set; }
        public int BikeID { get; set; }
        public string? Notes { get; set; }
        public double Cost { get; set; }
        public string? DateFlagged { get; set; }
    }
}
