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
            string[] barcodeParts = input.Split('_');

            if (barcodeParts.Length == 4 && barcodeParts.All(part => !string.IsNullOrEmpty(part.Trim())))
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
                // Invalid input format, add a model-level error
                TempData["ErrorMessage"] = "Work Order Seems To be Invalid. Please retry again";
                return View("WorkOrderScan");
            }
        }

        [HttpPost("/SaveProductLabel")]
        public async Task<IActionResult> SaveProductLabel(string productLabel, string productLabelSpec)
        {
            if (ModelState.IsValid)
            {
                string pattern = @"^\d{2}(\d{14})\d{2}(\d{6})(\d{2})(\w+)";
                Match match = Regex.Match(productLabel, pattern);

                Transaction latestTransaction = null; // Declare before the if block

                if (match.Success)
                {
                    latestTransaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();
                    if (latestTransaction.Result != null)
                    {
                        latestTransaction.Rescan_Initated = true;
                    }

                    if (productLabel.Length == 34)
                    {
                        latestTransaction.Product_Label_GTIN = match.Groups[1].Value;
                        DateTime dateTime = DateTime.ParseExact(match.Groups[2].Value, "yyMMdd", null);
                        latestTransaction.Product_Use_By = dateTime;
                        latestTransaction.Product_Lot_Num = match.Groups[4].Value;

                        ItemMaster item = await _dataContext.ItemMaster.FirstOrDefaultAsync(i => i.GTIN == latestTransaction.Product_Label_GTIN);
                        if (item != null)
                        {
                            int shelfLife = item.Shelf_Life ?? 0;
                            // Assign values from ItemMaster
                            latestTransaction.DB_GTIN = item.GTIN;
                            latestTransaction.DB_Catalog_Num = item.Catalog_Num;
                            DateTime workOrderDT = (DateTime)latestTransaction.WO_Mfg_Date;
                            latestTransaction.Calculated_Use_By = workOrderDT.AddDays((double)shelfLife);
                            latestTransaction.DB_Label_Spec = item.Label_Spec;
                            latestTransaction.Shelf_Life = shelfLife;
                        }
                        else
                        {
                            return RedirectToAction("WorkOrderError", "Home");
                        }

                        latestTransaction.Product_Label_Spec = productLabelSpec;
                    }
                    else
                    {
                        return BadRequest(new { errorMessage = "Product Label spec Input is Invalid Format" });
                    }
                }
                else
                {
                    return BadRequest(new { errorMessage = "Product Label spec Input is Invalid Format" });
                }

                if (latestTransaction != null)
                {
                    await _dataContext.SaveChangesAsync();
                }
                else
                {
                    return BadRequest(new { errorMessage = "Product Label spec Input is Invalid Format" });
                }

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
                    || transaction.DB_Label_Spec != transaction.Product_Label_Spec || transaction.Calculated_Use_By > transaction.Product_Use_By
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
                    
                }
                else
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

        [HttpPost]
        public ActionResult CheckSupervisorId(string supervisorEmpId)
        {
            // Get the user corresponding to the provided EmpID
            ApplicationUser user = _dataContext.Users.FirstOrDefault(u => u.EmpID == supervisorEmpId);

            if (user != null && user.UserRole == UserRole.Supervisor)
            {
                // Supervisor ID is valid
                return Json(true);
            }
            else
            {
                // Supervisor ID is invalid or not a supervisor, return an error
                return Json(new { error = "Supervisor ID is invalid. Please enter a valid ID." });
            }
        }


        public IActionResult WorkOrderScan()
        {
            return View();
        }
        public IActionResult WorkOrderBarcodeScan()
        {
            return View();
        }
        public IActionResult CartonLabelScan()
        {
            return View();
        }
        public IActionResult ProductLabelBarcodeScan()
        {
            workOrderInfo woi = new workOrderInfo();
            var transaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();
            var workOrder = _dataContext.Transaction.Where(x => x.WO_Lot_Num == transaction.WO_Lot_Num && x.Result != null).Distinct();
            woi.totalCount = workOrder.Count();
            woi.passedCount = workOrder.Where(x => x.Result == "Pass").Count();
            woi.failedCount = woi.totalCount - woi.passedCount;
            woi.scannedCount = workOrder.Where(x => x.Rescan_Initated == true).Count();
            woi.workOrderLotNo = transaction.WO_Lot_Num;
            return View(woi);
        }
       
        public async Task<IActionResult> FinalResult()
        {
            var transaction = await _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefaultAsync();

            if (transaction != null)
            {
                FinalResult result = new FinalResult();
                mismatchess mismatches = new mismatchess();
                LHS lhsdata = new LHS();
                RHS rhsdata = new RHS();
                lhsdata.cartonGTIN = transaction.Carton_Label_GTIN;
                lhsdata.cartonGTIN = transaction.Carton_Label_GTIN;
                lhsdata.woLotNo = transaction.WO_Lot_Num;
                lhsdata.cartonLabelSpec = transaction.Carton_Label_Spec;
                lhsdata.cartonUseBy = transaction.Carton_Use_By.ToString();
                lhsdata.woCatalogNumber = transaction.WO_Catalog_Num;
                lhsdata.cartonLotNo = transaction.Carton_Lot_Num;
                lhsdata.scannedIFU = transaction.Scanned_IFU;
                lhsdata.cartonUseBy = transaction.Carton_Use_By.ToString();

                rhsdata.dbGTIN = transaction.DB_GTIN;
                rhsdata.productLabelGTIN = transaction.Product_Label_GTIN;
                rhsdata.productLotNo = transaction.Product_Lot_Num;
                rhsdata.dbLabelSpec = transaction.DB_Label_Spec;
                rhsdata.calculateUseBy = transaction.Calculated_Use_By.ToString();
                rhsdata.dbCatalogNo = transaction.DB_Catalog_Num;
                rhsdata.productLotNo = transaction.Product_Lot_Num;
                rhsdata.dbIFU = transaction.DB_IFU;
                rhsdata.productUseBy = transaction.Product_Use_By.ToString();


                result.rhsData = rhsdata;
                result.lhsData = lhsdata;
                result.allMatch = true;
                if(transaction.Carton_Label_GTIN!=transaction.DB_GTIN||transaction.Carton_Label_GTIN!=transaction.Product_Label_GTIN||transaction.WO_Lot_Num!=transaction.Product_Lot_Num||transaction.Carton_Label_Spec!=transaction.DB_Label_Spec||transaction.Carton_Use_By!=transaction.Calculated_Use_By||transaction.WO_Catalog_Num!=transaction.DB_Catalog_Num||transaction.Carton_Lot_Num!=transaction.Product_Lot_Num||transaction.Scanned_IFU!=transaction.DB_IFU||transaction.Carton_Use_By!=transaction.Product_Use_By)
                {
                    result.allMatch = false;
                }
                if (!result.allMatch)
                {
                    if (transaction.DB_GTIN != transaction.Carton_Label_GTIN)
                    {
                        mismatches.gtinmismatch = true;
                    }
                    if (transaction.Carton_Label_GTIN != transaction.Product_Label_GTIN)
                    {
                        mismatches.gtinmismatches = true;
                    }
                    if (transaction.WO_Lot_Num != transaction.Product_Lot_Num)
                    {
                        mismatches.lotNoMismatch = true;
                    }
                    if (transaction.Carton_Label_Spec != transaction.DB_Label_Spec)
                    {
                        mismatches.labelSpecMismatch = true;
                    }
                    if (transaction.Carton_Use_By != transaction.Calculated_Use_By)
                    {
                        mismatches.calculatedUseByMismatches = true;
                    }

                    if (transaction.WO_Catalog_Num != transaction.DB_Catalog_Num)
                    {
                        mismatches.catalogNumMismatch = true;
                    }
                    if (transaction.Carton_Lot_Num != transaction.Product_Lot_Num)
                    {
                        mismatches.LotNumberMisMatch = true;
                    }
                    if (transaction.Scanned_IFU != transaction.DB_IFU)
                    {
                        mismatches.ifumismatches = true;
                    }
                    if (transaction.Carton_Use_By != transaction.Product_Use_By)
                    {
                        mismatches.CalculatedUseByMismatch = true;
                    }
                }
                
                if (result.allMatch)
                {
                    transaction.Result = "Pass";                 

                }
                else
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

                workOrderInfo woi = new workOrderInfo();
                var workOrder = _dataContext.Transaction.Where(x => x.WO_Lot_Num == transaction.WO_Lot_Num && x.Result != null).Distinct();
                woi.totalCount = workOrder.Count();
                woi.passedCount = workOrder.Where(x => x.Result == "Pass").Count();
                woi.failedCount = woi.totalCount - woi.passedCount;
                woi.scannedCount = workOrder.Where(x => x.Rescan_Initated == true).Count();

                result.workOrderInfo = woi;
                result.mismatches = mismatches;
                var objdata = result;
                return Json(objdata);
            }
            else
            {
                
                TempData["ErrorMessage"] = "Invalid barcodes. One or both of the scanned barcodes are not valid.";
                return Json(new { success = false, errorMessage = TempData["ErrorMessage"] });
            }
        }
        [HttpPost]
        public IActionResult SaveWorkOrderBarcode(string input)
        {
            string[] barcodeParts = input.Split('_');
            if (barcodeParts.Length == 4 && barcodeParts.All(part => !string.IsNullOrEmpty(part.Trim())))
            {

                Transaction transaction = new Transaction();
                transaction.WO_Catalog_Num = barcodeParts[0];
                DateTime date = DateTime.ParseExact(barcodeParts[2], "MMddyyyy", null);
                transaction.WO_Mfg_Date = date;
                transaction.WO_Lot_Num = barcodeParts[3];

                _dataContext.Transaction.Add(transaction);
                _dataContext.SaveChanges();

                TempData["WorkOrderLotNo"] = transaction.WO_Lot_Num;
                return RedirectToAction("CartonLabelScan", "Home");
            }
            else
            {
                TempData["ErrorMessage"] = "Work Order Seems To be Invalid. Please retry again";
                return View("WorkOrderBarcodeScan");
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveCartonLabel(string input1, string input2)
        {
            if (ModelState.IsValid)
            {
                string pattern = @"^\d{2}(\d{14})\d{2}(\d{6})(\d{2})(\w+)";
                Match match = Regex.Match(input1, pattern);

                Transaction transaction = null; // Declare the variable outside the block

                if (match.Success)
                {
                    transaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();

                    if (input1.Length==34)
                    {         
                        transaction.Carton_Label_GTIN = match.Groups[1].Value;
                        DateTime dateTime = DateTime.ParseExact(match.Groups[2].Value, "yyMMdd", null);
                        transaction.Carton_Use_By = dateTime;
                        transaction.Carton_Lot_Num = match.Groups[4].Value;

                        ItemMaster item = await _dataContext.ItemMaster.FirstOrDefaultAsync(i => i.GTIN == transaction.Carton_Label_GTIN);
                        if (item != null)
                        {
                            // Assign values from ItemMaster
                            int shelfLife = item.Shelf_Life ?? 0;
                            transaction.DB_GTIN = item.GTIN;
                            transaction.DB_Catalog_Num = item.Catalog_Num;
                            DateTime workOrderDT = (DateTime)transaction.WO_Mfg_Date;
                            transaction.Calculated_Use_By = workOrderDT.AddDays((double)shelfLife);
                            transaction.DB_Label_Spec = item.Label_Spec;
                            transaction.DB_IFU = item.IFU;
                            transaction.Shelf_Life = shelfLife;
                        }
                        else
                        {
                            return RedirectToAction("WorkOrderError", "Home");
                        }
                   
                            transaction.Carton_Label_Spec = input2;  

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Carton Label Input is Invalid Format";
                        return View("CartonLabelScan");
                    }

                }
                if (transaction != null)
                {
                    await _dataContext.SaveChangesAsync();
                    TempData["WorkOrderLotNo"] = transaction.WO_Lot_Num;
                }
                else
                {
                    TempData["ErrorMessage"] = "Carton Label Input is Invalid Format";
                    return View("CartonLabelScan");
                }
                return RedirectToAction("ProductLabelBarcodeScan", "Home");
            }
            else
            {
                return RedirectToAction("WorkOrderError", "Home");
            }
        }

        [HttpPost("/SaveProductLabelBarcode")]
        public async Task<IActionResult> SaveProductLabelBarcode(string input1, string input2, string input3)
        {
            if (ModelState.IsValid)
            {
                string pattern = @"^\d{2}(\d{14})\d{2}(\d{6})(\d{2})(\w+)";
                Match match = Regex.Match(input1, pattern);

                Transaction latestTransaction = null; // Declare before the if block

                if (match.Success)
                {                  
                    latestTransaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();
                    if (latestTransaction.Result != null)
                    {
                        latestTransaction.Rescan_Initated = true;
                    }

                    if (input1.Length==34)
                    {
                        latestTransaction.Product_Label_GTIN = match.Groups[1].Value;
                        DateTime dateTime = DateTime.ParseExact(match.Groups[2].Value, "yyMMdd", null);
                        latestTransaction.Product_Use_By = dateTime;
                        latestTransaction.Product_Lot_Num = match.Groups[4].Value;

                        ItemMaster item = await _dataContext.ItemMaster.FirstOrDefaultAsync(i => i.GTIN == latestTransaction.Carton_Label_GTIN);
                        if (item != null)
                        {
                            // Assign values from ItemMaster
                            latestTransaction.DB_GTIN = item.GTIN;
                            latestTransaction.DB_Catalog_Num = item.Catalog_Num;
                            DateTime workOrderDT = (DateTime)latestTransaction.WO_Mfg_Date;
                            latestTransaction.Calculated_Use_By = workOrderDT.AddDays((double)item.Shelf_Life);
                            latestTransaction.DB_Label_Spec = item.Label_Spec;
                            latestTransaction.DB_IFU = item.IFU;
                        }
                        else
                        {
                            return RedirectToAction("WorkOrderError", "Home");
                        }

                        latestTransaction.Product_Label_Spec = input2;
                        latestTransaction.Scanned_IFU = input3;
                    }
                    else
                    {
                        return BadRequest(new { errorMessage = "Product Label spec Input is Invalid Format" });

                    }
                }
                else
                {
                    return BadRequest(new { errorMessage = "Product Label spec Input is Invalid Format" });
                }

                if (latestTransaction != null)
                {
                    await _dataContext.SaveChangesAsync();
                }
                else
                {
                    return BadRequest(new { errorMessage = "Product Label spec Input is Invalid Format" });
                }

                return RedirectToAction("FinalResult");
            }
            else
            {
                return RedirectToAction("WorkOrderError", "Home");
            }
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