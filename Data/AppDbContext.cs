using Microsoft.EntityFrameworkCore;
using QuickBus.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
    public DbSet<Counter> Counters { get; set; }
    public DbSet<Journey> Journeys { get; set; }
    public DbSet<BookingRequest> BookingRequests { get; set; }
    public DbSet<Seat> Seats { get; set; }
}
