using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entity
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Transaction_Id { get; set; }
        [MaxLength(14)]
        public string? Product_Label_GTIN { get; set; }
        public string? Carton_Label_GTIN { get; set; }
        [MaxLength(14)]
        public string? DB_GTIN { get; set; }
        [MaxLength(30)]
        public string? WO_Lot_Num { get; set; }
        [MaxLength(30)]
        public string? Product_Lot_Num { get; set; }
        [MaxLength(30)]
        public string? Carton_Lot_Num { get; set; }
        [MaxLength(30)]
        public string? WO_Catalog_Num { get; set; }
        [MaxLength(30)]
        public string? DB_Catalog_Num { get; set; }
        [MaxLength(4)]
        public int Shelf_Life { get; set; }
        public DateTime? WO_Mfg_Date { get; set; }
        public DateTime? Calculated_Use_By { get; set; }
        public DateTime? Product_Use_By { get; set; }
        public string? Carton_Use_By { get; set; }
        [MaxLength(30)]
        public string? DB_Label_Spec { get; set; }
        [MaxLength(30)]
        public string? Product_Label_Spec { get; set; }
        [MaxLength(30)]
        public string? Carton_Label_Spec { get; set; }
        [MaxLength(30)]
        public string? DB_IFU { get; set; }
        [MaxLength(30)]
        public string? Scanned_IFU { get; set; }
        [MaxLength(30)]
        public string? User { get; set; }
        public string? Date_Time { get; set; }
        public Boolean Rescan_Initated { get; set; }
        [MaxLength(10)]
        public string? Result { get; set; }
        [MaxLength(30)]
        public string? Failure_Reason { get; set; }

    }
}
