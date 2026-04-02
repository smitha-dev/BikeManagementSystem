using Microsoft.Data.Sqlite;
using System;
using System.Windows;

namespace FindlayBikeShop
{
    public partial class EditRentalHistory : Window
    {
        private readonly string connectionString = "Data Source=BikeDatabase.db";

        private readonly bool isEditMode;
        private readonly RentalRecord? currentRental;
        private readonly Bike? currentBike;

        // edit existing rental
        public EditRentalHistory(RentalRecord rental)
        {
            InitializeComponent();

            isEditMode = true;
            currentRental = rental;

            StudentIDBox.Text = rental.StudentID ?? "";
            SemesterBox.Text = rental.SemesterRented ?? "";
            YearBox.Text = rental.Year == 0 ? "" : rental.Year.ToString();
            CheckoutDateBox.Text = rental.CheckoutDate ?? "";
            DueDateBox.Text = rental.DueDate ?? "";
            ReturnDateBox.Text = rental.ReturnDate ?? "";
            CheckinDate1Box.Text = rental.CheckinDate1 ?? "";
            CheckinDate2Box.Text = rental.CheckinDate2 ?? "";
            CheckinDate3Box.Text = rental.CheckinDate3 ?? "";
        }

        // add new rental
        public EditRentalHistory(Bike bike)
        {
            InitializeComponent();

            isEditMode = false;
            currentBike = bike;

            // optional defaults
            YearBox.Text = DateTime.Now.Year.ToString();
            CheckoutDateBox.Text = DateTime.Now.ToString("yyyy-MM-dd");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(YearBox.Text, out int year))
            {
                MessageBox.Show("Year must be a valid number.");
                return;
            }

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                if (isEditMode)
                {
                    string sql = @"
        UPDATE Rentals
        SET StudentID = @studentId,
            SemesterRented = @semester,
            Year = @year,
            CheckoutDate = @checkoutDate,
            DueDate = @dueDate,
            ReturnDate = @returnDate,
            CheckinDate1 = @checkin1,
            CheckinDate2 = @checkin2,
            CheckinDate3 = @checkin3
        WHERE RentalID = @rentalId;";

                    using (var cmd = new SqliteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@studentId", string.IsNullOrWhiteSpace(StudentIDBox.Text) ? DBNull.Value : StudentIDBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@semester", string.IsNullOrWhiteSpace(SemesterBox.Text) ? DBNull.Value : SemesterBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@year", year);
                        cmd.Parameters.AddWithValue("@checkoutDate", string.IsNullOrWhiteSpace(CheckoutDateBox.Text) ? DBNull.Value : CheckoutDateBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@dueDate", string.IsNullOrWhiteSpace(DueDateBox.Text) ? DBNull.Value : DueDateBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@returnDate", string.IsNullOrWhiteSpace(ReturnDateBox.Text) ? DBNull.Value : ReturnDateBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@checkin1", string.IsNullOrWhiteSpace(CheckinDate1Box.Text) ? DBNull.Value : CheckinDate1Box.Text.Trim());
                        cmd.Parameters.AddWithValue("@checkin2", string.IsNullOrWhiteSpace(CheckinDate2Box.Text) ? DBNull.Value : CheckinDate2Box.Text.Trim());
                        cmd.Parameters.AddWithValue("@checkin3", string.IsNullOrWhiteSpace(CheckinDate3Box.Text) ? DBNull.Value : CheckinDate3Box.Text.Trim());
                        cmd.Parameters.AddWithValue("@rentalId", currentRental!.RentalID);

                        cmd.ExecuteNonQuery();
                    }

                    string bikeStatus = string.IsNullOrWhiteSpace(ReturnDateBox.Text) ? "Rented" : "Available";

                    string bikeSql = @"
        UPDATE Bikes
        SET Status = @status,
            LastUpdated = CURRENT_DATE
        WHERE BikeID = @bikeId;";

                    using (var bikeCmd = new SqliteCommand(bikeSql, connection))
                    {
                        bikeCmd.Parameters.AddWithValue("@status", bikeStatus);
                        bikeCmd.Parameters.AddWithValue("@bikeId", currentRental!.BikeID);
                        bikeCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Rental updated!");
                }
                else
                {
                    string sql = @"
        INSERT INTO Rentals
        (BikeID, StudentID, SemesterRented, Year, CheckoutDate, DueDate, ReturnDate, CheckinDate1, CheckinDate2, CheckinDate3)
        VALUES
        (@bikeId, @studentId, @semester, @year, @checkoutDate, @dueDate, @returnDate, @checkin1, @checkin2, @checkin3);";

                    using (var cmd = new SqliteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@bikeId", currentBike!.BikeID);
                        cmd.Parameters.AddWithValue("@studentId", string.IsNullOrWhiteSpace(StudentIDBox.Text) ? DBNull.Value : StudentIDBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@semester", string.IsNullOrWhiteSpace(SemesterBox.Text) ? DBNull.Value : SemesterBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@year", year);
                        cmd.Parameters.AddWithValue("@checkoutDate", string.IsNullOrWhiteSpace(CheckoutDateBox.Text) ? DBNull.Value : CheckoutDateBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@dueDate", string.IsNullOrWhiteSpace(DueDateBox.Text) ? DBNull.Value : DueDateBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@returnDate", string.IsNullOrWhiteSpace(ReturnDateBox.Text) ? DBNull.Value : ReturnDateBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@checkin1", string.IsNullOrWhiteSpace(CheckinDate1Box.Text) ? DBNull.Value : CheckinDate1Box.Text.Trim());
                        cmd.Parameters.AddWithValue("@checkin2", string.IsNullOrWhiteSpace(CheckinDate2Box.Text) ? DBNull.Value : CheckinDate2Box.Text.Trim());
                        cmd.Parameters.AddWithValue("@checkin3", string.IsNullOrWhiteSpace(CheckinDate3Box.Text) ? DBNull.Value : CheckinDate3Box.Text.Trim());

                        cmd.ExecuteNonQuery();
                    }

                    string bikeSql = @"
        UPDATE Bikes
        SET Status = 'Rented',
            LastUpdated = CURRENT_DATE
        WHERE BikeID = @bikeId;";

                    using (var bikeCmd = new SqliteCommand(bikeSql, connection))
                    {
                        bikeCmd.Parameters.AddWithValue("@bikeId", currentBike!.BikeID);
                        bikeCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Rental added!");
                }
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}