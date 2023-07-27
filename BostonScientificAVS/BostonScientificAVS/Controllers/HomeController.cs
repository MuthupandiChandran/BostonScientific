using BostonScientificAVS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;


namespace BostonScientificAVS.Controllers
{
    [Authorize(Roles = "Admin,Supervisor,Operator")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Result()
        {
            return View();
        }

        public IActionResult HomeScreen()
        {
            bool myBooleanValue = false; // Default value in case TempData["MyBoolean"] is not set
            if (TempData.ContainsKey("AfterLogin") && TempData["AfterLogin"] is bool myBoolean)
            {
                myBooleanValue = myBoolean;
            }
            if (myBooleanValue)
            {
                int hoursInput = DotNetEnv.Env.GetInt("EXPIRE_TIME");
                ViewBag.HoursInput = hoursInput;
            } else
            {
                ViewBag.HoursInput = 0;
            }
            return View();
        }
        public IActionResult WorkOrderScan()
        {
            return View();
        }

        public IActionResult WorkOrderError()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}