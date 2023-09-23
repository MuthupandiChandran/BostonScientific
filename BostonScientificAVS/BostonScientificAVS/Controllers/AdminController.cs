﻿using BostonScientificAVS.DTO;
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
                        // Assign the current user name to the "Created_by" property only if it's a new item
                        if (!string.IsNullOrEmpty(item.GTIN))
                        {
                            // Assign the current date and time to the "Created" property for new items
                            item.Created = DateTime.Now;
                            item.Created_by = currentUserName;
                            //if (item.Edit_Date_Time == default(DateTime))
                            //{
                            //    item.Edit_Date_Time = null;
                            //}

                            _itemService.saveNewItem(item);
                            return Ok("success");
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
                IFU = "IFU"
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
        public IActionResult TransactionTable(string search)
        {
            DateTime today = DateTime.Today;
            ViewBag.SearchDate = search;
            string userFullName = @User.Identity.Name;
            ViewBag.UserFullName = userFullName;

            var records = _context.Transaction.ToList(); // Fetch all records to memory

            if (!string.IsNullOrEmpty(search))
            {
                var searchDate = DateTime.ParseExact(search, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var formattedSearchDate = searchDate.ToString("dd-MM-yyyy");

                records = records.Where(t => t.Date_Time != null &&
                                              t.Date_Time.StartsWith(formattedSearchDate)).ToList();
            }
            else
            {
                records = records.Where(t => t.Date_Time != null && t.Date_Time.Contains(today.ToString("dd-MM-yyyy"))).ToList();
            }

            return View("Transaction", records); // Specify the view name and pass the records
        }
    }
}
