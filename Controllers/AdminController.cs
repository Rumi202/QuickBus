using Microsoft.AspNetCore.Mvc;
//using QuickBus.Data;
using QuickBus.Models;
using Microsoft.EntityFrameworkCore;

namespace QuickBus.Controllers;
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    private bool IsAdmin() => HttpContext.Session.GetString("UserRole") == "Admin";

    public IActionResult Dashboard()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        ViewBag.CounterCount = _db.Counters.Count();
        ViewBag.PassengerCount = _db.Users.Count(u => u.Role == "Passenger");
        ViewBag.JourneyCount = _db.Journeys.Count();
        ViewBag.PendingBookingCount = _db.BookingRequests.Count(b => b.Status == "Pending");
        return View();
    }

    public IActionResult Counters()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        return View(_db.Counters.ToList());
    }

    [HttpPost]
    public IActionResult AddCounter(string name)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        if (!string.IsNullOrWhiteSpace(name))
        {
            _db.Counters.Add(new Counter { Name = name });
            _db.SaveChanges();
        }
        return RedirectToAction("Counters");
    }

    public IActionResult Passengers()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        return View(_db.Users.Where(u => u.Role == "Passenger").ToList());
    }

    public IActionResult Journeys()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        ViewBag.Counters = _db.Counters.ToList();
        var journeys = _db.Journeys
            .Include(j => j.StartCounter)
            .Include(j => j.EndCounter)
            .Include(j => j.Seats)
            .ToList();
        return View(journeys);
    }

    [HttpPost]
    public IActionResult AddJourney(string busNo, DateTime journeyDate, string time, int rows, int startCounterId, int endCounterId)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        if (startCounterId == endCounterId)
        {
            TempData["Error"] = "Start and End Counter cannot be the same.";
            return RedirectToAction("Journeys");
        }
        if (journeyDate.Date < DateTime.Now.Date)
        {
            TempData["Error"] = "Journey date cannot be in the past.";
            return RedirectToAction("Journeys");
        }
        var journey = new Journey
        {
            BusNo = busNo,
            JourneyDate = journeyDate,
            Time = time,
            Rows = rows,
            StartCounterId = startCounterId,
            EndCounterId = endCounterId
        };
        _db.Journeys.Add(journey);
        _db.SaveChanges();

        // Generate seats
        var seatList = new List<Seat>();
        for (int i = 0; i < rows; i++)
        {
            char rowChar = (char)('a' + i);
            for (int j = 1; j <= 4; j++)
            {
                seatList.Add(new Seat
                {
                    JourneyId = journey.Id,
                    SeatNumber = $"{rowChar}{j}"
                });
            }
        }
        _db.Seats.AddRange(seatList);
        _db.SaveChanges();

        return RedirectToAction("Journeys");
    }

    public IActionResult BookingRequests()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        var requests = _db.BookingRequests
            .Include(b => b.Journey).ThenInclude(j => j.StartCounter)
            .Include(b => b.Journey).ThenInclude(j => j.EndCounter)
            .Include(b => b.Passenger)
            .ToList();
        return View(requests);
    }

    [HttpGet]
    public IActionResult BookingDetails(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        // Try to get by BookingRequest first, fallback to Journey
        var request = _db.BookingRequests
            .Include(b => b.Journey).ThenInclude(j => j.Seats).ThenInclude(s => s.BookingRequest).ThenInclude(br => br.Passenger)
            .Include(b => b.Passenger)
            .FirstOrDefault(b => b.Id == id);

        if (request != null)
            return View(request);

        // If not found, treat id as JourneyId and show all bookings for that journey
        var journey = _db.Journeys
            .Include(j => j.Seats).ThenInclude(s => s.BookingRequest).ThenInclude(br => br.Passenger)
            .FirstOrDefault(j => j.Id == id);

        if (journey == null) return NotFound();

        // Create a dummy BookingRequest to pass journey and seats
        var dummy = new BookingRequest
        {
            Journey = journey,
            SeatNumbers = "",
            Status = "",
            Passenger = null
        };
        return View(dummy);
    }


    [HttpPost]
    public IActionResult Approve(int id)
    {
        var req = _db.BookingRequests.Include(b => b.Journey).FirstOrDefault(b => b.Id == id);
        if (req != null && req.Status == "Pending")
        {
            // Allocate seats
            var seatNumbers = req.SeatNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var seats = _db.Seats.Where(s => s.JourneyId == req.JourneyId && seatNumbers.Contains(s.SeatNumber)).ToList();
            foreach (var seat in seats)
            {
                seat.BookingRequestId = req.Id;
            }
            req.Status = "Approved";
            _db.SaveChanges();
        }
        return RedirectToAction("BookingRequests");
    }

    [HttpPost]
    public IActionResult Reject(int id)
    {
        var req = _db.BookingRequests.Find(id);
        if (req != null) { req.Status = "Rejected"; _db.SaveChanges(); }
        return RedirectToAction("BookingRequests");
    }
}
