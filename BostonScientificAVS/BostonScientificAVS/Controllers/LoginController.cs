using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Entity;
using Context;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using System.Net.WebSockets;
using BostonScientificAVS.Websocket;

namespace BostonScientificAVS.Controllers
{
    public class LoginController : Controller
    {
        private readonly DataContext _context;
        public IWebsocketHandler WebsocketHandler { get; }
        public LoginController(DataContext context, IWebsocketHandler websocketHandler)
        {
            _context = context;
            WebsocketHandler = websocketHandler;
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
                    TempData["ErrorMessage"] = "Employee ID Seems To be Invalid. Please retry again";
                    return RedirectToAction("Login","Login");
                }
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message;
                return RedirectToAction("Login", "Login");
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
            TempData.Clear();
            return RedirectToAction("Login", "Login");
        }

        [HttpGet("/ws")]
        public async Task Get()
        {
            try
            {
                var context = ControllerContext.HttpContext;
                var isSocketRequest = context.WebSockets.IsWebSocketRequest;

                if (isSocketRequest)
                {
                    WebSocket websocket = await context.WebSockets.AcceptWebSocketAsync();
                    await WebsocketHandler.Handle(Guid.NewGuid(), websocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}
