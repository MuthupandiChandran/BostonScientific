using BostonScientificAVS.Map;
using BostonScientificAVS.Models;
using Context;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;

namespace BostonScientificAVS.Services
{
    public class UserService
    {
        private readonly DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
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

    }
}
