using Microsoft.AspNetCore.Mvc;
//using QuickBus.Data;
using QuickBus.Models;

namespace QuickBus.Controllers;
public class AccountController : Controller
{
    private readonly AppDbContext _db;
    public AccountController(AppDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        var user = _db.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        if (user == null)
        {
            ViewBag.Error = "Invalid credentials";
            return View();
        }
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserRole", user.Role);
        if (user.Role == "Admin") return RedirectToAction("Dashboard", "Admin");
        return RedirectToAction("Dashboard", "Passenger");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public IActionResult Register(string username, string password)
    {
        if (_db.Users.Any(u => u.Username == username))
        {
            ViewBag.Error = "Username already exists";
            return View();
        }
        var user = new User { Username = username, Password = password, Role = "Passenger" };
        _db.Users.Add(user);
        _db.SaveChanges();
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserRole", user.Role);
        return RedirectToAction("Dashboard", "Passenger");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
