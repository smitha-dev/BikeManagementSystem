using Microsoft.Data.Sqlite;
using Microsoft.Win32;
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


namespace FindlayBikeShop
{
    public partial class BikeDetails : Window
    {
        private Bike currentBike;
        private string connectionString = "Data Source=BikeDatabase.db";

        public BikeDetails(Bike bike)
        {
            InitializeComponent();
            this.DataContext = bike;
            currentBike = bike;

            LoadBikePhoto();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            var inventoryWindow = new Inventory();
            inventoryWindow.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var maintenanceWindow = new MaintenanceHistory(currentBike.BikeID);
            maintenanceWindow.Show();
        }

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
            using (var connection = new SqliteConnection(connectionString))
            {
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
        }

        private void LoadBikePhoto()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
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
}