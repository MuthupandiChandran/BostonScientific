using BostonScientificAVS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;


namespace BostonScientificAVS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private  ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

    

        public IActionResult Index()
        {
            return View();
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
            return View();
        }

        public IActionResult WorkOrderScan()
        {
            return View();
        }

        public IActionResult LoginError()
        {
            return View();
        }

        public IActionResult WorkOrderError()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Users()
        {
           /* ApplicationUser user = new ApplicationUser()*/;
            var employees = _context.Users.ToList();
            List<ApplicationUser> users = new List<ApplicationUser>();
            if (employees != null)
            {
                foreach (var user in employees)
                {
                    var ApplicationUser = new ApplicationUser()
                    {
                        EmpID = user.EmpID,
                        UserFullName = user.UserFullName,
                        UserRole = user.UserRole

                    };
                    users.Add(ApplicationUser);

                }
                return View(users);
            }
            return View(users);
        }








        [HttpPost]
        public JsonResult SaveUser(ApplicationUser user)
        {
            
                var existingRecord = _context.Users.FirstOrDefault(u => u.EmpID == user.EmpID);
                if (existingRecord != null)
                {
                    existingRecord.UserFullName = user.UserFullName;
                    existingRecord.UserRole = user.UserRole;
                    _context.SaveChanges();
                return Json(new { success = true, message = "Updated successfully!" });
            
                }

            else
            {
                
                var newRecord = new ApplicationUser()
                {
                    EmpID = user.EmpID,
                    UserFullName = user.UserFullName,
                    UserRole = user.UserRole
                };
                _context.Users.Add(newRecord);
                _context.SaveChanges();
                return Json(new { success = true });
            }
        }






        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}