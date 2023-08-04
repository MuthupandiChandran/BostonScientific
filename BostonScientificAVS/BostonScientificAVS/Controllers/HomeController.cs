using BostonScientificAVS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BostonScientificAVS.Services;
using BostonScientificAVS.DTO;
using Entity;
using System.Text;
using Context;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace BostonScientificAVS.Controllers
{
    [Authorize(Roles = "Admin,Supervisor,Operator")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _dataContext;
        
        public HomeController(ILogger<HomeController> logger,DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
        }

        public IActionResult Privacy()
        {
            return View();
        }
       
        public IActionResult Result()
        {
         
            WorkOrderInfo woi = new WorkOrderInfo();
            var transaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();
            var workOrder = _dataContext.Transaction.Where(x => x.WO_Lot_Num == transaction.WO_Lot_Num && x.Result != null).Distinct();
            woi.totalCount = workOrder.Count();
            woi.passedCount = workOrder.Where(x => x.Result == "Pass").Count();
            woi.failedCount = woi.totalCount - woi.passedCount;
            woi.scannedCount = workOrder.Where(x => x.Rescan_Initated == true).Count();
            woi.workOrderLotNo = transaction.WO_Lot_Num;
            return View(woi);
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
                    DateTime date = DateTime.ParseExact(barcodeParts[2], "MMddyyyy", null);
                    transaction.WO_Mfg_Date = date;
                    transaction.WO_Lot_Num = barcodeParts[3];

                    _dataContext.Transaction.Add(transaction);
                    _dataContext.SaveChanges();

                    TempData["WorkOrderLotNo"] = transaction.WO_Lot_Num;

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



        [HttpPost("/SaveProductLabel")]
        public async Task<IActionResult> SaveProductLabel(string productLabel, string productLabelSpec)
        {
            if (ModelState.IsValid)
            {
                string pattern = @"\((\d+)\)"; // Matches two digits within parentheses

                string[] barcodeParts = Regex.Split(productLabel, pattern);
                var transaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();
                if (transaction.Result != null)
                {
                    transaction.Rescan_Initated = true;
                }
                if (barcodeParts.Length >= 3)
                {
                    transaction.Product_Label_GTIN = barcodeParts[2];
                    DateTime dateTime = DateTime.ParseExact(barcodeParts[4], "yyMMdd", null);
                    transaction.Product_Use_By = dateTime;
                    transaction.Product_Lot_Num = barcodeParts[6];

                    ItemMaster item = await _dataContext.ItemMaster.FirstOrDefaultAsync(i => i.GTIN == transaction.Product_Label_GTIN);
                    if (item != null)
                    {
                        // Assign values from ItemMaster
                        transaction.DB_GTIN = item.GTIN;
                        transaction.DB_Catalog_Num = item.Catalog_Num;
                        DateTime workOrderDT = (DateTime)transaction.WO_Mfg_Date;
                        transaction.Calculated_Use_By = workOrderDT.AddDays((double)item.Shelf_Life);
                        transaction.DB_Label_Spec = item.Label_Spec;
                    }
                    else
                    {
                        return RedirectToAction("WorkOrderError", "Home");
                    }

                    transaction.Product_Label_Spec = productLabelSpec;
                }

                await _dataContext.SaveChangesAsync();

                return RedirectToAction("ValidateTransaction");
            }
            else
            {
                return RedirectToAction("WorkOrderError", "Home");
            }
        }


        public async Task<IActionResult> ValidateTransaction()
        {
            // Fetch the latest record 
            var transaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();

            if (transaction != null)
            {
               
                Result result = new Result();
                Mismatches mismatches = new Mismatches();
                Lhs lhsData = new Lhs();
                Rhs rhsData = new Rhs();
                lhsData.dbGTIN = transaction.DB_GTIN;
                lhsData.workOrderLotNo = transaction.WO_Lot_Num;
                lhsData.dbLabelSpec = transaction.DB_Label_Spec;
                lhsData.calculatedUseBy = transaction.Calculated_Use_By.ToString();
                lhsData.dbCatalogNo = transaction.DB_Catalog_Num;

                rhsData.productLabelGTIN = transaction.Product_Label_GTIN;
                rhsData.productLotNo = transaction.Product_Lot_Num;
                rhsData.productLabelSpec = transaction.Product_Label_Spec;
                rhsData.productUseBy = transaction.Product_Use_By.ToString();
                rhsData.workOrderCatalogNo = transaction.WO_Catalog_Num;

                result.rhsData = rhsData;
                result.lhsData = lhsData;
                // initially assume all are a match
                result.allMatch = true;
                if (transaction.DB_GTIN != transaction.Product_Label_GTIN || transaction.WO_Lot_Num != transaction.Product_Lot_Num
                    || transaction.DB_Label_Spec != transaction.Product_Label_Spec || transaction.Calculated_Use_By < transaction.Product_Use_By
                    || transaction.DB_Catalog_Num != transaction.WO_Catalog_Num)
                {
                    result.allMatch = false;
                }
                if (!result.allMatch)
                {
                    if (transaction.DB_GTIN != transaction.Product_Label_GTIN)
                    {
                        mismatches.GTINMismatch = true;
                    }
                    if (transaction.WO_Lot_Num != transaction.Product_Lot_Num)
                    {
                        mismatches.lotNoMismatch = true;
                    }
                    if (transaction.DB_Label_Spec != transaction.Product_Label_Spec)
                    {
                        mismatches.labelSpecMismatch = true;
                    }
                    if (transaction.Calculated_Use_By < transaction.Product_Use_By)
                    {
                        mismatches.calculatedUseByMismatch = true;
                    }
                    if (transaction.DB_Catalog_Num != transaction.WO_Catalog_Num)
                    {
                        mismatches.catalogNumMismatch = true;
                    }
                }
                result.mismatches = mismatches;
                // setting other fields in the latest transaction record
                if (result.allMatch)
                {
                    transaction.Result = "Pass";
                } else
                {
                    transaction.Result = "Fail";
                }
                var empId = User.Claims.FirstOrDefault(c => c.Type == "EmpID")?.Value;
                var user = _dataContext.Users.Where(x => x.EmpID == empId).FirstOrDefault();

                if (user != null)
                {
                    transaction.User = user.Id + "";
                }
                DateTime currentDateTime = DateTime.Now;
                transaction.Date_Time = currentDateTime.ToString();
                await _dataContext.SaveChangesAsync();

                WorkOrderInfo woi = new WorkOrderInfo();
                var workOrder = _dataContext.Transaction.Where(x => x.WO_Lot_Num == transaction.WO_Lot_Num && x.Result != null).Distinct();
                woi.totalCount = workOrder.Count();
                woi.passedCount = workOrder.Where(x => x.Result == "Pass").Count();
                woi.failedCount = woi.totalCount - woi.passedCount;
                woi.scannedCount = workOrder.Where(x => x.Rescan_Initated == true).Count();
                
                result.workOrderInfo = woi;
                return Json(result);
            }
            else
            {
                // Transaction records not found in the database for the scanned barcodes
                TempData["ErrorMessage"] = "Invalid barcodes. One or both of the scanned barcodes are not valid.";
                return Json(new { success = false, errorMessage = TempData["ErrorMessage"] });
            }
        }
        public IActionResult HomeScreen()
        {
            bool myBooleanValue = false; // Default value in case TempData["MyBoolean"] is not set
            if (TempData.ContainsKey("AfterLogin") && TempData["AfterLogin"] is bool myBoolean)
            {
                myBooleanValue = myBoolean;
            }
            if (myBooleanValue)
            {
                int hoursInput = DotNetEnv.Env.GetInt("EXPIRE_TIME");
                ViewBag.HoursInput = hoursInput;
            } else
            {
                ViewBag.HoursInput = 0;
            }
            return View();
        }
        public IActionResult WorkOrderScan()
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