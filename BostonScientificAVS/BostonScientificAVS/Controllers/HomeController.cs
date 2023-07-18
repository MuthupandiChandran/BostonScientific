using BostonScientificAVS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BostonScientificAVS.Services;
using BostonScientificAVS.DTO;

namespace BostonScientificAVS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ItemService _itemService;
        public HomeController(ILogger<HomeController> logger, ItemService service)
        {
            _logger = logger;
            _itemService = service;
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

        public IActionResult Items()
        {
            var items = _itemService.getItems();
            return View(items);
        }


        [HttpPost("/SaveItem")]

        public ActionResult SaveItem(SingleItemEdit itemToEdit )
        {
            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}