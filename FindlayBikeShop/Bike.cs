using System.Collections.Generic;

namespace FindlayBikeShop
{
    public class Bike
    {
        public int BikeID { get; set; }
        public string? Brand { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Status { get; set; }
        public string? LastUpdated { get; set; }
        public string? Notes { get; set; }
        public string? Photo { get; set; }

        public string Display
        {
            get
            {
                var parts = new List<string> { $"ID: {BikeID}" };

                if (!string.IsNullOrEmpty(Status))
                    parts.Add($"Status: {Status}");
                if (!string.IsNullOrEmpty(LastUpdated))
                    parts.Add($"Last Updated: {LastUpdated}");
                if (!string.IsNullOrEmpty(Notes))
                    parts.Add($"Notes: {Notes}");
                if (!string.IsNullOrEmpty(Brand))
                    parts.Add($"Brand: {Brand}");
                if (!string.IsNullOrEmpty(Size))
                    parts.Add($"Size: {Size}");
                if (!string.IsNullOrEmpty(Color))
                    parts.Add($"Color: {Color}");

                return string.Join(" - ", parts);
            }
        }
    }
}