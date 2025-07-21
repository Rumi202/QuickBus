namespace QuickBus.Models;
public class Seat
{
    public int Id { get; set; }
    public int JourneyId { get; set; }
    public Journey? Journey { get; set; }
    public string SeatNumber { get; set; } = "";
    public int? BookingRequestId { get; set; }
    public BookingRequest? BookingRequest { get; set; }
}
