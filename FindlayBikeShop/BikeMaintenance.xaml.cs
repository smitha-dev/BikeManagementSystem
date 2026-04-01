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
        private int currentBikeID;
        private string connectionString = "Data Source=BikeDatabase.db";

        public MaintenanceHistory(int bikeID)
        {
            InitializeComponent();
            currentBikeID = bikeID;
        }

        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
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
                    "Maintenance"
                );

                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                string destinationPath = System.IO.Path.Combine(folder, fileName);

                System.IO.File.Copy(sourcePath, destinationPath, true);

                DamagePhoto.Source = new BitmapImage(new Uri(destinationPath));

                SavePhotoPathToDatabase("Images/Maintenance/" + fileName);
            }
        }

        private void SavePhotoPathToDatabase(string relativePath)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = @"
                    INSERT INTO Photos (BikeID, FilePath, PhotoType)
                    VALUES ($bikeID, $path, 'Maintenance')
                ";

                command.Parameters.AddWithValue("$bikeID", currentBikeID);
                command.Parameters.AddWithValue("$path", relativePath);

                command.ExecuteNonQuery();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string notes = DescriptionBox.Text;
            string costText = CostBox.Text;
            string partNeededText = PartNeededBox.Text;

            if (string.IsNullOrWhiteSpace(notes) || string.IsNullOrWhiteSpace(costText) || string.IsNullOrWhiteSpace(partNeededText))
            {
                MessageBox.Show("Please fill all fields");
                return;
            }

            if (!double.TryParse(costText, out double cost))
            {
                MessageBox.Show("Invalid cost");
                return;
            }

            SaveMaintenanceData(notes, cost, partNeededText);
        }

        private void SaveMaintenanceData(string notes, double cost, string partNeeded)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = @"
                    INSERT INTO Maintenance (BikeID, Notes, Cost, PartNeeded, DateFlagged)
                    VALUES ($bikeID, $notes, $cost, $partNeeded, $date)
                ";

                command.Parameters.AddWithValue("$bikeID", currentBikeID);
                command.Parameters.AddWithValue("$notes", notes);
                command.Parameters.AddWithValue("$cost", cost);
                command.Parameters.AddWithValue("$partNeeded", partNeeded);
                command.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

                command.ExecuteNonQuery();

            }

            MessageBox.Show("Saved successfully!");
        }

    }
}