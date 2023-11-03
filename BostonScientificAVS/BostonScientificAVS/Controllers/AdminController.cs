using BostonScientificAVS.DTO;
using BostonScientificAVS.Models;
using BostonScientificAVS.Services;
using Context;
using CsvHelper;
using Entity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Http;

namespace BostonScientificAVS.Controllers
{
    [Authorize(Roles= "Admin")]
    public class AdminController : Controller
    {
        private readonly ItemService _itemService;
        private readonly UserService _userService;
        private DataContext _context;
        public AdminController(ItemService itemService, DataContext context, UserService userService)
        {
            _itemService = itemService;
            _userService = userService;
            _context = context;
        }

        public IActionResult Items()
        {
            return View();
        }
        public IActionResult Settings()
        {
            return View();
        }

        public PartialViewResult RefreshItemsGrid()
        {
            var items = _itemService.getItems();
            return PartialView("_ItemsTable", items);
        }

        [HttpPost]
        public IActionResult UpdateExpiration(float expirationTime)
        {
           
            var result = new
            {
                NewExpirationTime = expirationTime, // Pass the newExpirationTime
                Admin = true // Set this based on your condition           
           };
            return Json(result);
        }

        [HttpPost("/UpdateItem")]
        public ActionResult UpdateItem(SingleItemEdit itemToEdit)
        {
            try
            {
                if (itemToEdit != null)
                {
                    string currentUserName = @User.Identity.Name;
                    _itemService.updateItem(itemToEdit, currentUserName);
                }
                return Ok("Successfully updated item");
            }
            catch (Exception e)
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
                    // Get the "CurrentUserName" session string
                    string currentUserName = @User.Identity.Name;

                    // If the value is not null or empty, proceed with saving the item
                    if (!string.IsNullOrEmpty(currentUserName))
                    {
                        if (!string.IsNullOrEmpty(item.GTIN))
                        {
                            // Check if a record with the same GTIN already exists
                            var existingItem = _context.ItemMaster.FirstOrDefault(u => u.GTIN == item.GTIN);
                            if (existingItem != null)
                            {
                                return Json(new { success = false, message = "GTIN is already in use." });
                            }
                            else
                            {
                                // Assign the current date and time to the "Created" property for new items
                                item.Created = DateTime.Now;
                                item.Created_by = currentUserName;

                                // Call the service to save the new item
                                _itemService.saveNewItem(item);
                                return Json(new { success = true });
                            }
                        }
                    }
                    else
                    {
                        return BadRequest("User not authenticated or session expired.");
                    }
                }
                return BadRequest("Item data is null.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Ok("error while adding item");
            }
        }



        [HttpPost("/DeleteItem")]

        public async Task<IActionResult>DeleteItem (ItemMaster item)
        {
            try
            {
                var deleterecord = await _context.ItemMaster.FirstOrDefaultAsync(u => u.GTIN == item.GTIN);
                if(deleterecord!=null)
                {
                    _context.ItemMaster.Remove(deleterecord);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "User deleted successfully" });
                }

