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

                if (!reader.IsDBNull(2))
                    PartNeededBox.Text = reader.GetString(2);
            }

            LoadPhotoFromDatabase();
        }

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

            MessageBox.Show("Saved!");
            this.Close();
        }

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

                var updateCmd = conn.CreateCommand();
                updateCmd.CommandText = "UPDATE Bikes SET Status='Available' WHERE BikeID=$bid";
                updateCmd.Parameters.AddWithValue("$bid", bikeID);
                updateCmd.ExecuteNonQuery();

                // update maintenance record to set DateFixed
                var fixCmd = conn.CreateCommand();
                fixCmd.CommandText = @"
                UPDATE Maintenance
                SET DateFixed = $dateFixed
                WHERE MaintenanceID = $mid;
";
                fixCmd.Parameters.AddWithValue("$dateFixed", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                fixCmd.Parameters.AddWithValue("$mid", currentMaintenanceID);

                fixCmd.ExecuteNonQuery();

                MessageBox.Show("Bike marked as Available!");
                this.Close();
            }
        }

        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (dialog.ShowDialog() == true)
            {
                string sourcePath = dialog.FileName;
                string fileName = "bike_" + DateTime.Now.Ticks + Path.GetExtension(sourcePath);

                string destinationFolder = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Images",
                    "Maintenance"
                );

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
    }
}