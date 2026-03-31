
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;

namespace FindlayBikeShop
{
    public partial class MaintenanceHistory : Window
    {
        private int currentMaintenanceID = 1;

        public MaintenanceHistory()
        {
            InitializeComponent();
        }
        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (dialog.ShowDialog() == true)
            {
                string sourcePath = dialog.FileName;

                string fileName = "bike_" + DateTime.Now.Ticks + System.IO.Path.GetExtension(sourcePath);

                string destinationFolder = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Images",
                    "Maintenance"
                );

                if (!System.IO.Directory.Exists(destinationFolder))
                {
                    System.IO.Directory.CreateDirectory(destinationFolder);
                }

                string destinationPath = System.IO.Path.Combine(destinationFolder, fileName);

                System.IO.File.Copy(sourcePath, destinationPath, true);

                DamagePhoto.Source = new BitmapImage(new Uri(destinationPath));

                SavePhotoPathToDatabase("Images/Maintenance/" + fileName);
            }
        }
        private void SavePhotoPathToDatabase(string relativePath)
        {
            string connectionString = "Data Source=BikeDatabase.db";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = @"INSERT INTO Photos (MaintenanceID, FilePath, PhotoType)
                    VALUES ($id, $path, 'Maintenance')
                ";
                command.Parameters.AddWithValue("$path", relativePath);
                command.Parameters.AddWithValue("$id", currentMaintenanceID);

                int rows = command.ExecuteNonQuery();

                //                MessageBox.Show("Saved! Rows: " + rows);
            }
        }
        private void LoadPhoto(string relativePath)
        {
            string fullPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                relativePath
            );

            if (System.IO.File.Exists(fullPath))
            {
                DamagePhoto.Source = new BitmapImage(new Uri(fullPath));
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string connectionString = "Data Source=BikeDatabase.db";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = @"
                    SELECT FilePath
                    FROM Photos
                    WHERE MaintenanceID = $id
                    ORDER BY PhotoID DESC
                    LIMIT 1
                ";

                command.Parameters.AddWithValue("$id", currentMaintenanceID);

                var result = command.ExecuteScalar();

                if (result is string photoPath && !string.IsNullOrWhiteSpace(photoPath))
                {
                    LoadPhoto(photoPath);
                }
            }
        }



        private void FixedButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Data Source=BikeDatabase.db";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Step 1: Get BikeID from Maintenance table
                var getBikeCmd = connection.CreateCommand();
                getBikeCmd.CommandText = @" SELECT BikeID 
                                            FROM Maintenance 
                                            WHERE MaintenanceID = $mid";
                getBikeCmd.Parameters.AddWithValue("$mid", currentMaintenanceID);

                object result = getBikeCmd.ExecuteScalar();

                if (result != null)
                {
                    int bikeID = Convert.ToInt32(result);

                    // Step 2: Update bike status to 'Available'
                    var updateCmd = connection.CreateCommand();
                    updateCmd.CommandText = @"
                                                UPDATE Bikes
                                                SET Status = 'Available'
                                                WHERE BikeID = $bid";
                    updateCmd.Parameters.AddWithValue("$bid", bikeID);

                    updateCmd.ExecuteNonQuery();

                    MessageBox.Show("Bike marked as Available!");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Bike not found for this maintenance record.");
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string connectionString = "Data Source=BikeDatabase.db";

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    string note = NoteTextBox.Text;
                    double cost = 0;
                    double.TryParse(CostTextBox.Text, out cost);

                    var updateMaintenanceCmd = connection.CreateCommand();
                    updateMaintenanceCmd.CommandText = @"
                UPDATE Maintenance
                SET Notes = $note,
                    Cost = $cost
                WHERE MaintenanceID = $mid
            ";
                    updateMaintenanceCmd.Parameters.AddWithValue("$note", note);
                    updateMaintenanceCmd.Parameters.AddWithValue("$cost", cost);
                    updateMaintenanceCmd.Parameters.AddWithValue("$mid", currentMaintenanceID);

                    updateMaintenanceCmd.ExecuteNonQuery();

                    var getBikeCmd = connection.CreateCommand();
                    getBikeCmd.CommandText = @"
                SELECT BikeID
                FROM Maintenance
                WHERE MaintenanceID = $mid
            ";
                    getBikeCmd.Parameters.AddWithValue("$mid", currentMaintenanceID);

                    object result = getBikeCmd.ExecuteScalar();

                    if (result != null)
                    {
                        int bikeID = Convert.ToInt32(result);

                        var updateBikeCmd = connection.CreateCommand();
                        updateBikeCmd.CommandText = @"
                    UPDATE Bikes
                    SET Status = 'Maintenance'
                    WHERE BikeID = $bid
                ";
                        updateBikeCmd.Parameters.AddWithValue("$bid", bikeID);

                        updateBikeCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Saved!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }
    }
}
