using Microsoft.Data.Sqlite;
using System;
using System.Windows;

namespace FindlayBikeShop
{
    public partial class EditRentalHistory : Window
    {
        private readonly string connectionString = "Data Source=BikeDatabase.db";
        private readonly RentalRecord currentRental;
        private readonly bool isNewRental = false;
        private readonly int bikeId;

        // edit existing rental / return flow
        public EditRentalHistory(RentalRecord rental)
        {
            InitializeComponent();

            currentRental = rental;
            bikeId = rental.BikeID;

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

        // create brand new rental from a bike
        public EditRentalHistory(Bike bike)
        {
            InitializeComponent();

            isNewRental = true;
            bikeId = bike.BikeID;
            currentRental = new RentalRecord { BikeID = bike.BikeID };

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

                if (isNewRental)
                {
                    string insertSql = @"
                        INSERT INTO Rentals
                        (BikeID, StudentID, SemesterRented, Year, CheckoutDate, DueDate, ReturnDate, CheckinDate1, CheckinDate2, CheckinDate3)
                        VALUES
                        (@bikeId, @studentId, @semester, @year, @checkoutDate, @dueDate, @returnDate, @checkin1, @checkin2, @checkin3);";

                    using (var cmd = new SqliteCommand(insertSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@bikeId", bikeId);
                        cmd.Parameters.AddWithValue("@studentId", string.IsNullOrWhiteSpace(StudentIDBox.Text) ? DBNull.Value : StudentIDBox.Text);
                        cmd.Parameters.AddWithValue("@semester", string.IsNullOrWhiteSpace(SemesterBox.Text) ? DBNull.Value : SemesterBox.Text);
                        cmd.Parameters.AddWithValue("@year", year);
                        cmd.Parameters.AddWithValue("@checkoutDate", string.IsNullOrWhiteSpace(CheckoutDateBox.Text) ? DBNull.Value : CheckoutDateBox.Text);
                        cmd.Parameters.AddWithValue("@dueDate", string.IsNullOrWhiteSpace(DueDateBox.Text) ? DBNull.Value : DueDateBox.Text);
                        cmd.Parameters.AddWithValue("@returnDate", string.IsNullOrWhiteSpace(ReturnDateBox.Text) ? DBNull.Value : ReturnDateBox.Text);
                        cmd.Parameters.AddWithValue("@checkin1", string.IsNullOrWhiteSpace(CheckinDate1Box.Text) ? DBNull.Value : CheckinDate1Box.Text);
                        cmd.Parameters.AddWithValue("@checkin2", string.IsNullOrWhiteSpace(CheckinDate2Box.Text) ? DBNull.Value : CheckinDate2Box.Text);
                        cmd.Parameters.AddWithValue("@checkin3", string.IsNullOrWhiteSpace(CheckinDate3Box.Text) ? DBNull.Value : CheckinDate3Box.Text);

                        cmd.ExecuteNonQuery();
                    }

                    // mark bike as rented
                    using (var bikeCmd = new SqliteCommand(@"
                        UPDATE Bikes
                        SET Status = 'Rented',
                            LastUpdated = CURRENT_DATE
                        WHERE BikeID = @bikeId;", connection))
                    {
                        bikeCmd.Parameters.AddWithValue("@bikeId", bikeId);
                        bikeCmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    string updateSql = @"
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

                    using (var cmd = new SqliteCommand(updateSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@studentId", string.IsNullOrWhiteSpace(StudentIDBox.Text) ? DBNull.Value : StudentIDBox.Text);
                        cmd.Parameters.AddWithValue("@semester", string.IsNullOrWhiteSpace(SemesterBox.Text) ? DBNull.Value : SemesterBox.Text);
                        cmd.Parameters.AddWithValue("@year", year);
                        cmd.Parameters.AddWithValue("@checkoutDate", string.IsNullOrWhiteSpace(CheckoutDateBox.Text) ? DBNull.Value : CheckoutDateBox.Text);
                        cmd.Parameters.AddWithValue("@dueDate", string.IsNullOrWhiteSpace(DueDateBox.Text) ? DBNull.Value : DueDateBox.Text);
                        cmd.Parameters.AddWithValue("@returnDate", string.IsNullOrWhiteSpace(ReturnDateBox.Text) ? DBNull.Value : ReturnDateBox.Text);
                        cmd.Parameters.AddWithValue("@checkin1", string.IsNullOrWhiteSpace(CheckinDate1Box.Text) ? DBNull.Value : CheckinDate1Box.Text);
                        cmd.Parameters.AddWithValue("@checkin2", string.IsNullOrWhiteSpace(CheckinDate2Box.Text) ? DBNull.Value : CheckinDate2Box.Text);
                        cmd.Parameters.AddWithValue("@checkin3", string.IsNullOrWhiteSpace(CheckinDate3Box.Text) ? DBNull.Value : CheckinDate3Box.Text);
                        cmd.Parameters.AddWithValue("@rentalId", currentRental.RentalID);

                        cmd.ExecuteNonQuery();
                    }

                    // if returned, mark bike available again
                    if (!string.IsNullOrWhiteSpace(ReturnDateBox.Text))
                    {
                        using (var bikeCmd = new SqliteCommand(@"
                            UPDATE Bikes
                            SET Status = 'Available',
                                LastUpdated = CURRENT_DATE
                            WHERE BikeID = @bikeId;", connection))
                        {
                            bikeCmd.Parameters.AddWithValue("@bikeId", bikeId);
                            bikeCmd.ExecuteNonQuery();
                        }
                    }
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