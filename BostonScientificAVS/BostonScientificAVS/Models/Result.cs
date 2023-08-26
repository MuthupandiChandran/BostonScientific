
namespace BostonScientificAVS.Models
{
    public class Result
    {
        public bool allMatch { get; set; }
        public Mismatches mismatches { get; set; }
        public Lhs lhsData { get; set; }
        public LHS lhsdata { get; set; }
        public RHS rhsdata { get; set; }
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
       public bool LotNumberMisMatch { get; set; }
     public bool ifumismatches { get; set; }
        public bool CalculatedUseByMismatch { get; set; }
        public bool gtinmismatches { get; set; }

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

    public class LHS
    {
        public string cartonGTIN { get; set; }
        public string woLotNo { get; set; }
        public string cartonLabelSpec { get; set; }
        public string cartonUseBy { get; set; }
        public string woCatalogNumber { get; set; }
        public string cartonLotNo { get; set; }
        public string scannedIFU { get; set; }
    }

    public class RHS
    {
        public string dbGTIN { get; set; }
        public string productLabelGTIN { get; set; }
        public string productLotNo { get; set; }
        public string dbLabelSpec { get; set; }
        public string calculateUseBy { get; set; }
        public string dbCatalogNo { get; set; }
        public string dbIFU { get; set; }
        public string productUseBy { get; set; }


    }

    public class WorkOrderInfo
    {
        public int? totalCount { get; set; }
        public int? passedCount { get; set; }
        public int? failedCount { get; set; }
        public int? scannedCount { get; set; }

        public string workOrderLotNo { get; set; }

    }
}
