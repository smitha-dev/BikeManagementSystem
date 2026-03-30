namespace FindlayBikeShop
{
    public class RentalRecord
    {
        public int RentalID { get; set; }
        public int BikeID { get; set; }

        public string? StudentID { get; set; }
        public string? SemesterRented { get; set; }
        public int Year { get; set; }

        public string? CheckoutDate { get; set; }
        public string? DueDate { get; set; }
        public string? ReturnDate { get; set; }

        public string? CheckinDate1 { get; set; }
        public string? CheckinDate2 { get; set; }
        public string? CheckinDate3 { get; set; }
    }
}
