using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Entity
{
    public class ItemMaster
    {
        [Key]
        [MaxLength(14)]
        public string GTIN { get; set; }

        [MaxLength(30)]
        public string? Catalog_Num { get; set; }

        public int? Shelf_Life { get; set; }

        [MaxLength(30)]
        public string? Label_Spec { get; set; }

        [MaxLength(30)]
        public string? IFU { get; set; }

        public DateTime? Edit_Date_Time { get; set; } // Change the data type to DateTime

        public string? Edit_By { get; set; }

        public DateTime? Created { get; set; } // Change the data type to DateTime

        public string? Created_by { get; set; }
    }


}