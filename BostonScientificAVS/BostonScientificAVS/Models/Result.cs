
namespace BostonScientificAVS.Models
{
    public class Result
    {
        public bool allMatch { get; set; }
        public Mismatches mismatches { get; set; }
        public Lhs lhsData { get; set; }
        public Rhs rhsData { get; set; }
        public WorkOrderInfo workOrderInfo { get; set; }

    }

    public class Mismatches
    {
       public bool GTINMismatch { get; set; }
       public bool lotNoMismatch { get; set; }
       public bool labelSpecMismatch { get; set; }
       public bool calculatedUseByMismatch { get; set; }
       public bool catalogNumMismatch { get; set; }
      
    }
  
    public class Rhs
    {
        public string productLabelGTIN { get; set; }
        public string productLotNo { get; set; }
        public string productLabelSpec { get; set; }
        public string productUseBy { get; set; }
        public string workOrderCatalogNo { get; set; }
    }

    public class Lhs    {
        public string dbGTIN { get; set; }
        public string workOrderLotNo { get; set; }

        public string dbLabelSpec { get; set; }

        public string calculatedUseBy { get; set; }
        public string dbCatalogNo { get; set; }
    }

  

    public class WorkOrderInfo
    {
        public int? totalCount { get; set; }
        public int? passedCount { get; set; }
        public int? failedCount { get; set; }
        public int? scannedCount { get; set; }

        public string workOrderLotNo { get; set; }
        public string workOrderCatalogNo { get; set; }

    }

    public class countinfo
    {
        public string Product_Gtin { get; set; }
        public string Db_Gtin { get; set; }
        public string Carton_Gtin { get; set; }
        public string WO_Lot_No { get; set; }
        public string Product_Lot_No { get; set; }
        public string Carton_Spec { get; set; }
        public string Db_Spec { get; set; }
        public string Product_Spec { get; set; }
        public string Carton_Use_By { get; set; }
        public string Calculate_Use_By { get; set; }
        public string Wo_Catalog_Num { get; set; }
        public string Db_Catalog_No { get; set; }
        public string Carton_Lot_No { get; set; }
        public string Db_Ifu { get; set; }
        public string Scanned_Ifu { get; set; }
        public string Pro_Use_By { get; set; }
        public bool GTINMismatch { get; set; }
        public bool LabelMismatch { get; set; }
        public bool IfuMismatch { get; set; }
        public bool DB_GTIN_Mismatch {get;set;}
        public bool LotnoMismatch { get; set; }
        public bool FirstUseby_Mismatch { get; set; }
        public bool SecondUseby_Mismatch { get; set; }
        public bool CatalogMismatch { get; set; }
        public bool LotNoMismatches { get; set; }
        public int? totalcount { get; set; }
        public int? passedCount { get; set; }
        public int? failedCount { get; set; }
        public int? scannedCount { get; set; }

    }

}
