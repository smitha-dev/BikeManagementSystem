using Microsoft.Data.Sqlite;
using System.Windows;

namespace FindlayBikeShop
{
    public partial class EditRentalHistory : Window
    {
        private readonly string connectionString = "Data Source=BikeDatabase.db";
        private readonly RentalRecord currentRental;

        public EditRentalHistory(RentalRecord rental)
        {
            InitializeComponent();

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