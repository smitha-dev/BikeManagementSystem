using Microsoft.Data.Sqlite;
using System.Windows;
using System.Windows.Controls;

namespace FindlayBikeShop
{
    public partial class MainWindow : Window
    {
        private string connectionString = "Data Source=BikeDatabase.db";

        public MainWindow()
        {
            InitializeComponent();

            // Load all bike lists on startup
            LoadMaintenanceBikes();
            LoadAvailableBikes();
            LoadRentedBikes();
        }

        // ==============================
        // Maintenance Bikes
        // ==============================
        private void LoadMaintenanceBikes()
        {
            var bikes = new List<Bike>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string sql = @"
            SELECT b.BikeID, b.Brand, b.Size, b.MinHeight, b.MaxHeight, 
                   b.Color, b.Status, b.LastUpdated, m.Notes
            FROM Bikes b
            LEFT JOIN Maintenance m ON b.BikeID = m.BikeID
            WHERE b.Status = 'Maintenance'
              AND m.MaintenanceID = (
                  SELECT MAX(MaintenanceID)
                  FROM Maintenance
                  WHERE BikeID = b.BikeID
              )
            ORDER BY b.BikeID;";

                using (var cmd = new SqliteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bikes.Add(new Bike
                        {
                            BikeID = reader.GetInt32(0),
                            Brand = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Size = reader.IsDBNull(2) ? null : reader.GetString(2),
                            MinHeight = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                            MaxHeight = reader.IsDBNull(4) ? null : reader.GetDouble(4),
                            Color = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Status = reader.IsDBNull(6) ? null : reader.GetString(6),
                            LastUpdated = reader.IsDBNull(7) ? null : FormatDate(reader.GetString(7)),
                            Notes = reader.IsDBNull(8) ? null : reader.GetString(8)
                        });
                    }
                }
            }

            BikeNeedToRepair.ItemsSource = bikes;
        }

        // ==============================
        // Available bikes
        // ==============================
        private void LoadAvailableBikes()
        {
            var bikes = new List<Bike>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string sql = @"
            SELECT BikeID, Brand, Size, MinHeight, MaxHeight, Color, Status, LastUpdated
            FROM Bikes
            WHERE Status = 'Available'
            ORDER BY BikeID;";

                using (var cmd = new SqliteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bikes.Add(new Bike
                        {
                            BikeID = reader.GetInt32(0),
                            Brand = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Size = reader.IsDBNull(2) ? null : reader.GetString(2),
                            MinHeight = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                            MaxHeight = reader.IsDBNull(4) ? null : reader.GetDouble(4),
                            Color = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Status = reader.IsDBNull(6) ? null : reader.GetString(6),
                            LastUpdated = reader.IsDBNull(7) ? null : reader.GetString(7)
                        });
                    }
                }
            }

            AvailableBikeList.ItemsSource = bikes;
        }

        // ==============================
        // Unavailable bikes
        // ==============================
        private void LoadRentedBikes()
        {
            var bikes = new List<Bike>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string sql = @"
            SELECT BikeID, Brand, Size, MinHeight, MaxHeight, Color, Status, LastUpdated
            FROM Bikes 
            WHERE Status = 'Rented'
            ORDER BY BikeID;";

                using (var cmd = new SqliteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bikes.Add(new Bike
                        {
                            BikeID = reader.GetInt32(0),
                            Brand = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Size = reader.IsDBNull(2) ? null : reader.GetString(2),
                            MinHeight = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                            MaxHeight = reader.IsDBNull(4) ? null : reader.GetDouble(4),
                            Color = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Status = reader.IsDBNull(6) ? null : reader.GetString(6),
                            LastUpdated = reader.IsDBNull(7) ? null : reader.GetString(7)
                        });
                    }
                }
            }

            RentedBikeList.ItemsSource = bikes;
        }

        // ==============================
        // Navigation buttons
        // ==============================
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

        private void BikeNeedToRepair_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BikeNeedToRepair.SelectedItem is Bike selectedBike)
            {
                var detailsWindow = new BikeDetails(selectedBike);
                detailsWindow.Show();
                this.Close();
            }
        }

        private void AvailableBikeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AvailableBikeList.SelectedItem is Bike selectedBike)
            {
                var detailsWindow = new BikeDetails(selectedBike);
                detailsWindow.Show();
                this.Close();
            }
        }

        private void RentedBikeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RentedBikeList.SelectedItem is Bike selectedBike)
            {
                var detailsWindow = new BikeDetails(selectedBike);
                detailsWindow.Show();
                this.Close();
            }

        }

          private string FormatDate(string? dbDate)
          {
              if (string.IsNullOrEmpty(dbDate))
                  return "";
        
              if (DateTime.TryParse(dbDate, out DateTime parsed))
                  return parsed.ToString("MMM-dd-yyyy"); // Month as short name (Apr, May, etc.)
        
              return dbDate;
          }
           private void Backup_Click(object sender, RoutedEventArgs e)
 {
     BackupHelper.BackupBikeData();
 }

 private void Restore_Click(object sender, RoutedEventArgs e)
 {
     var result = MessageBox.Show(
         "Restoring will overwrite your current database and images. Continue?",
         "Confirm Restore",
         MessageBoxButton.YesNo,
         MessageBoxImage.Warning);

     if (result != MessageBoxResult.Yes) return;

     try
     {
         // 1. Clear images (release image locks)
         UIHelper.ClearAllImages(this);

         // 2. Release SQLite locks 
         SqliteConnection.ClearAllPools();
         GC.Collect();
         GC.WaitForPendingFinalizers();

         // 3. Select backup
         var dialog = new Microsoft.Win32.OpenFileDialog
         {
             Filter = "Zip Files (*.zip)|*.zip",
             Title = "Select a backup to restore"
         };

         if (dialog.ShowDialog() != true) return;

         string backupPath = dialog.FileName;
         string baseDir = AppDomain.CurrentDomain.BaseDirectory;
         string dbPath = Path.Combine(baseDir, "BikeDatabase.db");
         string imagesPath = Path.Combine(baseDir, "Images");

         // 4. Extract zip to temp folder
         string tempFolder = Path.Combine(Path.GetTempPath(), "BikeRestore_" + DateTime.Now.Ticks);
         Directory.CreateDirectory(tempFolder);
         System.IO.Compression.ZipFile.ExtractToDirectory(backupPath, tempFolder);

         // 5. Release locks AGAIN (extra safe before delete)
         SqliteConnection.ClearAllPools();
         GC.Collect();
         GC.WaitForPendingFinalizers();

         // 6. Delete current database
         if (File.Exists(dbPath))
             File.Delete(dbPath);

         // 7. Copy new database
         string backupDb = Path.Combine(tempFolder, "BikeDatabase.db");
         if (File.Exists(backupDb))
             File.Copy(backupDb, dbPath);

         // 8. Restore images
         string backupImages = Path.Combine(tempFolder, "Images");
         if (Directory.Exists(backupImages))
         {
             if (Directory.Exists(imagesPath))
                 Directory.Delete(imagesPath, true);

             BackupHelper.CopyDirectory(backupImages, imagesPath);
         }

         // 9. Clean up
         Directory.Delete(tempFolder, true);

         MessageBox.Show("Restore completed successfully!");

         // 10. Refresh UI
         var newWindow = new Inventory();
         newWindow.Show();
         this.Close();
     }
     catch (Exception ex)
     {
         MessageBox.Show($"Restore failed: {ex.Message}");
     }
 }
    }
}
