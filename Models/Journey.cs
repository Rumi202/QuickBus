namespace QuickBus.Models;

public class Journey
{
    public int Id { get; set; }
    public string BusNo { get; set; } = "";
    public DateTime JourneyDate { get; set; }
    public string Time { get; set; } = "";
    public int Rows { get; set; }
    public int StartCounterId { get; set; }
    public int EndCounterId { get; set; }
    public Counter? StartCounter { get; set; }
    public Counter? EndCounter { get; set; }
    public List<Seat> Seats { get; set; } = new();
}
