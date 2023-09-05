
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

    }

    public class countinfo
    {
        public string Product_Gtin { get; set; }
        public string Db_Gtin { get; set; }
        public bool GTINMismatch { get; set; }
        public int? totalcount { get; set; }
        public int? passedCount { get; set; }
        public int? failedCount { get; set; }
        public int? scannedCount { get; set; }

    }
}
