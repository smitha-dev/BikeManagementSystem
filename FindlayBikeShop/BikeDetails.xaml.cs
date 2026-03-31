using Microsoft.Data.Sqlite;
using System.Windows;
using System.Windows.Controls;


namespace FindlayBikeShop
{
    public partial class BikeDetails : Window
    {

        private string connectionString = "Data Source=BikeDatabase.db";

        // store the selected bike that is being displayed (used for passing to edit window)
        private Bike currentBike;

        public BikeDetails(Bike bike)
        {
            InitializeComponent();

            currentBike = bike;
            this.DataContext = bike;

            LoadRentalHistory();
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
        int maintenanceID = GetLatestMaintenanceID(currentBike.BikeID);

        var maintenanceWindow = new MaintenanceHistory(maintenanceID);
        maintenanceWindow.Show();
    }

    private int GetLatestMaintenanceID(int bikeID)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
        SELECT MaintenanceID
        FROM Maintenance
        WHERE BikeID = $bikeId
        ORDER BY MaintenanceID DESC
        LIMIT 1
    ";
            cmd.Parameters.AddWithValue("$bikeId", bikeID);

            object result = cmd.ExecuteScalar();

            if (result != null)
                return Convert.ToInt32(result);
        }

        // If no maintenance record exists → create one
        return CreateNewMaintenanceRecord(bikeID);
    }

    private int CreateNewMaintenanceRecord(int bikeID)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
        INSERT INTO Maintenance (BikeID)
        VALUES ($bikeId);
        SELECT last_insert_rowid();
    ";
            cmd.Parameters.AddWithValue("$bikeId", bikeID);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }


        private void EditDetails_Click(object sender, RoutedEventArgs e)
        {
            // open the addBike window, but pass current bike in case of editing an existing bike instead of adding a new one
            var editWindow = new AddBike(currentBike);
            editWindow.ShowDialog();

            // refresh the bike page after editing details to display edited information
            var refreshedWindow = new BikeDetails(currentBike);
            refreshedWindow.Show();
            this.Close();
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
            {
                LoadRentalHistory();
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void LoadRentalHistory()
        {
            var rentals = new List<RentalRecord>();

            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                string sql = @"
            SELECT RentalID, BikeID, StudentID, SemesterRented, Year,
                   CheckoutDate, DueDate, ReturnDate,
                   CheckinDate1, CheckinDate2, CheckinDate3
            FROM Rentals
            WHERE BikeID = @bikeId
            ORDER BY CheckoutDate DESC;";

                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@bikeId", currentBike.BikeID);

                    using (var reader = cmd.ExecuteReader())
                    {
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
                    }
                }
            }
            RentalHistoryGrid.ItemsSource = rentals;
        }

        private RentalRecord? GetSelectedRental()
        {
            return RentalHistoryGrid.SelectedItem as RentalRecord;
        }
    }
}
