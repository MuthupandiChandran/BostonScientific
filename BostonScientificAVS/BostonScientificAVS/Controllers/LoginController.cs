using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Entity;
using Context;
using Microsoft.Data.SqlClient;

namespace BostonScientificAVS.Controllers
{
    public class LoginController : Controller
    {
        private readonly DataContext _context;

        public LoginController(DataContext context)
        {
            _context = context;
        }
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            if (claimUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("HomeScreen", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(ApplicationUser user)
        {
            try
            {
                bool isDatabaseConnected = checkDbConnection(); // Implement the method to check the database connection

                if (!isDatabaseConnected)
                {
                    TempData["ErrorMsg"] = "Database Connection Could Not Be Established";
                    return RedirectToAction("Login");
                }


                List<ApplicationUser> validUser = _context.Users.Where(x => x.EmpID == user.EmpID).ToList();

                if (validUser.Count > 0)
                {
                    ApplicationUser userInfo = validUser[0];
                    List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, userInfo.UserFullName),
                    new Claim(ClaimTypes.Name, userInfo.UserFullName),
                     new Claim("EmpID", userInfo.EmpID),
                    new Claim(ClaimTypes.Role, userInfo.UserRole.ToString())
                };

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                    // Additional properties you can add

                    AuthenticationProperties properties = new AuthenticationProperties()
                    {
                        AllowRefresh = true,
                        IsPersistent = true
                    };
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

                    // instructing the controller that this is the call after immediate login
                    bool afterLogin = true;
                    TempData["AfterLogin"] = afterLogin;
                    HttpContext.Session.SetString("CurrentUserName", userInfo.UserFullName);
                    return RedirectToAction("HomeScreen", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "EmployeeID Seems To be Invalid. Please retry again";
                    return RedirectToAction("Login");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest();
            }

        }

        //private string GetConnectionStringFromEnv()
        //{
        //    var appRootDirectory = Directory.GetCurrentDirectory();
        //    var envFilePath = Path.Combine(appRootDirectory, ".env");

        //    if (File.Exists(envFilePath))
        //    {
        //        var lines = File.ReadAllLines(envFilePath);
        //        foreach (var line in lines)
        //        {
        //            var parts = line.Split(new char[] { '=' }, 2);
        //            if (parts.Length == 2 && parts[0] == "DB_CON_STRING")
        //            {
        //                return parts[1];
        //            }
        //        }
        //    }

        //    return null; // Return null if the connection string is not found
        //}



        private Boolean checkDbConnection()
        {

            if (_context.Database.CanConnect())
            {
                return true;
            }
            return false;
        }



        public IActionResult LoginError()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            if (claimUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("HomeScreen", "Home");
            }
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login");
        }
    }
}
