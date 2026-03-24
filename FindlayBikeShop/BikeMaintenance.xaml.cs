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
    }
}