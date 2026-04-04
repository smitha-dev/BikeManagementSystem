using Microsoft.Data.Sqlite;
using System.Windows;
using System.Windows.Controls;

namespace FindlayBikeShop
{
    public partial class AddBike : Window
    {
        private string connectionString = "Data Source=BikeDatabase.db";

        // checks to see if the form is adding a bike or editing an existing bike
        // false by default
        private bool isEditMode = false;

        // hold bike being edited if in edit mode
        // if not in edit mode, will create a new bike when saved
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

            // "enable" edit mode, store the bike being edited, and fill the form in with the existing data
            isEditMode = true;
            currentBike = bikeToEdit;

            this.Title = "Edit Bike";
            BikeHeader.Text = "Edit Bike";

            LoadBikeData();
        }

        // function to fill the form with existing data
        private void LoadBikeData()
        {
            // don't do anything if no bike was passed to the function (fallback)
            if (currentBike == null)
                return;

            BrandBox.Text = currentBike.Brand;
            MaxHeightBox.Text = currentBike.MaxHeight?.ToString() ?? "";
            MinHeightBox.Text = currentBike.MinHeight?.ToString() ?? "";
            ColorBox.Text = currentBike.Color;

            SelectComboBoxItem(SizeBox, currentBike.Size);
            SelectComboBoxItem(StatusBox, currentBike.Status);
        }

        // helper function for the combo boxes since they require special handling to select the correct item based on the value from the database
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

        // function to save the results of the form to the database
        private void Save_Click(object sender, RoutedEventArgs e)
        {
           
            string brand= BrandBox.Text.Trim();
            string size = (SizeBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string minHeightText = MinHeightBox.Text.Trim();
            string maxHeightText = MaxHeightBox.Text.Trim();
            string color = ColorBox.Text.Trim();
            string status = (StatusBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            // form validation
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(brand))
                errors.Add("Brand is required.");

            if (SizeBox.SelectedItem == null)
                errors.Add("Size is required.");

            if (!double.TryParse(minHeightText, out double minHeight))
                errors.Add("Min Height must be a valid number.");

            if (!double.TryParse(maxHeightText, out double maxHeight))
                errors.Add("Max Height must be a valid number.");
            
            // check logical range
            if (errors.Count == 0 && minHeight > maxHeight)
                errors.Add("Min Height cannot be greater than Max Height.");

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

                // ===========================
                // edit bike or add a new bike
                // core logic
                // ===========================

                // if in edit mode
                if (isEditMode)
                {

                    // use "update" to edit existing bike instead of "insert"
                    string updateSql = @"
                        UPDATE Bikes
                        SET Brand = @brand,
                            Size = @size,
                            MaxHeight = @maxheight,
                            MinHeight = @minheight,
                            Color = @color,
                            Status = @status,
                            LastUpdated = CURRENT_DATE
                        WHERE BikeID = @bikeId";

                    using (var cmd = new SqliteCommand(updateSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@brand", brand);
                        cmd.Parameters.AddWithValue("@size", size);
                        cmd.Parameters.AddWithValue("@maxheight", maxHeight);
                        cmd.Parameters.AddWithValue("@minheight", minHeight);
                        cmd.Parameters.AddWithValue("@color", color);
                        cmd.Parameters.AddWithValue("@status", status);

                        // tells DB which bike to update
                        cmd.Parameters.AddWithValue("@bikeId", currentBike.BikeID);

                        cmd.ExecuteNonQuery();
                    }

                    currentBike.Brand = brand;
                    currentBike.Size = size;
                    currentBike.MaxHeight = maxHeight;
                    currentBike.MinHeight = minHeight;
                    currentBike.Color = color;
                    currentBike.Status = status;
                    currentBike.LastUpdated = DateTime.Now.ToShortDateString();

                    MessageBox.Show("Bike updated!");
                }
                else

                // if in add mode
                {
                    string insertSql = @"
                        INSERT INTO Bikes 
                        (Brand, Size,MaxHeight, MinHeight, Color, Status, DateAdded, LastUpdated)
                        VALUES 
                        (@brand, @size, @maxheight, @minheight, @color, @status, CURRENT_DATE, CURRENT_DATE)";

                    using (var cmd = new SqliteCommand(insertSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@brand", brand);
                        cmd.Parameters.AddWithValue("@size", size);
                        cmd.Parameters.AddWithValue("@maxheight", maxHeight);
                        cmd.Parameters.AddWithValue("@minheight", minHeight);
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