                else
                {
                    return Json(new { success = false, message = "User not found" });
                }
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = "Error deleting user: " + ex.Message });

            }
        }




        // MVC Controller action method to handle file upload
        [HttpPost("/uploadItemsExcel")]
        public async Task<string> ImportCSV(IFormFile file)
        {
            try
            {
                string currentUserName = @User.Identity.Name;
                await _itemService.importCsv(file,currentUserName);
                return "file upload Successfully";
            }
            catch (Exception e)
            {
                return "file upload failed";
            }


        }

        [HttpGet]
        public IActionResult DownloadCSVItems()
        {
            // Create an empty record with only header column values
            var headerRecord = new ItemMasterCsvHeader
            {
                GTIN = "GTIN",
                Catalog_Num = "Catalog_Num",
                Shelf_Life = "Shelf_Life",
                Label_Spec = "Label_Spec",
                IFU = "IFU",
                Edit_Date_Time = "Edit_Date_Time",
                Edit_By = "Edit_By",
                Created = "Created",
                Created_by = "Created_by"
                // Add more header column values as needed
            };

            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecord(headerRecord);
                csv.NextRecord(); // Write a newline to complete the header row
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream, "text/csv", "Users.csv");
        }

        [HttpGet]
        public IActionResult Users()
        {
            /* ApplicationUser user = new ApplicationUser()*/

            var employees = _context.Users.ToList();
            List<ApplicationUser> users = new List<ApplicationUser>();
            if (employees != null)
            {
                foreach (var user in employees)
                {
                    var ApplicationUser = new ApplicationUser()
                    {
                        EmpID = user.EmpID,
                        UserFullName = user.UserFullName,
                        UserRole = user.UserRole

                    };
                    users.Add(ApplicationUser);

                }
                return View(users);
            }

            return View(users);

        }

        [HttpPost]
        public async Task<JsonResult> SaveUser(ApplicationUser user)
        {
            var existingRecord = await _context.Users.FirstOrDefaultAsync(u => u.EmpID == user.EmpID);

            if (existingRecord != null)
            {
                existingRecord.UserFullName = user.UserFullName;
                existingRecord.UserRole = user.UserRole;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Updated successfully!" });
            }
            else
            {
                var newRecord = new ApplicationUser()
                {
                    EmpID = user.EmpID,
                    UserFullName = user.UserFullName,
                    UserRole = user.UserRole
                };
                _context.Users.Add(newRecord);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
        }

        [HttpPost]
        public async Task<IActionResult>DeleteUser(ApplicationUser user)
        {
            try
            {
                var existingRecord = await _context.Users.FirstOrDefaultAsync(u => u.EmpID == user.EmpID);
                if (existingRecord != null)
                {
                    _context.Users.Remove(existingRecord);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "User deleted successfully" });
                }
                
                else
                {
                    return Json(new { success = false, message = "User not found" });
                }
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = "Error deleting user: " + ex.Message });
            }
        }



        public async Task importCsv(IFormFile file)
        {

            try
            {
                await _userService.importCsv(file);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
         
        }


        [HttpGet]
        public IActionResult DownloadCSV()
        {
            var headerRecord = new ApplicationUsersCsvHeader
            {
                EmpID = "EmpID",
                UserFullName = "UserFullName",
                UserRole = "UserRole"
            };

            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecord(headerRecord);
                csv.NextRecord();
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream, "text/csv", "users.csv");
        }
        [HttpGet]
        public IActionResult TransactionTable(string startDate, string endDate)
        {        
            var records = _context.Transaction.ToList();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                // Convert the submitted date strings to DateTime objects
                if (DateTime.TryParse(startDate, out DateTime startSearchDate) && DateTime.TryParse(endDate, out DateTime endSearchDate))
                {
                    // Filter records within the specified date range
                    var filteredRecords = records.Where(t =>
                        t.Date_Time != null &&
                        DateTime.ParseExact(t.Date_Time, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture) >= startSearchDate &&
                        DateTime.ParseExact(t.Date_Time, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture) <= endSearchDate).ToList();

                    // Display the filtered records in the view
                    return View("Transaction", filteredRecords);
                }
            }
            else
            {
                // Filter records for today by default
                DateTime today = DateTime.Today;
                records = records.Where(t =>
                    t.Date_Time != null &&
                    DateTime.ParseExact(t.Date_Time, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture).Date == today).ToList();
            }


            return View("Transaction", records); // Specify the view name and pass the records
        }



        [HttpGet]
        public IActionResult ExportTransactionData(string hiddenStartDate, string hiddenEndDate)
        {
            var records = _context.Transaction.ToList();

            if (!string.IsNullOrEmpty(hiddenStartDate) && !string.IsNullOrEmpty(hiddenEndDate))
            {
                // Define the correct date format "dd-MM-yyyy HH:mm:ss" to parse the dates
                string dateFormat = "dd-MM-yyyy HH:mm:ss";
                var startSearchDate = DateTime.ParseExact(hiddenStartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var endSearchDate = DateTime.ParseExact(hiddenEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                records = records.Where(t =>
                    t.Date_Time != null &&
                    DateTime.ParseExact(t.Date_Time, dateFormat, CultureInfo.InvariantCulture) >= startSearchDate &&
                    DateTime.ParseExact(t.Date_Time, dateFormat, CultureInfo.InvariantCulture) <= endSearchDate).ToList();
            }

            // Create a CSV content string with headers and data
            var csvData = new StringBuilder();
            csvData.AppendLine("Transaction Id,Product Label GTIN,Carton Label GTIN,DB GTIN,WO Lot No,Product Lot No,Carton Lot No,WO Catalog No,DB Catalog No,ShelfLife,WO Mfg Date,Calculated Use By,Product Use By,Carton Use By,DB Label Spec,Product Label Spec,Carton Label Spec,DB IFU,Scanned IFU,User,Date Time,Rescan Initiated,Result,Failure Reason,Supervisor Name");

            foreach (var record in records)
            {
                csvData.AppendLine($"{record.Transaction_Id},{record.Product_Label_GTIN},{record.Carton_Label_GTIN},{record.DB_GTIN},{record.WO_Lot_Num},{record.Product_Lot_Num},{record.Carton_Lot_Num},{record.WO_Catalog_Num},{record.DB_Catalog_Num},{record.Shelf_Life},{record.WO_Mfg_Date},{record.Calculated_Use_By},{record.Product_Use_By},{record.Carton_Use_By},{record.DB_Label_Spec},{record.Product_Label_Spec},{record.Carton_Label_Spec},{record.DB_IFU},{record.Scanned_IFU},{User.Identity.Name},{record.Date_Time},{record.Rescan_Initated},{record.Result},{record.Failure_Reason},{record.Supervisor_Name}");
            }

            // Return the CSV file as a response
            var fileName = "ExportedData.csv";
            var contentDisposition = new ContentDisposition
            {
                FileName = fileName,
                Inline = false
            };
            Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

            // Convert the CSV content to a byte array and return as a file
            var csvBytes = Encoding.UTF8.GetBytes(csvData.ToString());
            return File(csvBytes, "text/csv");
        }
    }
}
