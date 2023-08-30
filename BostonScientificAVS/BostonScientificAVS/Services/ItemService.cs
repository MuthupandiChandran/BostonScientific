using BostonScientificAVS.DTO;
using Context;
using Entity;
using CsvHelper;
using System.Globalization;
using BostonScientificAVS.Models;
using CsvHelper.Configuration;

namespace BostonScientificAVS.Services
{
    public class ItemService
    {
        private readonly DataContext _context;
        public ItemService(DataContext context)
        {
            _context = context;
        }

        public List<ItemMaster> getItems()
        {
            return _context.ItemMaster.ToList();
        }

        public void updateItem(SingleItemEdit updatedItem,string editBy)
        {
            if (updatedItem.gtin_value != null)
            {
                var itemToUpdate = _context.ItemMaster.FirstOrDefault(x => x.GTIN == updatedItem.gtin_value);
                switch (updatedItem.column_name)
                {
                    case "GTIN":
                        itemToUpdate.GTIN = updatedItem.updated_value;
                        break;
                    case "Catalog_Num":
                        itemToUpdate.Catalog_Num = updatedItem.updated_value;
                        break;
                    case "Shelf_Life":
                        itemToUpdate.Shelf_Life = int.Parse(updatedItem.updated_value);
                        break;
                    case "Label_Spec":
                        itemToUpdate.Label_Spec = updatedItem.updated_value;
                        break;
                    case "IFU":
                        itemToUpdate.IFU = updatedItem.updated_value;
                        break;
                    //case "Edit_Date_Time":
                    //    // Set the "Edit_Date_Time" property to the current date and time
                    //    itemToUpdate.Edit_Date_Time = DateTime.Now; // Or use DateTime.Now if the property type is DateTime
                        //break;
                    //case "Edit_By":
                    //    itemToUpdate.Edit_By = updatedItem.updated_value;
                    //    break;            

                }
                itemToUpdate.Edit_By = editBy;
                itemToUpdate.Edit_Date_Time = DateTime.Now;
                _context.SaveChanges();
            }


          
            
            //if (itemToUpdate != null)
            //{
            //    foreach(var col in itemToUpdate)
            //    {
            //        if (col = column)
            //    }
            //}

        }

        public void saveNewItem(ItemMaster item)
        {
            if (item != null)
            {
                _context.ItemMaster.Add(item);
                _context.SaveChanges();
            }
        }

        public async Task importCsv(IFormFile file, string currentUserName)
        {         
            if (file != null && file.Length > 0)
            {               
                // Read the contents of the CSV file
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    // Create a CSV reader with CsvConfiguration and set MissingFieldFound to null
                    var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        MissingFieldFound = null
                    };
                    using (var csv = new CsvReader(reader, csvConfig))
                    {
                        // Map the CSV columns to your model properties
                        csv.Context.RegisterClassMap<ItemMasterMap>();

                        // Read the CSV records
                        var records = csv.GetRecords<ItemMaster>().ToList();

                        foreach (var record in records)
                        {
                            var existingRecord = _context.ItemMaster.FirstOrDefault(x => x.GTIN == record.GTIN);

                            if (existingRecord != null)
                            {
                                // Update existing record
                                existingRecord.Created_by = record.Created_by;
                                existingRecord.Catalog_Num = record.Catalog_Num;
                                existingRecord.Shelf_Life = record.Shelf_Life;
                                existingRecord.Label_Spec = record.Label_Spec;
                                existingRecord.IFU = record.IFU;
                                // Update other properties as needed
                            }
                            else
                            {
                                // Insert new record
                                record.Created_by = currentUserName; // Set a default value for Created_by
                                record.Created = DateTime.Now; // Set the current date and time for Created
                                _context.ItemMaster.Add(record);
                            }
                        }

                        // Save changes to the database
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }


    }
}
