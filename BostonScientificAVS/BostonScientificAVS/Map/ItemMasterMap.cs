using CsvHelper.Configuration;
using Entity;

namespace BostonScientificAVS.Models
{
    

    public class ItemMasterMap : ClassMap<ItemMaster>
    {
        public ItemMasterMap()
        {
            // Map CSV columns to model properties
            Map(m => m.GTIN).Index(0);
            Map(m => m.Catalog_Num).Index(1);
            Map(m => m.Shelf_Life).Index(2);
            Map(m => m.Label_Spec).Index(3);
            Map(m => m.IFU).Index(4);
            Map(m => m.Edit_Date_Time).Index(5);
            Map(m => m.Edit_By).Index(6);
            Map(m => m.Created).Index(7);
            Map(m => m.Created_by).Index(8);
            // Add mappings for other properties as needed
        }
    }
}
