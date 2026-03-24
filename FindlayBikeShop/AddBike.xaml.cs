using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FindlayBikeShop
{
    public partial class AddBike : Window
    {
        private string connectionString = "Data Source=BikeDatabase.db";

        public AddBike()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string brand = BrandBox.Text.Trim();
            string size = (SizeBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string color = ColorBox.Text.Trim();
            string status = (StatusBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            // form validation
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(brand))
                errors.Add("Brand is required.");

            if (SizeBox.SelectedItem == null)
                errors.Add("Size is required.");

            if (!double.TryParse(SeatBox.Text, out double seatheight))
                errors.Add("Seat height must be a valid number.");
            
            if (string.IsNullOrWhiteSpace(color))
                errors.Add("Color is required.");

            if (StatusBox.SelectedItem == null)
                errors.Add("Status is required.");

            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join("\n", errors));
                return;
            }

            // insert into database
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string sql = @"INSERT INTO Bikes 
                       (Brand, Size, SeatHeight, Color, Status, DateAdded, LastUpdated)
                       VALUES 
                       (@brand, @size, @seatheight, @color, @status, CURRENT_DATE, CURRENT_DATE)";

                using (var cmd = new SqliteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@brand", brand);
                    cmd.Parameters.AddWithValue("@size", size);
                    cmd.Parameters.AddWithValue("@seatheight", seatheight);
                    cmd.Parameters.AddWithValue("@color", color);
                    cmd.Parameters.AddWithValue("@status", status);

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Bike added!");
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}