using Microsoft.AspNetCore.Mvc;

namespace BostonScientificAVS.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
