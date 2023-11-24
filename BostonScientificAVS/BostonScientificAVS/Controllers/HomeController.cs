using BostonScientificAVS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BostonScientificAVS.Services;
using BostonScientificAVS.DTO;
using Entity;
using System.Net.WebSockets;
using System.Text;
using Context;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using BostonScientificAVS.Websocket;
using Azure;
using System.Reflection.Metadata;
using System.Diagnostics;

namespace BostonScientificAVS.Controllers
{
    [Authorize(Roles = "Admin,Supervisor,Operator")]

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _dataContext;
        public IWebsocketHandler WebsocketHandler { get; }

        public HomeController(ILogger<HomeController> logger, DataContext dataContext, IWebsocketHandler websocketHandler)
        {
            _logger = logger;
            _dataContext = dataContext;
            WebsocketHandler = websocketHandler;

        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Result(bool udpMessage)
        {
            if (udpMessage)
            {
                // If udpMessage is "N", send it to the socket method
                SendMessageToUDPclient("N");
                udpMessage = true;
            }

            WorkOrderInfo woi = new WorkOrderInfo();
            var transaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();
            var itemmaster = _dataContext.ItemMaster.OrderByDescending(x => x.GTIN).FirstOrDefault();
            var workOrder = _dataContext.Transaction.Where(x => x.WO_Lot_Num == transaction.WO_Lot_Num && x.Type == "Packaging" && x.Result != null).Distinct();
            woi.totalCount = workOrder.Count();
            woi.passedCount = workOrder.Where(x => x.Result == "Pass").Count();
            woi.failedCount = woi.totalCount - woi.passedCount;
            woi.scannedCount = workOrder.Where(x => x.Rescan_Initated == true).Count();
            woi.workOrderLotNo = transaction.WO_Lot_Num;
            woi.workOrderCatalogNo = transaction.WO_Catalog_Num;
            woi.workOrderMfgDate = transaction.WO_Mfg_Date;
            woi.shelflife = itemmaster.Shelf_Life;
            woi.DbCatalogNo = itemmaster.Catalog_Num;
            woi.Dbspec = itemmaster.Label_Spec;
            woi.DbGtin = itemmaster.GTIN;
            return View(woi);
        }

        [HttpPost]
        public IActionResult SaveWorkOrder(string input,bool udpMessage)
        {
            if (input != null && input.Trim() != "")
            {
                string[] barcodeParts = input.Split('_');

                if (barcodeParts.Length == 4 && barcodeParts[2].Length == 8 && barcodeParts.All(part => !string.IsNullOrEmpty(part.Trim())))
                {
                    // All parts are non-empty, proceed with saving the data
                    Transaction transaction = new Transaction();
                    transaction.WO_Catalog_Num = barcodeParts[0];
                    DateTime date = DateTime.ParseExact(barcodeParts[2], "MMddyyyy", null);
                    transaction.WO_Mfg_Date = date;
                    transaction.WO_Lot_Num = barcodeParts[3];
                    transaction.Date_Time = DateTime.Now;
                    _dataContext.Transaction.Add(transaction);
                    _dataContext.SaveChanges();

                    TempData["WorkOrderLotNo"] = transaction.WO_Lot_Num;                 
                }
                else
                {
                    // Invalid input format, add a model-level error
                    SendMessageToUDPclient("E");
                    TempData["ErrorMessage"] = "Work Order Seems To Be Invalid. Please retry again";
                    return View("WorkOrderScan");
                }
            
                    if (udpMessage)
                    {
                        return RedirectToAction("Result", "Home", new { udpMessage = udpMessage });
                    }
                    else
                    {

                        return RedirectToAction("Result", "Home");
                    }
            }
            else
            {
                // Handle the case when input is "Null" or empty
                SendMessageToUDPclient("E");
                TempData["ErrorMessage"] = "Work Order is Null or Empty. Please retry with a valid input.";
                return View("WorkOrderScan");
            }
        }


        [HttpPost("/SaveProductLabel")]
        public async Task<IActionResult> SaveProductLabel(string productLabel, string productLabelSpec, bool gtinmismatch)
        {
            if (string.IsNullOrEmpty(productLabelSpec) && productLabel.Length >34)
            {
                string[] barcodeParts = productLabel.Split('_');

                if (barcodeParts.Length == 4 && barcodeParts[2].Length == 8 && barcodeParts.All(part => !string.IsNullOrEmpty(part.Trim())))
                {
                    // All parts are non-empty, proceed with saving the data
                    Transaction transaction = new Transaction();
                    transaction.WO_Catalog_Num = barcodeParts[0];
                    DateTime date = DateTime.ParseExact(barcodeParts[2], "MMddyyyy", null);
                    transaction.WO_Mfg_Date = date;
                    transaction.WO_Lot_Num = barcodeParts[3];
                    transaction.Date_Time = DateTime.Now;
                    _dataContext.Transaction.Add(transaction);
                    _dataContext.SaveChanges();

                    TempData["WorkOrderLotNo"] = transaction.WO_Lot_Num;
                    return Json(new {workorder = true,
                        transaction.WO_Catalog_Num,
                        transaction.WO_Mfg_Date,
                        transaction.WO_Lot_Num
                    });
                }
            }

            else
            {

                gtinmismatch = false; // Assuming this line is not needed in other scenarios
                string pattern = @"^\d{2}(\d{14})\d{2}(\d{6})(\d{2})(\w+)";
                Match match = Regex.Match(productLabel, pattern);
                if (!match.Success)
                {
                    await SendMessageToUDPclient("E");
                    return BadRequest(new { errorMessage = "Product Label spec Input is Invalid Format" });
                }
                else
                {

                    Transaction latestTransaction = null; // Declare before the if block

                    if (match.Success)
                    {
                        latestTransaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();
                        if (latestTransaction.Product_Label_GTIN != null)
                        {
                            var record = new Transaction
                            {
                                WO_Catalog_Num = latestTransaction.WO_Catalog_Num,
                                WO_Mfg_Date = latestTransaction.WO_Mfg_Date,
                                WO_Lot_Num = latestTransaction.WO_Lot_Num
                            };

                            _dataContext.Transaction.Add(record);
                            await _dataContext.SaveChangesAsync();
                            latestTransaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();
                        }

                        if (productLabel.Length == 34 && match.Groups[1].Length == 14 && match.Groups[2].Length == 6 && match.Groups[4].Length == 8)
                        {
                            latestTransaction.Product_Label_GTIN = match.Groups[1].Value;
                            DateTime dateTime = DateTime.ParseExact(match.Groups[2].Value, "yyMMdd", null);
                            latestTransaction.Product_Use_By = dateTime;
                            latestTransaction.Product_Lot_Num = match.Groups[4].Value;
                            latestTransaction.Type = "Packaging";

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
                                gtinmismatch = true;
                                latestTransaction.Product_Label_Spec = productLabelSpec;
                                await _dataContext.SaveChangesAsync();
                                await SendMessageToUDPclient("F");
                                return RedirectToAction("GTINmismatch", new { gtinMismatch = true, ProductLabelSpec = productLabelSpec });
                            }

                            latestTransaction.Product_Label_Spec = productLabelSpec;
                        }
                        else
                        {
                            await SendMessageToUDPclient("E");
                            return BadRequest(new { errorMessage = "Product Label spec Input is Invalid Format" });
                        }
                    }
                    else
                    {
                        await SendMessageToUDPclient("E");
                        return BadRequest(new { errorMessage = "Product Label spec Input is Invalid Format" });
                    }

                    if (latestTransaction != null)
                    {
                        await _dataContext.SaveChangesAsync();
                    }
                    else
                    {
                        await SendMessageToUDPclient("E");
                        return BadRequest(new { errorMessage = "Product Label spec Input is Invalid Format" });
                    }
                }
            }
            return RedirectToAction("ValidateTransaction");
        }

