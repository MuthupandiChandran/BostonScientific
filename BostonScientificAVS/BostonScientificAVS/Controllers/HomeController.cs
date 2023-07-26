using BostonScientificAVS.Map;
using BostonScientificAVS.Models;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using BostonScientificAVS.Services;
using BostonScientificAVS.DTO;
using Entity;
using System.Text;
using Context;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BostonScientificAVS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _dataContext;
        
        public HomeController(ILogger<HomeController> logger,DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
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

        [HttpPost]
        public IActionResult SaveWorkOrder(string input)
        {

            if (ModelState.IsValid)
            {
                string[] barcodeParts = input.Split('_');
                Transaction transaction = new Transaction();

                if (barcodeParts.Length == 4)
                {
                    transaction.WO_Catalog_Num = barcodeParts[0];
                    transaction.WO_Mfg_Date = barcodeParts[2];
                    transaction.WO_Lot_Num = barcodeParts[3];
                }

                _dataContext.Transaction.Add(transaction);
                _dataContext.SaveChanges();
                return RedirectToAction("Result", "Home");
            }
            else
            {
                return RedirectToAction("WorkOrderError", "Home");
            }

        }

        
        [HttpPost]
        public IActionResult SaveProductLabel(string input, string input1)
        {
            if (ModelState.IsValid)
            {
                string pattern = @"(?<=\()\d{2}(?=\))"; // Matches two digits within parentheses
                                                        //(01)12345(02)20256(03)81253

                string[] barcodeParts = Regex.Split(input, pattern);
                Transaction transaction = new Transaction();
                if (barcodeParts.Length >= 3)
                {
                    transaction.Product_Label_GTIN = barcodeParts[1];
                    transaction.Product_Use_By = barcodeParts[2];
                    transaction.Product_Lot_Num = barcodeParts[3];
                    transaction.Product_Label_Spec = input1;
                }

                _dataContext.Transaction.Add(transaction);
                _dataContext.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("WorkOrderError", "Home");
            }
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


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}