public class MaintenanceRecord
{
    public int MaintenanceID { get; set; }
    public int BikeID { get; set; }
    public string? DateFlagged { get; set; }
    public string? DateFixed { get; set; }
    public string? Notes { get; set; }
    public double Cost { get; set; }

    public string? PartNeeded { get; set; }
}