        public async Task<IActionResult> GTINmismatch(bool gtinMismatch,string ProductLabelSpec)
        {
            if (gtinMismatch)
            {
                Result result = new Result();
                var item = _dataContext.ItemMaster.OrderByDescending(x=>x.GTIN).FirstOrDefault();
                var transaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();
                transaction.Result = "Fail";
                DateTime currentDateTime = DateTime.Now;
                transaction.Date_Time = currentDateTime;
                var empId = User.Claims.FirstOrDefault(c => c.Type == "EmpID")?.Value;
                var user = _dataContext.Users.Where(x => x.EmpID == empId).FirstOrDefault();

                if (user != null)
                {
                    transaction.User = user.Id + "";
                }
                string Input3 = TempData.ContainsKey("Input3") ? (string)TempData["Input3"] : null;
                if (!string.IsNullOrEmpty(ProductLabelSpec))
                {
                    var product_Gtin = _dataContext.Transaction
                        .OrderByDescending(x => x.Transaction_Id)
                        .Select(x => x.Product_Label_GTIN)
                        .FirstOrDefault();

                    var product_spec = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).Select(x => x.Product_Label_Spec).FirstOrDefault();
                    var product_lot_no = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).Select(x => x.Product_Lot_Num).FirstOrDefault();
                    var wo_lot_no = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).Select(x => x.WO_Lot_Num).FirstOrDefault();
                    DateTime? workOrderDT = null;
                    var date1 = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).Select(x => x.WO_Mfg_Date).FirstOrDefault();
                    var date2 = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).Select(x => x.Product_Use_By).FirstOrDefault();
                    int shelfLife = item.Shelf_Life ?? 0;

                    if (date1 != null)
                    {
                        workOrderDT = (DateTime)date1;
                    }

                    if (workOrderDT.HasValue)
                    {
                        var date3 = workOrderDT.Value.AddDays((double)shelfLife);
                        var productuseby = date2.HasValue ? date2.Value.ToString("yyyy-MM-dd") : null;

                        DateTime date3DateTime;
                        DateTime productusebyDateTime;

                        if (DateTime.TryParseExact(date3.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date3DateTime) &&
                            DateTime.TryParseExact(productuseby, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out productusebyDateTime))
                        {
                            var wo_catalog_no = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).Select(x => x.WO_Catalog_Num).FirstOrDefault();
                            var count = _dataContext.Transaction.Where(x => x.WO_Lot_Num == transaction.WO_Lot_Num && x.Type == "Packaging" && x.Result != null).Distinct();
                            var countData = new countinfo
                            {
                                Product_Gtin = product_Gtin,
                                Db_Gtin = item.GTIN,
                                GTINMismatch = true, // Set the GTINMismatch flag to true
                                Db_Spec = item.Label_Spec,
                                Product_Spec = product_spec,
                                LabelMismatch = item.Label_Spec != ProductLabelSpec, // Conditionally set LabelMismatch
                                Product_Lot_No = product_lot_no,
                                WO_Lot_No = wo_lot_no,
                                LotnoMismatch = product_lot_no != wo_lot_no,
                                Calculate_Use_By = date3.ToString("yyyy-MM-dd"),
                                Pro_Use_By = productuseby,
                                FirstUseby_Mismatch = date3DateTime < productusebyDateTime, // Compare as DateTime objects
                                Wo_Catalog_Num = wo_catalog_no,
                                Db_Catalog_No = item.Catalog_Num,
                                CatalogMismatch = item.Catalog_Num != wo_catalog_no,
                                totalcount = count.Count(),
                                passedCount = count.Where(x => x.Result == "Pass").Count(),
                                failedCount = count.Count() - count.Where(x => x.Result == "Pass").Count(),
                                scannedCount = count.Where(x => x.Rescan_Initated == true).Count()
                            };
                            await _dataContext.SaveChangesAsync();
                            // Return the countData as JSON response
                            return Json(countData);
                        }
                    }
                }
                if (string.IsNullOrEmpty(ProductLabelSpec))
                {
                    var product_Gtin = _dataContext.Transaction
                    .OrderByDescending(x => x.Transaction_Id)
                    .Select(x => x.Product_Label_GTIN)
                    .FirstOrDefault();
                    var ifu = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).Select(x => x.Scanned_IFU).FirstOrDefault();
                    var carton_gtin = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).Select(x => x.Carton_Label_GTIN).FirstOrDefault();
                    var carton_spec = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).Select(x => x.Carton_Label_Spec).FirstOrDefault();
                    var wo_lot_no = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).Select(x => x.WO_Lot_Num).FirstOrDefault();
                    var product_lot_no = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).Select(x => x.Product_Lot_Num).FirstOrDefault();
                    var date1 = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).Select(x => x.Carton_Use_By).FirstOrDefault();
                    var date2 = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).Select(x => x.Calculated_Use_By).FirstOrDefault();
                    var date3 = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).Select(x => x.Product_Use_By).FirstOrDefault(); 
                    var carton_use_by = date1.HasValue ? date1.Value.ToString("yyyy-MM-dd") : null;
                    var calculate_use_by = date2.HasValue ? date2.Value.ToString("yyyy-MM-dd") : null;
                    var product_use_by = date3.HasValue ? date3.Value.ToString("yyy-MM-dd") : null;
                    var wo_catalog_no = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).Select(x => x.WO_Catalog_Num).FirstOrDefault();
                    var carton_lot_no = _dataContext.Transaction.OrderByDescending(x=>x.Transaction_Id).Select(x=>x.Carton_Lot_Num).FirstOrDefault();
                    var count = _dataContext.Transaction.Where(x => x.WO_Lot_Num == transaction.WO_Lot_Num && x.Type == "Boxing" && x.Result != null).Distinct();
                    var countData = new countinfo
                    {
                        Product_Gtin = product_Gtin,
                        Carton_Gtin = carton_gtin,
                        Db_Gtin = item.GTIN,
                        GTINMismatch = true,// Set the GTINMismatch flag to true
                        DB_GTIN_Mismatch = item.GTIN != carton_gtin,
                        Carton_Spec = carton_spec,
                        Db_Spec = item.Label_Spec,
                        LabelMismatch = carton_spec != item.Label_Spec,
                        Scanned_Ifu = Input3,
                        Db_Ifu = item.IFU,
                        IfuMismatch = item.IFU != ifu,
                        WO_Lot_No = wo_lot_no,
                        Product_Lot_No = product_lot_no,
                        LotnoMismatch = wo_lot_no != product_lot_no,
                        Carton_Use_By = carton_use_by,
                        Calculate_Use_By = calculate_use_by,
                        FirstUseby_Mismatch = carton_use_by != calculate_use_by,
                        Pro_Use_By = product_use_by,
                        SecondUseby_Mismatch = carton_use_by != product_use_by,
                        Db_Catalog_No = item.Catalog_Num,
                        Wo_Catalog_Num = wo_catalog_no,
                        CatalogMismatch = item.Catalog_Num != wo_catalog_no,
                        Carton_Lot_No = carton_lot_no,
                        LotNoMismatches = carton_lot_no != product_lot_no,
                        totalcount = count.Count(),
                        passedCount = count.Where(x => x.Result == "Pass").Count(),
                        failedCount = count.Count() - count.Where(x => x.Result == "Pass").Count(),
                        scannedCount = count.Where(x => x.Rescan_Initated == true).Count()


                    };
                    await _dataContext.SaveChangesAsync();

                    // Return the countData as JSON response
                    return Json(countData);
                }
            }

            // Handle case when GTINMismatch is not true
            return Ok(); // Return a default response if needed
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
                    || transaction.DB_Label_Spec != transaction.Product_Label_Spec || transaction.Calculated_Use_By != transaction.Product_Use_By
                    || transaction.DB_Catalog_Num != transaction.WO_Catalog_Num) //(mismatches = true)
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
                    if (transaction.Calculated_Use_By != transaction.Product_Use_By)
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
                    await SendMessageToUDPclient("P");
                }
                else
                {
                    transaction.Result = "Fail";
                    await SendMessageToUDPclient("F");
                }
                var empId = User.Claims.FirstOrDefault(c => c.Type == "EmpID")?.Value;
                var user = _dataContext.Users.Where(x => x.EmpID == empId).FirstOrDefault();

                if (user != null)
                {
                    transaction.User = user.Id + "";
                }
                DateTime currentDateTime = DateTime.Now;
                transaction.Date_Time = currentDateTime;
                await _dataContext.SaveChangesAsync();

               
                    WorkOrderInfo woi = new WorkOrderInfo();
                    var workOrder = _dataContext.Transaction.Where(x => x.WO_Lot_Num == transaction.WO_Lot_Num && x.Type=="Packaging" && x.Result != null).Distinct();
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

            float hoursInput = 0;

            if (myBooleanValue)
            {
                var sessionExpirySetting = _dataContext.Settings.FirstOrDefault(s => s.ConfigKey == "SESSION_EXPIRY_TIME");

                if (sessionExpirySetting != null && float.TryParse(sessionExpirySetting.ConfigValue, out float value))
                {
                    hoursInput = value;
                }
                ViewBag.HoursInput = hoursInput;
            }
            else
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
            var transaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();
            if (user != null && user.UserRole == UserRole.Supervisor)
            {
                transaction.Supervisor_Name = user.UserFullName;
                _dataContext.SaveChanges();
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
            SendMessageToUDPclient("R");
            return View();
        }
        public IActionResult WorkOrderBarcodeScan()
        {
            SendMessageToUDPclient("R");
            return View();
        }
        public IActionResult CartonLabelScan(bool udpMsg)
        {
            if (udpMsg)
            {
                // If udpMessage is "N", send it to the socket method
                SendMessageToUDPclient("N");
            }
            return View();
        }
        public IActionResult ProductLabelBarcodeScan()
        {
                workOrderInfo woi = new workOrderInfo();
                var itemmaster = _dataContext.ItemMaster.OrderByDescending(x => x.GTIN).FirstOrDefault();
                var transaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();
                var workOrder = _dataContext.Transaction.Where(x => x.WO_Lot_Num == transaction.WO_Lot_Num && x.Type == "Boxing" && x.Result != null).Distinct();
                woi.totalCount = workOrder.Count();
                woi.passedCount = workOrder.Where(x => x.Result == "Pass").Count();
                woi.failedCount = woi.totalCount - woi.passedCount;
                woi.scannedCount = workOrder.Where(x => x.Rescan_Initated == true).Count();
                woi.workOrderLotNo = transaction.WO_Lot_Num;
                woi.workOrderMfgDate = transaction.WO_Mfg_Date;
                woi.workOrderCatalogNo = transaction.WO_Catalog_Num;
                woi.shelflife = itemmaster.Shelf_Life;
                woi.Carton_Use_By = transaction.Carton_Use_By;
                woi.Calculateuseby = transaction.Calculated_Use_By;
                woi.CartonLotNo = transaction.Carton_Lot_Num;
                woi.ifu = itemmaster.IFU;
                woi.Cartongtin = transaction.Carton_Label_GTIN;
                woi.Dbgtin = itemmaster.GTIN;
                woi.CartonLabelspec = transaction.Carton_Label_Spec;
                woi.Dbspec = itemmaster.Label_Spec;
                woi.DbCatalogNo = itemmaster.Catalog_Num;

                FinalResult result = new FinalResult();
                mismatchess mismatches = new mismatchess();
                LHS lhsdata = new LHS();
                RHS rhsdata = new RHS();

                rhsdata.dbCatalogNo = woi.DbCatalogNo;
                lhsdata.woCatalogNumber = woi.workOrderCatalogNo;
                rhsdata.calculateUseBy = woi.Calculateuseby.ToString();
                lhsdata.cartonUseBy = transaction.Carton_Use_By.ToString();
                lhsdata.woLotNo = transaction.WO_Lot_Num;
                lhsdata.cartonLotNo = woi.CartonLotNo;
                rhsdata.dbLabelSpec = woi.Dbspec;
                lhsdata.cartonLabelSpec = woi.CartonLabelspec;

                result.rhsData = rhsdata;
                result.lhsData = lhsdata;
                result.allMatch = true;

                if (transaction.DB_Catalog_Num != transaction.WO_Catalog_Num || transaction.Calculated_Use_By != transaction.Carton_Use_By || transaction.WO_Lot_Num != transaction.Carton_Lot_Num || transaction.DB_Label_Spec != transaction.Carton_Label_Spec)
                {

                    result.allMatch = false;

                }

                if (!result.allMatch)
                {
                    if (transaction.DB_Catalog_Num != transaction.WO_Catalog_Num)
                    {
                        mismatches.catalogNumMismatch = true;
                        mismatches.rescan_catalog = true;
                    }
                    if (transaction.Calculated_Use_By != transaction.Carton_Use_By)
                    {
                        mismatches.calculatedUseByMismatches = true;
                        woi.cartonMismatch = true;
                    }
                    if (transaction.WO_Lot_Num != transaction.Carton_Lot_Num)
                    {
                        mismatches.lotNoMismatch = true;
                        mismatches.rescan_lotno = true;
                        woi.cartonMismatch = true;
                    }
                    if (transaction.DB_Label_Spec != transaction.Carton_Label_Spec)
                    {
                        mismatches.labelSpecMismatch = true;
                        woi.cartonMismatch = true;

                    }

                }


                if (result.allMatch)
                {
                    transaction.Result = "Pass";
                    SendMessageToUDPclient("P");

                }
                else
                {
                    transaction.Result = "Fail";
                    SendMessageToUDPclient("F");
                }
                var empId = User.Claims.FirstOrDefault(c => c.Type == "EmpID")?.Value;
                var user = _dataContext.Users.Where(x => x.EmpID == empId).FirstOrDefault();

                if (user != null)
                {
                    transaction.User = user.Id + "";
                }
                DateTime currentDateTime = DateTime.Now;
                transaction.Date_Time = currentDateTime;
                _dataContext.SaveChangesAsync();
           
                TempData["woi"] = woi;  
                return View(woi);

        }

        public async Task<IActionResult> FinalResult(bool rescanmsg)
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
                if (transaction.WO_Lot_Num != transaction.Product_Lot_Num || transaction.Carton_Label_Spec != transaction.DB_Label_Spec || transaction.Carton_Use_By != transaction.Calculated_Use_By || transaction.WO_Catalog_Num != transaction.DB_Catalog_Num || transaction.Carton_Lot_Num != transaction.Product_Lot_Num || transaction.Scanned_IFU != transaction.DB_IFU || transaction.Carton_Use_By != transaction.Product_Use_By||transaction.Carton_Label_GTIN != transaction.Product_Label_GTIN)
                {
                    result.allMatch = false;
                }
                if (!result.allMatch)
                {
                   if(transaction.Carton_Label_GTIN != transaction.Product_Label_GTIN)
                    {
                        mismatches.gtinMismatch = true;
                    }
                    
                    if (transaction.WO_Lot_Num != transaction.Product_Lot_Num)
                    {
                        mismatches.lotNoMismatch = true;
                        mismatches.rescan_lotno = true;
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
                        mismatches.rescan_catalog = true;
                    }
                    if (transaction.Carton_Lot_Num != transaction.Product_Lot_Num)
                    {
                        mismatches.LotNumberMisMatch = true;
                    }
                    if (transaction.Scanned_IFU != transaction.DB_IFU)
                    {
                        mismatches.ifumismatches = true;
                        mismatches.rescan_ifu = true;
                    }
                    if (transaction.Carton_Use_By != transaction.Product_Use_By)
                    {
                        mismatches.CalculatedUseByMismatch = true;
                    }
                }

                if (result.allMatch)
                {
                    transaction.Result = "Pass";
                    await SendMessageToUDPclient("P");

                }
                else
                {
                    transaction.Result = "Fail";
                    await SendMessageToUDPclient("F");
                }
                var empId = User.Claims.FirstOrDefault(c => c.Type == "EmpID")?.Value;
                var user = _dataContext.Users.Where(x => x.EmpID == empId).FirstOrDefault();

                if (user != null)
                {
                    transaction.User = user.Id + "";
                }
                DateTime currentDateTime = DateTime.Now;
                transaction.Date_Time = currentDateTime;
                await _dataContext.SaveChangesAsync();

                workOrderInfo woi = new workOrderInfo();
                var workOrder = _dataContext.Transaction.Where(x => x.WO_Lot_Num == transaction.WO_Lot_Num &&x.Type=="Boxing"&& x.Result != null).Distinct();
                woi.totalCount = workOrder.Count();
                woi.passedCount = workOrder.Where(x => x.Result == "Pass").Count();
                woi.failedCount = woi.totalCount - woi.passedCount;
                woi.scannedCount = workOrder.Where(x => x.Rescan_Initated == true).Count();

                result.workOrderInfo = woi;
                result.mismatches = mismatches;
                var objdata = result;

                if (rescanmsg == true)
                {
                    result.cartonscan = true;
                   
                }
                    return  Json(objdata);   
            }
            else
            {
                await SendMessageToUDPclient("E");
                TempData["ErrorMessage"] = "Invalid barcodes. One or both of the scanned barcodes are not valid.";
                return Json(new { success = false, errorMessage = TempData["ErrorMessage"] });
            }
        }
        [HttpPost]
        public IActionResult SaveWorkOrderBarcode(string input,bool udpMsg)
        {
            if (input != null && input.Trim() != "")
            {
                string[] barcodeParts = input.Split('_');
                if (barcodeParts.Length == 4 && barcodeParts[2].Length == 8 && barcodeParts.All(part => !string.IsNullOrEmpty(part.Trim())))
                {

                    Transaction transaction = new Transaction();
                    transaction.WO_Catalog_Num = barcodeParts[0];
                    DateTime date = DateTime.ParseExact(barcodeParts[2], "MMddyyyy", null);
                    transaction.WO_Mfg_Date = date;
                    transaction.WO_Lot_Num = barcodeParts[3];
                    transaction.Date_Time = DateTime.Now;
                    _dataContext.Transaction.Add(transaction);
                    _dataContext.SaveChanges();

                    TempData["WorkOrderLotNo"] = transaction.WO_Lot_Num;
                }
                else
                {
                    SendMessageToUDPclient("E");
                    TempData["ErrorMessage"] = "Work Order Seems To be Invalid. Please retry again";
                    return View("WorkOrderBarcodeScan");
                }
                if (udpMsg)
                {
                    return Json(new { redirectTo = "/Home/CartonLabelScan" });
                }

                else 
                { 
                   return RedirectToAction("CartonLabelScan", "Home");
                }
            }

            else
            {
                // Handle the case when input is "Null" or empty
                SendMessageToUDPclient("E");
                TempData["ErrorMessage"] = "Work Order is Null or Empty. Please retry with a valid input.";
                return View("WorkOrderScan");
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

                    if (transaction.Product_Label_GTIN != null)
                    {
                        var record = new Transaction
                        {
                            WO_Catalog_Num = transaction.WO_Catalog_Num,
                            WO_Mfg_Date = transaction.WO_Mfg_Date,
                            WO_Lot_Num = transaction.WO_Lot_Num,
                            Carton_Label_GTIN = transaction.Carton_Label_GTIN,
                            Carton_Lot_Num = transaction.Carton_Lot_Num,
                            Carton_Use_By = transaction.Carton_Use_By
                        };

                        _dataContext.Transaction.Add(record);
                        await _dataContext.SaveChangesAsync();
                        transaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();
                    }

                    if (input1.Length == 34 || match.Groups[1].Length == 14 || match.Groups[2].Length == 6 || match.Groups[4].Length == 8)
                    {
                        transaction.Carton_Label_GTIN = match.Groups[1].Value;
                        DateTime dateTime = DateTime.ParseExact(match.Groups[2].Value, "yyMMdd", null);
                        transaction.Carton_Use_By = dateTime;
                        transaction.Carton_Lot_Num = match.Groups[4].Value;
                        transaction.Type = "Boxing";

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
                            await SendMessageToUDPclient("E");
                            TempData["ErrorMessage"] = "Carton GTIN value is Invalid";
                            return View("CartonLabelScan");

                        }

                        transaction.Carton_Label_Spec = input2;

                    }
                    else
                    {
                        await SendMessageToUDPclient("E");
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
                    await SendMessageToUDPclient("E");
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
        public async Task<IActionResult>SaveProductLabelBarcode(string input1, string input2, string input3, bool gtinmismatch, bool rescanmsg)
        {

            if (string.IsNullOrEmpty(input2) && string.IsNullOrEmpty(input3) && input1.Length > 34)
            {
                string[] barcodeParts = input1.Split('_');

                if (barcodeParts.Length == 4 && barcodeParts[2].Length == 8 && barcodeParts.All(part => !string.IsNullOrEmpty(part.Trim())))
                {
                    // All parts are non-empty, proceed with saving the data
                    Transaction transaction = new Transaction();
                    transaction.WO_Catalog_Num = barcodeParts[0];
                    DateTime date = DateTime.ParseExact(barcodeParts[2], "MMddyyyy", null);
                    transaction.WO_Mfg_Date = date;
                    transaction.WO_Lot_Num = barcodeParts[3];
                    transaction.Date_Time = DateTime.Now;
                    _dataContext.Transaction.Add(transaction);
                    _dataContext.SaveChanges();

                    TempData["WorkOrderLotNo"] = transaction.WO_Lot_Num;
                    return Json(new
                    {
                        workorder = true,
                        transaction.WO_Catalog_Num,
                        transaction.WO_Mfg_Date,
                        transaction.WO_Lot_Num
                    });
                }
            }
            else if (string.IsNullOrEmpty(input3))
            {
                    
                    string pattern = @"^\d{2}(\d{14})\d{2}(\d{6})(\d{2})(\w+)";
                    Match match = Regex.Match(input1, pattern);

                    var transaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();

                    if (transaction.Product_Label_GTIN != null)
                    {
                        var record = new Transaction
                        {
                            WO_Catalog_Num = transaction.WO_Catalog_Num,
                            WO_Mfg_Date = transaction.WO_Mfg_Date,
                            WO_Lot_Num = transaction.WO_Lot_Num,
                            Carton_Label_GTIN = transaction.Carton_Label_GTIN,
                            Carton_Lot_Num = transaction.Carton_Lot_Num,
                            Carton_Use_By = transaction.Carton_Use_By
                        };

                        _dataContext.Transaction.Add(record);
                        await _dataContext.SaveChangesAsync();
                        transaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();

                    }
                    if (input1.Length == 34 || match.Groups[1].Length == 14 || match.Groups[2].Length == 6 || match.Groups[4].Length == 8)
                    {
                        transaction.Carton_Label_GTIN = match.Groups[1].Value;
                        DateTime dateTime = DateTime.ParseExact(match.Groups[2].Value, "yyMMdd", null);
                        transaction.Carton_Use_By = dateTime;
                        transaction.Carton_Lot_Num = match.Groups[4].Value;
                        transaction.Type = "Boxing";

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
                            await SendMessageToUDPclient("E");
                            TempData["ErrorMessage"] = "Carton GTIN value is Invalid";
                            return View("CartonLabelScan");

                        }

                        transaction.Carton_Label_Spec = input2;
                    }
                    else
                    {
                        await SendMessageToUDPclient("E");
                        TempData["ErrorMessage"] = "Carton Label Input is Invalid Format";
                        return View("CartonLabelScan");
                    }
                    if (transaction != null)
                    {
                        await _dataContext.SaveChangesAsync();
                        TempData["WorkOrderLotNo"] = transaction.WO_Lot_Num;
                    }
                    else
                    {
                        await SendMessageToUDPclient("E");
                        TempData["ErrorMessage"] = "Carton Label Input is Invalid Format";
                        return View("CartonLabelScan");
                    }

                    workOrderInfo woi = new workOrderInfo();
                    var itemmaster = _dataContext.ItemMaster.OrderByDescending(x => x.GTIN).FirstOrDefault();
                    var workOrder = _dataContext.Transaction.Where(x => x.WO_Lot_Num == transaction.WO_Lot_Num && x.Type == "Boxing" && x.Result != null).Distinct();
                    woi.totalCount = workOrder.Count();
                    woi.passedCount = workOrder.Where(x => x.Result == "Pass").Count();
                    woi.failedCount = woi.totalCount - woi.passedCount;
                    woi.scannedCount = workOrder.Where(x => x.Rescan_Initated == true).Count();
                    woi.workOrderLotNo = transaction.WO_Lot_Num;
                    woi.workOrderMfgDate = transaction.WO_Mfg_Date;
                    woi.workOrderCatalogNo = transaction.WO_Catalog_Num;
                    woi.shelflife = itemmaster.Shelf_Life;
                    woi.Carton_Use_By = transaction.Carton_Use_By;
                    woi.Calculateuseby = transaction.Calculated_Use_By;
                    woi.CartonLotNo = transaction.Carton_Lot_Num;
                    woi.ifu = itemmaster.IFU;
                    woi.Cartongtin = transaction.Carton_Label_GTIN;
                    woi.Dbgtin = itemmaster.GTIN;
                    woi.CartonLabelspec = transaction.Carton_Label_Spec;
                    woi.Dbspec = itemmaster.Label_Spec;
                    woi.DbCatalogNo = itemmaster.Catalog_Num;
                    woi.cartonscan = true;

                    FinalResult result = new FinalResult();
                    mismatchess mismatches = new mismatchess();
                    LHS lhsdata = new LHS();
                    RHS rhsdata = new RHS();

                    rhsdata.dbCatalogNo = woi.DbCatalogNo;
                    lhsdata.woCatalogNumber = woi.workOrderCatalogNo;
                    rhsdata.calculateUseBy = woi.Calculateuseby.ToString();
                    lhsdata.cartonUseBy = transaction.Carton_Use_By.ToString();
                    lhsdata.woLotNo = transaction.WO_Lot_Num;
                    lhsdata.cartonLotNo = woi.CartonLotNo;
                    rhsdata.dbLabelSpec = woi.Dbspec;
                    lhsdata.cartonLabelSpec = woi.CartonLabelspec;

                    result.rhsData = rhsdata;
                    result.lhsData = lhsdata;
                    result.allMatch = true;

                    if (transaction.DB_Catalog_Num != transaction.WO_Catalog_Num || transaction.Calculated_Use_By != transaction.Carton_Use_By || transaction.WO_Lot_Num != transaction.Carton_Lot_Num || transaction.DB_Label_Spec != transaction.Carton_Label_Spec)
                    {

                        result.allMatch = false;

                    }

                    if (!result.allMatch)
                    {
                        if (transaction.DB_Catalog_Num != transaction.WO_Catalog_Num)
                        {
                            mismatches.catalogNumMismatch = true;
                            mismatches.rescan_catalog = true;
                        }
                        if (transaction.Calculated_Use_By != transaction.Carton_Use_By)
                        {
                            mismatches.calculatedUseByMismatches = true;
                            woi.cartonMismatch = true;
                        }
                        if (transaction.WO_Lot_Num != transaction.Carton_Lot_Num)
                        {
                            mismatches.lotNoMismatch = true;
                            mismatches.rescan_lotno = true;
                            woi.cartonMismatch = true;
                        }
                        if (transaction.DB_Label_Spec != transaction.Carton_Label_Spec)
                        {
                            mismatches.labelSpecMismatch = true;
                            woi.cartonMismatch = true;

                        }

                    }


                    if (result.allMatch)
                    {
                        transaction.Result = "Pass";
                        SendMessageToUDPclient("P");

                    }
                    else
                    {
                        transaction.Result = "Fail";
                        SendMessageToUDPclient("F");
                    }
                    var empId = User.Claims.FirstOrDefault(c => c.Type == "EmpID")?.Value;
                    var user = _dataContext.Users.Where(x => x.EmpID == empId).FirstOrDefault();

                    if (user != null)
                    {
                        transaction.User = user.Id + "";
                    }
                    DateTime currentDateTime = DateTime.Now;
                    transaction.Date_Time = currentDateTime;
                    _dataContext.SaveChangesAsync();
                    return Json(woi);
              
            }
            else
            {
                string pattern = @"^\d{2}(\d{14})\d{2}(\d{6})(\d{2})(\w+)";
                Match match = Regex.Match(input1, pattern);
                if (!match.Success)
                {
                    await SendMessageToUDPclient("E");
                    return BadRequest(new { errorMessage = "Product Label spec Input is Invalid Format" });
                }
                else
                {
                    Transaction latestTransaction = null; // Declare before the if block

                    if (match.Success)
                    {
                        latestTransaction = _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefault();
                        if (input1.Length == 34 && match.Groups[1].Length == 14 && match.Groups[2].Length == 6 && match.Groups[4].Length == 8)
                        {
                            latestTransaction.Product_Label_GTIN = match.Groups[1].Value;
                            DateTime dateTime = DateTime.ParseExact(match.Groups[2].Value, "yyMMdd", null);
                            latestTransaction.Product_Use_By = dateTime;
                            latestTransaction.Product_Lot_Num = match.Groups[4].Value;
                            latestTransaction.Type = "Boxing";

                            ItemMaster item = _dataContext.ItemMaster.OrderBy(i => i.GTIN).FirstOrDefault();

                            // Assign values from ItemMaster
                            latestTransaction.DB_GTIN = item.GTIN;
                            latestTransaction.DB_Catalog_Num = item.Catalog_Num;
                            DateTime workOrderDT = (DateTime)latestTransaction.WO_Mfg_Date;
                            latestTransaction.Calculated_Use_By = workOrderDT.AddDays((double)item.Shelf_Life);
                            latestTransaction.DB_Label_Spec = item.Label_Spec;
                            latestTransaction.DB_IFU = item.IFU;
                            latestTransaction.Product_Label_Spec = input2;
                            latestTransaction.Scanned_IFU = input3;
                        }
                        else
                        {
                            await SendMessageToUDPclient("E");
                            return BadRequest(new { errorMessage = "Product Label spec Input is Invalid Format" });

                        }
                    }


                    if (latestTransaction != null)
                    {
                        await _dataContext.SaveChangesAsync();
                    }
                }
            }
            return RedirectToAction("FinalResult", new { rescanmsg });
        }

        [HttpPost]
        public async Task<IActionResult> Rescan(bool rescanmsg,bool value)
        {
            var transaction = await _dataContext.Transaction.OrderByDescending(x => x.Transaction_Id).FirstOrDefaultAsync();
            if (rescanmsg)
            {
                transaction.Rescan_Initated = true;
                await _dataContext.SaveChangesAsync();
                await SendMessageToUDPclient("R");
            }
            // Requirement 2: Create a duplicate record with Result "Pass"
            var duplicateTransaction = new Transaction
            {
                WO_Catalog_Num = transaction.WO_Catalog_Num,
                DB_Catalog_Num = transaction.DB_Catalog_Num,
                WO_Mfg_Date = transaction.WO_Mfg_Date,
                Shelf_Life = transaction.Shelf_Life,
                Calculated_Use_By = transaction.Calculated_Use_By,
                Carton_Use_By = transaction.Carton_Use_By,
                Product_Use_By = transaction.Product_Use_By,
                WO_Lot_Num = transaction.WO_Lot_Num,
                Carton_Lot_Num = transaction.Carton_Lot_Num,
                Product_Lot_Num = transaction.Product_Lot_Num,
                DB_GTIN = transaction.DB_GTIN,
                Carton_Label_GTIN = transaction.Carton_Label_GTIN,
                Product_Label_GTIN = transaction.Product_Label_GTIN,
                DB_Label_Spec = transaction.DB_Label_Spec,
                Product_Label_Spec = transaction.Product_Label_Spec,
                Carton_Label_Spec = transaction.Carton_Label_Spec,
                DB_IFU = transaction.DB_IFU,
                Scanned_IFU = transaction.Scanned_IFU,
                User = transaction.User,
                Result = "Pass", // Set the Result column to "Pass"
                Rescan_Initated = false, // Reset the Rescan_Initated column
                Date_Time = DateTime.Now
            };

            _dataContext.Transaction.Add(duplicateTransaction);
            await _dataContext.SaveChangesAsync();

            // Calculate woi and return it to the view
            if (value)
            {
                var woi = new workOrderInfo();
                var workOrder = _dataContext.Transaction.Where(x => x.WO_Lot_Num == transaction.WO_Lot_Num && x.Type == "Boxing" && x.Result != null).Distinct();
                woi.totalCount = workOrder.Count();
                woi.passedCount = workOrder.Where(x => x.Result == "Pass").Count();
                woi.failedCount = woi.totalCount - woi.passedCount;
                woi.scannedCount = workOrder.Where(x => x.Rescan_Initated == true).Count();

                FinalResult result = new FinalResult();
                mismatchess mismatches = new mismatchess();
                LHS lhsdata = new LHS();
                RHS rhsdata = new RHS();

                result.rhsData = rhsdata;
                result.lhsData = lhsdata;
                result.allMatch = true;

                if (transaction.DB_Catalog_Num != transaction.WO_Catalog_Num || transaction.Calculated_Use_By != transaction.Carton_Use_By || transaction.WO_Lot_Num != transaction.Carton_Lot_Num || transaction.DB_Label_Spec != transaction.Carton_Label_Spec)
                {

                    result.allMatch = false;

                }

                if (!result.allMatch)
                {
                    if (transaction.DB_Catalog_Num != transaction.WO_Catalog_Num)
                    {
                        mismatches.catalogNumMismatch = true;
                        mismatches.rescan_catalog = true;
                    }
                    if (transaction.Calculated_Use_By != transaction.Carton_Use_By)
                    {
                        mismatches.calculatedUseByMismatches = true;
                        woi.cartonMismatch = true;
                    }
                    if (transaction.WO_Lot_Num != transaction.Carton_Lot_Num)
                    {
                        mismatches.lotNoMismatch = true;
                        mismatches.rescan_lotno = true;
                        woi.cartonMismatch = true;
                    }
                    if (transaction.DB_Label_Spec != transaction.Carton_Label_Spec)
                    {
                        mismatches.labelSpecMismatch = true;
                        woi.cartonMismatch = true;

                    }

                }


                return Json(woi);
            }
            else
            {
                var woi = new workOrderInfo();
                var workOrder = _dataContext.Transaction.Where(x => x.WO_Lot_Num == transaction.WO_Lot_Num && x.Type == "Packaging"&& x.Result != null).Distinct();
                woi.totalCount = workOrder.Count();
                woi.passedCount = workOrder.Where(x => x.Result == "Pass").Count();
                woi.failedCount = woi.totalCount - woi.passedCount;
                woi.scannedCount = workOrder.Where(x => x.Rescan_Initated == true).Count();
                return Json(woi);
            }
        }


        public IActionResult WorkOrderError()
        {
            return View();
        }

        public async Task SendMessageToUDPclient(String input)
        {
            try
            {
                await WebsocketHandler.writeToUDPSocketConnection(input);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        [HttpPost]
        public IActionResult Shutdown()
        {
            // Create a new process
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.StartInfo = startInfo;
            process.Start();
            // Run a command (for example, "dir" to list files in the current directory)
            process.StandardInput.WriteLine("taskkill /f /im chrome.exe >nul");
            process.StandardInput.WriteLine("shutdown /s");
            process.StandardInput.Close();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();

            //System.Diagnostics.Process.Start("CMD.exe", "");
            //System.Diagnostics.Process.Start("CMD.exe", "shutdown /s");
            return Json(result);

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
