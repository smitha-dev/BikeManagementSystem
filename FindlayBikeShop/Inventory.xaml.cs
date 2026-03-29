using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
<<<<<<< Updated upstream
using System.Collections.Generic;
using System;
using System.Collections.Generic;
=======

using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;
using System.ComponentModel;





>>>>>>> Stashed changes

namespace FindlayBikeShop
{
    /// <summary>
    /// Interaction logic for Inventory.xaml
    /// </summary>
    public partial class Inventory : Window
    {
<<<<<<< Updated upstream
        private string connectionString = "Data Source=BikeDatabase.db";

    public Inventory()
        {
            InitializeComponent();
            LoadAllBikes();
=======


        private string connectionString = "Data Source=BikeDatabase.db";

        private ICollectionView? bikesView;

        public Inventory()
        {
            InitializeComponent();
            LoadAllBikes();
           


>>>>>>> Stashed changes
        }

        private void LoadAllBikes()
        {
            var bikes = new List<Bike>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string sql = @"SELECT b.BikeID, b.Brand, b.Size, b.Color, b.Status, b.LastUpdated,
                      p.FilePath
               FROM Bikes b
               LEFT JOIN Photos p ON b.BikeID = p.BikeID
               GROUP BY b.BikeID
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
                            Color = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Status = reader.IsDBNull(4) ? null : reader.GetString(4),
<<<<<<< Updated upstream
                            LastUpdated = reader.IsDBNull(5)
                                ? null
                                : reader.GetDateTime(5).ToString("yyyy-MM-dd"),
                            Photo = reader.IsDBNull(6) ? null : reader.GetString(6)
                    });
=======
                            LastUpdated = reader.IsDBNull(5) ? null: reader.GetDateTime(5).ToString("yyyy-MM-dd"),
                                
                            
                        });
>>>>>>> Stashed changes
                    }
                }
            }

            BikesListView.ItemsSource = bikes;

            bikesView = CollectionViewSource.GetDefaultView(BikesListView.ItemsSource);
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            // Do nothing since already here
        }

        private void AddBike_Click(object sender, RoutedEventArgs e)
        {
            var addBikeWindow = new AddBike();
<<<<<<< Updated upstream
            addBikeWindow.ShowDialog();

            // Refresh list after adding a bike
            LoadAllBikes();
        }
    }

}
=======
            addBikeWindow.Show();
            
        }

    



        private void BikesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BikesListView.SelectedItem is Bike selectedBike)
            {
                var detailsWindow = new BikeDetails(selectedBike);
                detailsWindow.Show();
                this.Close();
            }
        }

        private void RentReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Bike bike)
            {
                string newStatus = bike.Status == "Rented" ? "Available" : "Rented";

                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                string sql = "UPDATE Bikes SET Status = @status WHERE BikeID = @id";
                using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@status", newStatus);
                cmd.Parameters.AddWithValue("@id", bike.BikeID);
                cmd.ExecuteNonQuery();

                bike.Status = newStatus;

                // Update button and ListView
                btn.Content = bike.Status == "Rented" ? "Return" : "Rent";
                BikesListView.Items.Refresh();
            }
        }



        private void FilterByStatus(string status)
        {
            if (bikesView == null) return;

            if (status == "All")
            {
                bikesView.Filter = null;
            }
            else
            {
                bikesView.Filter = obj =>
                {
                    if (obj is Bike bike)
                    {
                        return bike.Status == status;
                    }
                    return false;
                };
            }
        }

        
        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusFilterComboBox.SelectedItem is ComboBoxItem item)
            {
                string selectedStatus = item.Content.ToString() ?? "All";
                FilterByStatus(selectedStatus);
            }
        }
         public class StatusToIsEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ❌ disable if Maintenance
            return value?.ToString() != "Maintenance";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    }


    public class StatusToButtonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == "Rented" ? "Return" : "Rent";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToIsEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() != "Maintenance";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}



>>>>>>> Stashed changes
