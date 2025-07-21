using Microsoft.AspNetCore.Mvc;
//using QuickBus.Data;
using QuickBus.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace QuickBus.Controllers;
public class PassengerController : Controller
{
    private readonly AppDbContext _db;
    public PassengerController(AppDbContext db) => _db = db;

    private int? GetUserId() => HttpContext.Session.GetInt32("UserId");
    private bool IsPassenger() => HttpContext.Session.GetString("UserRole") == "Passenger";

    public IActionResult Dashboard()
    {
        if (!IsPassenger()) return RedirectToAction("Login", "Account");
        var userId = GetUserId() ?? 0;
        ViewBag.CounterCount = _db.Counters.Count();
        ViewBag.JourneyCount = _db.Journeys.Count();
        ViewBag.MyJourneyCount = _db.BookingRequests.Count(b => b.PassengerId == userId);
        return View();
    }

    public IActionResult Counters()
    {
        if (!IsPassenger()) return RedirectToAction("Login", "Account");
        return View(_db.Counters.ToList());
    }

    public IActionResult Journeys()
    {
        if (!IsPassenger()) return RedirectToAction("Login", "Account");
        var journeys = _db.Journeys
            .Include(j => j.StartCounter)
            .Include(j => j.EndCounter)
            .ToList();
        return View(journeys);
    }

    [HttpGet]
    public IActionResult Book(int journeyId)
    {
        if (!IsPassenger()) return RedirectToAction("Login", "Account");
        var journey = _db.Journeys
            .Include(j => j.Seats)
            .FirstOrDefault(j => j.Id == journeyId);
        if (journey == null) return NotFound();

        var bookedSeats = _db.Seats
            .Where(s => s.JourneyId == journeyId && s.BookingRequestId != null)
            .Select(s => s.SeatNumber)
            .ToList();

        ViewBag.Journey = journey;
        ViewBag.BookedSeats = bookedSeats;
        return View();
    }

    [HttpPost]
    public IActionResult Book(int journeyId, List<string> selectedSeats)
    {
        if (!IsPassenger()) return RedirectToAction("Login", "Account");
        var userId = GetUserId() ?? 0;
        if (selectedSeats == null || selectedSeats.Count == 0)
            return RedirectToAction("Journeys");

        // Check if seats are still available
        var alreadyBooked = _db.Seats
            .Where(s => s.JourneyId == journeyId && selectedSeats.Contains(s.SeatNumber) && s.BookingRequestId != null)
            .Any();
        if (alreadyBooked)
        {
            TempData["Error"] = "One or more selected seats are already booked.";
            return RedirectToAction("Book", new { journeyId });
        }

        if (!_db.BookingRequests.Any(b => b.JourneyId == journeyId && b.PassengerId == userId && b.Status == "Pending"))
        {
            _db.BookingRequests.Add(new BookingRequest
            {
                JourneyId = journeyId,
                PassengerId = userId,
                Status = "Pending",
                SeatNumbers = string.Join(",", selectedSeats)
            });
            _db.SaveChanges();
        }
        return RedirectToAction("MyJourneys");
    }

    public IActionResult MyJourneys()
    {
        if (!IsPassenger()) return RedirectToAction("Login", "Account");
        var userId = GetUserId() ?? 0;
        var myBookings = _db.BookingRequests
            .Include(b => b.Journey).ThenInclude(j => j.StartCounter)
            .Include(b => b.Journey).ThenInclude(j => j.EndCounter)
            .Where(b => b.PassengerId == userId)
            .ToList();
        return View(myBookings);
    }

    [HttpGet]
    public IActionResult BookingDetails(int id)
    {
        if (!IsPassenger()) return RedirectToAction("Login", "Account");
        var userId = GetUserId() ?? 0;
        var booking = _db.BookingRequests
            .Include(b => b.Journey).ThenInclude(j => j.Seats)
            .FirstOrDefault(b => b.Id == id && b.PassengerId == userId);
        if (booking == null) return NotFound();
        return View(booking);
    }
}
