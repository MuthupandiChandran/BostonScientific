using BostonScientificAVS.DTO;
using Context;
using Entity;
using CsvHelper;
using System.Globalization;
using BostonScientificAVS.Models;

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

        public void updateItem(SingleItemEdit updatedItem)
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
                    case "Edit_Date_Time":
                        itemToUpdate.Edit_Date_Time = updatedItem.updated_value;
                        break;
                    case "Edit_By":
                        itemToUpdate.Edit_By = updatedItem.updated_value;
                        break;
                    case "Created":
                        itemToUpdate.Created = updatedItem.updated_value;
                        break;
                    case "Created_by":
                        itemToUpdate.Created_by = updatedItem.updated_value;
                        break;
                }
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

        public async Task importCsv(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // Read the contents of the CSV file
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    // Create a CSV reader
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
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
                                existingRecord.Created = record.Created;
                                existingRecord.Edit_Date_Time = record.Edit_Date_Time;
                                existingRecord.Edit_By = record.Edit_By;
                                // Update other properties as needed
                            }
                            else
                            {
                                // Insert new record
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
