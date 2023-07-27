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
using Microsoft.EntityFrameworkCore;

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

                if (barcodeParts.Length == 4 &&
                    barcodeParts.All(part => !string.IsNullOrEmpty(part.Trim())))
                {
                    // All parts are non-empty, proceed with saving the data
                    Transaction transaction = new Transaction();
                    transaction.WO_Catalog_Num = barcodeParts[0];
                    transaction.WO_Mfg_Date = barcodeParts[2];
                    transaction.WO_Lot_Num = barcodeParts[3];

                    _dataContext.Transaction.Add(transaction);
                    _dataContext.SaveChanges();

                    return RedirectToAction("Result", "Home");
                }
                else
                {
                    // Invalid input format, redirect to error page
                    return RedirectToAction("WorkOrderError", "Home");
                }
            }
            else
            {
                return RedirectToAction("WorkOrderError", "Home");
            }
        }



        [HttpPost]
        public async Task<IActionResult> SaveProductLabel(string input, string input1)
        {
            if (ModelState.IsValid)
            {
                string pattern = @"\((\d+)\)"; // Matches two digits within parentheses

                string[] barcodeParts = Regex.Split(input, pattern);
                Transaction transaction = new Transaction();
                if (barcodeParts.Length >= 3)
                {
                    transaction.Product_Label_GTIN = barcodeParts[2];
                    transaction.Product_Use_By = barcodeParts[4];
                    transaction.Product_Lot_Num = barcodeParts[6];

                    ItemMaster item = await _dataContext.ItemMaster.FirstOrDefaultAsync(i => i.GTIN == transaction.Product_Label_GTIN);
                    if (item != null)
                    {
                        // Assign values from ItemMaster
                        transaction.DB_GTIN = item.GTIN;
                        transaction.DB_Catalog_Num = item.Catalog_Num;
                        transaction.Calculated_Use_By = transaction.WO_Mfg_Date + item.Shelf_Life;
                        transaction.DB_Label_Spec = item.Label_Spec;
                    }
                    else
                    {
                        return RedirectToAction("WorkOrderError", "Home");
                    }

                    transaction.Product_Label_Spec = input1;
                }

                _dataContext.Transaction.Add(transaction);
                await _dataContext.SaveChangesAsync();

                return RedirectToAction("ValidateTransaction", new { productBarcode = transaction.Product_Label_GTIN, woBarcode = transaction.DB_GTIN });
            }
            else
            {
                return RedirectToAction("WorkOrderError", "Home");
            }
        }


        public async Task<IActionResult> ValidateTransaction(string woBarcode,string productBarcode)
        {
            // Fetch the relevant Transaction records from the database based on the scanned barcodes
            Transaction woTransaction = await _dataContext.Transaction.FirstOrDefaultAsync(t => t.DB_GTIN == woBarcode);
            Transaction productTransaction = await _dataContext.Transaction.FirstOrDefaultAsync(t => t.Product_Label_GTIN == productBarcode);

            if (woTransaction != null && productTransaction != null)
            {
                // Check if all the fields match to confirm a successful transaction
                if (woTransaction.WO_Lot_Num == productTransaction.Product_Lot_Num &&
                    woTransaction.DB_Label_Spec == productTransaction.Product_Label_Spec &&
                    woTransaction.Calculated_Use_By == productTransaction.Product_Use_By &&
                    woTransaction.DB_Catalog_Num == productTransaction.WO_Catalog_Num)
                {
                    // Valid transaction
                    return Json(new { success = true });
                }
                else
                {
                    // Invalid transaction, indicate which fields are not matching
                    string errorFields = "";
                    if (woTransaction.WO_Lot_Num != productTransaction.Product_Lot_Num)
                        errorFields += "WO Lot Number, ";
                    if (woTransaction.DB_Label_Spec != productTransaction.Product_Label_Spec)
                        errorFields += "DB Label Spec, ";
                    if (woTransaction.Calculated_Use_By != productTransaction.Product_Use_By)
                        errorFields += "Calculated Use By, ";
                    if (woTransaction.DB_Catalog_Num != productTransaction.WO_Catalog_Num)
                        errorFields += "DB Catalog Number, ";

                    errorFields = errorFields.TrimEnd(',', ' '); // Remove the trailing comma and space

                    TempData["ErrorMessage"] = "Invalid transaction. The following fields do not match: " + errorFields;
                    return Json(new { success = false, errorMessage = TempData["ErrorMessage"] });
                }
            }
            else
            {
                // Transaction records not found in the database for the scanned barcodes
                TempData["ErrorMessage"] = "Invalid barcodes. One or both of the scanned barcodes are not valid.";
                return Json(new { success = false, errorMessage = TempData["ErrorMessage"] });
            }
        }

        //public IActionResult InvalidTransaction()
        //{
        //    // Display the invalid transaction error message to the user
        //    ViewBag.ErrorMessage = TempData["ErrorMessage"];
        //    return View();
        //}







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