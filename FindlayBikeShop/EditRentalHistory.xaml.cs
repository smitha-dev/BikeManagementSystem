using Microsoft.Data.Sqlite;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FindlayBikeShop
{
    public partial class EditRentalHistory : Window
    {
        private readonly string connectionString = "Data Source=BikeDatabase.db";
        private readonly RentalRecord currentRental;
        private readonly bool isNewRental = false;
        private readonly int bikeId;

        // ===========================
        // Constructors
        // ===========================

        // Opens the window in edit mode for an existing rental record
        public EditRentalHistory(RentalRecord rental)
        {
            InitializeComponent();

            currentRental = rental;
            bikeId = rental.BikeID;
            LoadRentalIntoForm(rental);
        }

        // Opens the window in add mode for creating a new rental
        public EditRentalHistory(Bike bike)
        {
            InitializeComponent();

            isNewRental = true;
            bikeId = bike.BikeID;
            currentRental = new RentalRecord { BikeID = bike.BikeID };

            CheckoutDatePicker.SelectedDate = DateTime.Today;
        }

        // ===========================
        // Form setup
        // ===========================

        // Loads an existing rental record into the form controls
        private void LoadRentalIntoForm(RentalRecord rental)
        {
            StudentIDBox.Text = rental.StudentID ?? "";
            SetSemester(rental.SemesterRented);
            YearBox.Text = rental.Year == 0 ? "" : rental.Year.ToString();

            CheckoutDatePicker.SelectedDate = ParseDate(rental.CheckoutDate);
            DueDatePicker.SelectedDate = ParseDate(rental.DueDate);
            ReturnDatePicker.SelectedDate = ParseDate(rental.ReturnDate);
            CheckinDate1Picker.SelectedDate = ParseDate(rental.CheckinDate1);
            CheckinDate2Picker.SelectedDate = ParseDate(rental.CheckinDate2);
            CheckinDate3Picker.SelectedDate = ParseDate(rental.CheckinDate3);
        }

        // ===========================
        // Save / Cancel
        // ===========================

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm(out string errorMessage))
            {
                MessageBox.Show(errorMessage, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int year = int.Parse(YearBox.Text);
            string semester = GetSelectedSemester();
            string normalizedStudentId = StudentIDBox.Text.Trim().ToUpper();

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            if (isNewRental)
            {
                if (!CanRentBike(connection, bikeId, out string message))
                {
                    MessageBox.Show(message, "Action Blocked", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                InsertRental(connection, bikeId, normalizedStudentId, semester, year);
                UpdateBikeStatus(connection, bikeId, "Rented");
            }
            else
            {
                UpdateRental(connection, currentRental.RentalID, normalizedStudentId, semester, year);

                // If a return date exists, make the bike available again
                if (ReturnDatePicker.SelectedDate.HasValue)
                {
                    UpdateBikeStatus(connection, bikeId, "Available");
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

        // ===========================
        // Database write helpers
        // ===========================

        // Inserts a new rental record
        private void InsertRental(SqliteConnection connection, int bikeId, string studentId, string semester, int year)
        {
            string insertSql = @"
                INSERT INTO Rentals
                (BikeID, StudentID, SemesterRented, Year, CheckoutDate, DueDate, ReturnDate, CheckinDate1, CheckinDate2, CheckinDate3)
                VALUES
                (@bikeId, @studentId, @semester, @year, @checkoutDate, @dueDate, @returnDate, @checkin1, @checkin2, @checkin3);";

            using var cmd = new SqliteCommand(insertSql, connection);
            cmd.Parameters.AddWithValue("@bikeId", bikeId);
            AddRentalParameters(cmd, studentId, semester, year);
            cmd.ExecuteNonQuery();
        }

        // Updates an existing rental record
        private void UpdateRental(SqliteConnection connection, int rentalId, string studentId, string semester, int year)
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

            using var cmd = new SqliteCommand(updateSql, connection);
            AddRentalParameters(cmd, studentId, semester, year);
            cmd.Parameters.AddWithValue("@rentalId", rentalId);
            cmd.ExecuteNonQuery();
        }

        // Adds the shared rental form values to a SQL command
        private void AddRentalParameters(SqliteCommand cmd, string studentId, string semester, int year)
        {
            cmd.Parameters.AddWithValue("@studentId", ToDbValue(studentId));
            cmd.Parameters.AddWithValue("@semester", ToDbValue(semester));
            cmd.Parameters.AddWithValue("@year", year);
            cmd.Parameters.AddWithValue("@checkoutDate", ToDbValue(FormatDate(CheckoutDatePicker.SelectedDate)));
            cmd.Parameters.AddWithValue("@dueDate", ToDbValue(FormatDate(DueDatePicker.SelectedDate)));
            cmd.Parameters.AddWithValue("@returnDate", ToDbValue(FormatDate(ReturnDatePicker.SelectedDate)));
            cmd.Parameters.AddWithValue("@checkin1", ToDbValue(FormatDate(CheckinDate1Picker.SelectedDate)));
            cmd.Parameters.AddWithValue("@checkin2", ToDbValue(FormatDate(CheckinDate2Picker.SelectedDate)));
            cmd.Parameters.AddWithValue("@checkin3", ToDbValue(FormatDate(CheckinDate3Picker.SelectedDate)));
        }

        // Updates the bike's current status and refresh timestamp
        private void UpdateBikeStatus(SqliteConnection connection, int bikeId, string status)
        {
            using var cmd = new SqliteCommand(@"
                UPDATE Bikes
                SET Status = @status,
                    LastUpdated = CURRENT_DATE
                WHERE BikeID = @bikeId;", connection);

            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@bikeId", bikeId);
            cmd.ExecuteNonQuery();
        }

        // ===========================
        // Business rule helpers
        // ===========================

        // Returns the bike's current status from the database
        private string? GetBikeStatus(SqliteConnection connection, int bikeId)
        {
            using var cmd = new SqliteCommand(
                "SELECT Status FROM Bikes WHERE BikeID = @bikeId;",
                connection);

            cmd.Parameters.AddWithValue("@bikeId", bikeId);

            var result = cmd.ExecuteScalar();
            return result?.ToString();
        }

        // Checks whether the bike has any unresolved maintenance records
        private bool HasOpenMaintenance(SqliteConnection connection, int bikeId)
        {
            using var cmd = new SqliteCommand(@"
                SELECT COUNT(1)
                FROM Maintenance
                WHERE BikeID = @bikeId
                  AND DateFixed IS NULL;", connection);

            cmd.Parameters.AddWithValue("@bikeId", bikeId);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        // Prevents bikes from being rented when their state makes that invalid
        private bool CanRentBike(SqliteConnection connection, int bikeId, out string message)
        {
            string? bikeStatus = GetBikeStatus(connection, bikeId);
            bool hasOpenMaintenance = HasOpenMaintenance(connection, bikeId);

            if (string.Equals(bikeStatus, "Rented", StringComparison.OrdinalIgnoreCase))
            {
                message = "This bike is already rented and cannot be rented again until it is returned.";
                return false;
            }

            if (string.Equals(bikeStatus, "Maintenance", StringComparison.OrdinalIgnoreCase))
            {
                message = "This bike is currently in maintenance and cannot be rented.";
                return false;
            }

            if (string.Equals(bikeStatus, "Retired", StringComparison.OrdinalIgnoreCase))
            {
                message = "This bike is retired and cannot be rented.";
                return false;
            }

            if (hasOpenMaintenance)
            {
                message = "This bike has an open maintenance record and cannot be rented until maintenance is completed.";
                return false;
            }

            message = "";
            return true;
        }

        // ===========================
        // Form value helpers
        // ===========================

        // Gets the selected semester value from the combo box
        private string GetSelectedSemester()
        {
            return (SemesterBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
        }

        // Sets the semester combo box to match an existing value
        private void SetSemester(string semester)
        {
            if (string.IsNullOrWhiteSpace(semester))
            {
                SemesterBox.SelectedIndex = -1;
                return;
            }

            foreach (ComboBoxItem item in SemesterBox.Items)
            {
                if (string.Equals(item.Content?.ToString(), semester, StringComparison.OrdinalIgnoreCase))
                {
                    SemesterBox.SelectedItem = item;
                    return;
                }
            }

            SemesterBox.SelectedIndex = -1;
        }

        // Parses a date string from the database into a nullable DateTime
        private DateTime? ParseDate(string dateText)
        {
            if (string.IsNullOrWhiteSpace(dateText))
                return null;

            string[] formats =
            {
                "yyyy-MM-dd",
                "MM-dd-yyyy",
                "M-d-yyyy",
                "MM/dd/yyyy",
                "M/d/yyyy"
            };

            if (DateTime.TryParseExact(dateText, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate;
            }

            if (DateTime.TryParse(dateText, out parsedDate))
            {
                return parsedDate;
            }

            return null;
        }

        // Formats a nullable date for database storage
        private string? FormatDate(DateTime? date)
        {
            return date?.ToString("yyyy-MM-dd");
        }

        // Converts blank strings to DBNull for nullable database fields
        private object ToDbValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value;
        }

        // ===========================
        // Validation
        // ===========================

        // Validates all form inputs and returns a combined error message if needed
        private bool ValidateForm(out string errorMessage)
        {
            var errors = new List<string>();

            string studentId = StudentIDBox.Text.Trim().ToUpper();

            // Student ID
            if (string.IsNullOrWhiteSpace(studentId))
            {
                errors.Add("Student ID is required.");
            }
            else if (!Regex.IsMatch(studentId, @"^U\d{6}$"))
            {
                errors.Add("Student ID must be in the format UXXXXXX (6 digits).");
            }

            // Semester
            if (SemesterBox.SelectedItem == null)
                errors.Add("Semester must be selected.");

            // Year
            if (string.IsNullOrWhiteSpace(YearBox.Text))
            {
                errors.Add("Year is required.");
            }
            else if (!int.TryParse(YearBox.Text, out int year))
            {
                errors.Add("Year must be a valid number.");
            }
            else if (year < 2000 || year > 2100)
            {
                errors.Add("Year must be between 2000 and 2100.");
            }

            // Dates
            DateTime? checkout = CheckoutDatePicker.SelectedDate;
            DateTime? due = DueDatePicker.SelectedDate;
            DateTime? returned = ReturnDatePicker.SelectedDate;
            DateTime? checkin1 = CheckinDate1Picker.SelectedDate;
            DateTime? checkin2 = CheckinDate2Picker.SelectedDate;
            DateTime? checkin3 = CheckinDate3Picker.SelectedDate;

            if (!checkout.HasValue)
                errors.Add("Checkout Date is required.");

            if (!due.HasValue)
                errors.Add("Due Date is required.");

            if (checkout.HasValue && due.HasValue && due.Value < checkout.Value)
                errors.Add("Due Date cannot be before Checkout Date.");

            if (checkout.HasValue && returned.HasValue && returned.Value < checkout.Value)
                errors.Add("Return Date cannot be before Checkout Date.");

            if (checkout.HasValue && checkin1.HasValue && checkin1.Value < checkout.Value)
                errors.Add("Check-in Date 1 cannot be before Checkout Date.");

            if (checkout.HasValue && checkin2.HasValue && checkin2.Value < checkout.Value)
                errors.Add("Check-in Date 2 cannot be before Checkout Date.");

            if (checkout.HasValue && checkin3.HasValue && checkin3.Value < checkout.Value)
                errors.Add("Check-in Date 3 cannot be before Checkout Date.");

            if (checkin1.HasValue && checkin2.HasValue && checkin2.Value < checkin1.Value)
                errors.Add("Check-in Date 2 cannot be before Check-in Date 1.");

            if (checkin2.HasValue && checkin3.HasValue && checkin3.Value < checkin2.Value)
                errors.Add("Check-in Date 3 cannot be before Check-in Date 2.");

            errorMessage = string.Join("\n", errors);
            return errors.Count == 0;
        }

        // DELETE BUTTON
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this rental record?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                using var conn = new SqliteConnection(connectionString);
                conn.Open();

                using var transaction = conn.BeginTransaction();

                try
                {
                    using var deleteCmd = conn.CreateCommand();
                    deleteCmd.Transaction = transaction;
                    deleteCmd.CommandText = @"
                DELETE FROM Rentals
                WHERE RentalID = $rid;
            ";
                    deleteCmd.Parameters.AddWithValue("$rid", currentRental.RentalID); // use the ID, not the object
                    deleteCmd.ExecuteNonQuery();

                    transaction.Commit();

                    MessageBox.Show("Rental record deleted!");
                    DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Delete failed: " + ex.Message);
                }
            }
        }

    }
}