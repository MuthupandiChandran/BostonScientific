using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Entity;
using Context;

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
                List<ApplicationUser> validUser = _context.Users.Where(x => x.EmpID == user.EmpID).ToList();

                if (validUser.Count > 0)
                {
                    ApplicationUser userInfo = validUser[0];
                    List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, userInfo.UserFullName),
                    new Claim(ClaimTypes.Name, userInfo.UserFullName),
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
                    try
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    return RedirectToAction("HomeScreen", "Home");

                }

                return RedirectToAction("LoginError");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest();
            }

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
