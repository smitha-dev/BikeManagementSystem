using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FindlayBikeShop
{
    public partial class AddBike : Window
    {
        private string connectionString = "Data Source=BikeDatabase.db";

        private bool isEditMode = false;
        private Bike currentBike;

        // constructor for adding a new bike
        public AddBike()
        {
            InitializeComponent();
            this.Title = "Add New Bike";
            BikeHeader.Text = "Add New Bike";
        }

        // constructor for edit mode
        public AddBike(Bike bikeToEdit)
        {
            InitializeComponent();

            isEditMode = true;
            currentBike = bikeToEdit;

            this.Title = "Edit Bike";
            BikeHeader.Text = "Edit Bike";

            LoadBikeData();
        }

        private void LoadBikeData()
        {
            if (currentBike == null)
                return;

            BrandBox.Text = currentBike.Brand;
            SeatBox.Text = currentBike.SeatHeight.ToString();
            ColorBox.Text = currentBike.Color;

            SelectComboBoxItem(SizeBox, currentBike.Size);
            SelectComboBoxItem(StatusBox, currentBike.Status);
        }

        private void SelectComboBoxItem(ComboBox comboBox, string valueToMatch)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == valueToMatch)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string brand = BrandBox.Text.Trim();
            string size = (SizeBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string color = ColorBox.Text.Trim();
            string status = (StatusBox.SelectedItem as ComboBoxItem)?.Content.ToString();

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

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                if (isEditMode)
                {
                    string updateSql = @"
                        UPDATE Bikes
                        SET Brand = @brand,
                            Size = @size,
                            SeatHeight = @seatheight,
                            Color = @color,
                            Status = @status,
                            LastUpdated = CURRENT_DATE
                        WHERE BikeID = @bikeId";

                    using (var cmd = new SqliteCommand(updateSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@brand", brand);
                        cmd.Parameters.AddWithValue("@size", size);
                        cmd.Parameters.AddWithValue("@seatheight", seatheight);
                        cmd.Parameters.AddWithValue("@color", color);
                        cmd.Parameters.AddWithValue("@status", status);
                        cmd.Parameters.AddWithValue("@bikeId", currentBike.BikeID);

                        cmd.ExecuteNonQuery();
                    }

                    currentBike.Brand = brand;
                    currentBike.Size = size;
                    currentBike.SeatHeight = seatheight;
                    currentBike.Color = color;
                    currentBike.Status = status;
                    currentBike.LastUpdated = DateTime.Now.ToShortDateString();

                    MessageBox.Show("Bike updated!");
                }
                else
                {
                    string insertSql = @"
                        INSERT INTO Bikes 
                        (Brand, Size, SeatHeight, Color, Status, DateAdded, LastUpdated)
                        VALUES 
                        (@brand, @size, @seatheight, @color, @status, CURRENT_DATE, CURRENT_DATE)";

                    using (var cmd = new SqliteCommand(insertSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@brand", brand);
                        cmd.Parameters.AddWithValue("@size", size);
                        cmd.Parameters.AddWithValue("@seatheight", seatheight);
                        cmd.Parameters.AddWithValue("@color", color);
                        cmd.Parameters.AddWithValue("@status", status);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Bike added!");
                }
            }

            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}