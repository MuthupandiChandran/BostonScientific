using BostonScientificAVS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BostonScientificAVS.Services;
using BostonScientificAVS.DTO;
using Entity;

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

        public PartialViewResult RefreshItemsGrid()
        {
            var items = _itemService.getItems();
            return PartialView("_ItemsTable", items);
        }


        [HttpPost("/UpdateItem")]
        public ActionResult UpdateItem(SingleItemEdit itemToEdit )
        {
            try
            {
                if (itemToEdit != null)
                {
                    _itemService.updateItem(itemToEdit); 
                }
                return Ok("Successfully updated item");
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return Ok("Error While updating item");
            }
           
        }

        [HttpPost("/SaveNewItem")]
        public ActionResult SaveNewItem(ItemMaster item)
        {
            try
            {
                if (item != null)
                {
                    _itemService.saveNewItem(item);
                }
                return Ok("success");
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return Ok("error while adding item");
            }
        }

        // MVC Controller action method to handle file upload
        [HttpPost("/uploadItemsExcel")]
        public async Task<string> ImportCSV(IFormFile file)
        {
            try
            {
                await _itemService.importCsv(file);
                return "file upload Successfully";
            }
            catch(Exception e)
            {
                return "file upload failed";
            }
            
            
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}