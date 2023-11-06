using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BostonScientificAVS.Models
{
    public class ItemMasterCsvHeader
    {
        public string GTIN { get; set; }
        public string? Catalog_Num { get; set; }
        public string? Shelf_Life { get; set; }
        public string? Label_Spec { get; set; }
        public string? IFU { get; set; }
        public string? Edit_Date_Time { get; set; }
        public string? Edit_By { get; set; }
        public string? Created { get; set; }
        public string? Created_by { get; set; }
    }

    public class ApplicationUsersCsvHeader
    {

        public string EmpID { get; set; }

        public string UserFullName { get; set; }

        public string UserRole { get; set; }
    }

    public class settings
    {
        public string? sessiontime { get; set; }
    }



}
