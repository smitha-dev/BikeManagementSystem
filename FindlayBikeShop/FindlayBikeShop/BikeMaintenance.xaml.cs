using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace FindlayBikeShop
{
    public partial class MaintenanceHistory : Window
    {
        private int currentMaintenanceID;
        private string connectionString = "Data Source=BikeDatabase.db";

        public MaintenanceHistory(int maintenanceID)
        {
            InitializeComponent();
            currentMaintenanceID = maintenanceID;
            LoadMaintenanceDetails();
        }

        // ===========================
        // Load maintenance record
        // ===========================
        private void LoadMaintenanceDetails()
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Notes, Cost, PartNeeded
                FROM Maintenance
                WHERE MaintenanceID = $mid;
            ";
            cmd.Parameters.AddWithValue("$mid", currentMaintenanceID);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                NoteTextBox.Text = reader.IsDBNull(0) ? "" : reader.GetString(0);
                CostTextBox.Text = reader.IsDBNull(1) ? "0" : reader.GetDouble(1).ToString();
                PartNeededBox.Text = reader.IsDBNull(2) ? "" : reader.GetString(2);
            }

            LoadPhotoFromDatabase();
        }

        // ===========================
        // Save maintenance record
        // ===========================
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string note = NoteTextBox.Text;
            double.TryParse(CostTextBox.Text, out double cost);
            string partNeeded = PartNeededBox.Text;

            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Maintenance
                SET Notes = $note,
                    Cost = $cost,
                    PartNeeded = $part
                WHERE MaintenanceID = $mid;
            ";
            cmd.Parameters.AddWithValue("$note", note);
            cmd.Parameters.AddWithValue("$cost", cost);
            cmd.Parameters.AddWithValue("$part", partNeeded);
            cmd.Parameters.AddWithValue("$mid", currentMaintenanceID);
            cmd.ExecuteNonQuery();

            // Update bike status to "Maintenance"
            var getBikeCmd = conn.CreateCommand();
            getBikeCmd.CommandText = "SELECT BikeID FROM Maintenance WHERE MaintenanceID = $mid";
            getBikeCmd.Parameters.AddWithValue("$mid", currentMaintenanceID);
            var result = getBikeCmd.ExecuteScalar();

            if (result != null)
            {
                int bikeID = Convert.ToInt32(result);
                var bikeCmd = conn.CreateCommand();
                bikeCmd.CommandText = @"
                    UPDATE Bikes
                    SET Status = 'Maintenance'
                    WHERE BikeID = $bikeId;
                ";
                bikeCmd.Parameters.AddWithValue("$bikeId", bikeID);
                bikeCmd.ExecuteNonQuery();
            }

            MessageBox.Show("Saved!");
            this.Close();
        }

        // ===========================
        // Mark bike as fixed
        // ===========================
        private void FixedButton_Click(object sender, RoutedEventArgs e)
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT BikeID FROM Maintenance WHERE MaintenanceID = $mid";
            cmd.Parameters.AddWithValue("$mid", currentMaintenanceID);

            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                int bikeID = Convert.ToInt32(result);

                // Update bike status
                var updateCmd = conn.CreateCommand();
                updateCmd.CommandText = "UPDATE Bikes SET Status='Available' WHERE BikeID=$bid";
                updateCmd.Parameters.AddWithValue("$bid", bikeID);
                updateCmd.ExecuteNonQuery();

                // Update maintenance record to set DateFixed with month name
                var fixCmd = conn.CreateCommand();
                fixCmd.CommandText = @"
                    UPDATE Maintenance
                    SET DateFixed = $dateFixed
                    WHERE MaintenanceID = $mid;
                ";
                fixCmd.Parameters.AddWithValue("$dateFixed", DateTime.Now.ToString("MMM-dd-yyyy HH:mm:ss"));
                fixCmd.Parameters.AddWithValue("$mid", currentMaintenanceID);
                fixCmd.ExecuteNonQuery();

                MessageBox.Show("Bike marked as Available!");
                this.Close();
            }
        }

        // ===========================
        // Upload damage photo
        // ===========================
        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (dialog.ShowDialog() == true)
            {
                string sourcePath = dialog.FileName;
                string fileName = "bike_" + DateTime.Now.Ticks + Path.GetExtension(sourcePath);

                string destinationFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Maintenance");
                if (!Directory.Exists(destinationFolder))
                    Directory.CreateDirectory(destinationFolder);

                string destinationPath = Path.Combine(destinationFolder, fileName);
                File.Copy(sourcePath, destinationPath, true);

                DamagePhoto.Source = new BitmapImage(new Uri(destinationPath));
                SavePhotoPathToDatabase("Images/Maintenance/" + fileName);
            }
        }

        private void SavePhotoPathToDatabase(string relativePath)
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Photos (MaintenanceID, FilePath, PhotoType)
                VALUES ($mid, $path, 'Maintenance');
            ";
            cmd.Parameters.AddWithValue("$mid", currentMaintenanceID);
            cmd.Parameters.AddWithValue("$path", relativePath);
            cmd.ExecuteNonQuery();
        }

        private void LoadPhotoFromDatabase()
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT FilePath
                FROM Photos
                WHERE MaintenanceID = $mid
                ORDER BY PhotoID DESC
                LIMIT 1;
            ";
            cmd.Parameters.AddWithValue("$mid", currentMaintenanceID);

            var result = cmd.ExecuteScalar();
            if (result is string path)
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                if (File.Exists(fullPath))
                    DamagePhoto.Source = new BitmapImage(new Uri(fullPath));
            }
        }

        // ===========================
        // Delete maintenance record
        // ===========================
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this maintenance record?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                using var conn = new SqliteConnection(connectionString);
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    DELETE FROM Maintenance
                    WHERE MaintenanceID = $mid;
                ";
                cmd.Parameters.AddWithValue("$mid", currentMaintenanceID);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Maintenance record deleted!");
                this.Close();
            }
        }

        // ===========================
        // Helper method for formatting dates
        // ===========================
        private string FormatDate(string? dbDate)
        {
            if (string.IsNullOrEmpty(dbDate))
                return "";

            if (DateTime.TryParse(dbDate, out DateTime parsed))
                return parsed.ToString("MMM-dd-yyyy"); // Month as short name (Apr, May, etc.)

            return dbDate;
        }
    }
}