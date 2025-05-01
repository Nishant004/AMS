using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AMS.Models;
using AMS.Helpers;

namespace AMS.Controllers;

public class HomeController : Controller
{

    public IActionResult Index()
    {

        var userSession = SessionHelper.GetUserSession(HttpContext);
        if (userSession == null)
        {
            return View();
        }

        if (userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Attendance", new { area = "" });
        }
        else if (userSession.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Employee", new { area = "Employee" });
        }

        // Fallback: unknown role
        TempData["Error"] = "Unauthorized role access.";
        return View();


    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
