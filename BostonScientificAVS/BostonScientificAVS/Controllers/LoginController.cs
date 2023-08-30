using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Entity;
using Context;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;

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
            try
            {
                bool isDatabaseConnected = checkDbConnection(); // Implement the method to check the database connection

                if (!isDatabaseConnected)
                {
                    TempData["ErrorMsg"] = "Database Connection Could Not Be Established";

                }


                ClaimsPrincipal claimUser = HttpContext.User;

                if (claimUser.Identity.IsAuthenticated)
                {
                    return RedirectToAction("HomeScreen", "Home");
                }
                return View();
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest();
            }

           

        }

        [HttpPost]
        public async Task<IActionResult> Login(ApplicationUser user)
        {
            try
            {
                //List<ApplicationUser> validUser = _context.Users.Where(x => x.EmpID == user.EmpID).First<ApplicationUser>();
                bool validUser = _context.Users.Any(x => x.EmpID == user.EmpID);
                
                if (validUser)
                {
                    ApplicationUser loggedInUser = _context.Users.Where(x => x.EmpID == user.EmpID).First<ApplicationUser>();
                    var userRole = loggedInUser.UserRole.ToString();
                    bool afterLogin = true;
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loggedInUser.UserFullName),
                        new Claim(ClaimTypes.Role, userRole),
                        new Claim("EmpID",loggedInUser.EmpID)

                    };
                    var claimsIdentity = new ClaimsIdentity(authClaims, "Login");
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    TempData["AfterLogin"] = afterLogin;
                    return RedirectToAction("HomeScreen", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "EmployeeID Seems To be Invalid. Please retry again";
                    return RedirectToAction("Login","Login");
                }
            }
            catch (Exception e)

            {
                Console.WriteLine(e);
                return BadRequest();
            }

        }

       
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
