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

namespace BostonScientificAVS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ItemService _itemService;
        private DataContext _context;
        public HomeController(ILogger<HomeController> logger, DataContext context, ItemService service)
        {
            _logger = logger;
            _context = context;
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
            return View();
        }

        public PartialViewResult RefreshItemsGrid()
        {
            var items = _itemService.getItems();
            return PartialView("_ItemsTable", items);
        }


        [HttpPost("/UpdateItem")]
        public ActionResult UpdateItem(SingleItemEdit itemToEdit)
        {
            try
            {
                if (itemToEdit != null)
                {
                    _itemService.updateItem(itemToEdit);
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
                    _itemService.saveNewItem(item);
                }
                return Ok("success");
            }
            catch (Exception e)
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
                Created_by = "Created_By"
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
            return File(memoryStream, "text/csv", "users.csv");
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
        public JsonResult SaveUser(ApplicationUser user)
        {

            var existingRecord = _context.Users.FirstOrDefault(u => u.EmpID == user.EmpID);
            if (existingRecord != null)
            {
                existingRecord.UserFullName = user.UserFullName;
                existingRecord.UserRole = user.UserRole;
                _context.SaveChanges();
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
                _context.SaveChanges();
                return Json(new { success = true });
            }
        }


        public async Task importCsv(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<ApplicationUserMap>();
                        var records = csv.GetRecords<ApplicationUser>().ToList();

                        foreach (var record in records)
                        {
                            var existingRecord = _context.Users.FirstOrDefault(x => x.EmpID == record.EmpID);

                            if (existingRecord != null)
                            {

                                existingRecord.EmpID = record.EmpID;
                                existingRecord.UserFullName = record.UserFullName;
                                existingRecord.UserRole = record.UserRole;

                            }
                            else
                            {

                                _context.Users.Add(record);
                            }
                        }


                        await _context.SaveChangesAsync();

                    }
                }
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


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}