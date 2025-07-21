namespace QuickBus.Models;
public class BookingRequest
{
    public int Id { get; set; }
    public int JourneyId { get; set; }
    public Journey? Journey { get; set; }
    public int PassengerId { get; set; }
    public User? Passenger { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public string SeatNumbers { get; set; } = ""; // Comma-separated seat numbers
}
