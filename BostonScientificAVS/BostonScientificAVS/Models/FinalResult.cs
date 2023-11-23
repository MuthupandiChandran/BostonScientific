namespace BostonScientificAVS.Models
{
    public class FinalResult
    {
        public bool allMatch { get; set; }
        public mismatchess mismatches { get; set; }
        public LHS lhsData { get; set; }
        public RHS rhsData { get; set; }
        public workOrderInfo workOrderInfo { get; set; }
        public bool cartonscan { get; set; }
    }

    public class mismatchess
    {
        public bool lotNoMismatch { get; set; }
        public bool labelSpecMismatch { get; set; }
        public bool CalculatedUseByMismatch { get; set; }
        public bool gtinMismatch { get; set; }
        public bool catalogNumMismatch { get; set; }
        public bool LotNumberMisMatch { get; set; }
        public bool ifumismatches { get; set; }
        public bool calculatedUseByMismatches { get; set; }
        public bool rescan_ifu { get; set; }
        public bool rescan_catalog { get; set; }
        public bool rescan_lotno { get; set; }

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

    public class workOrderInfo
    {
        public int? totalCount { get; set; }
        public int? passedCount { get; set; }
        public int? failedCount { get; set; }
        public int? scannedCount { get; set; }

        public string workOrderLotNo { get; set; }
        public string workOrderCatalogNo { get; set; }
        public DateTime? workOrderMfgDate { get; set; }
        public int? shelflife { get; set; }
        public DateTime? Carton_Use_By { get; set; }
        public string CartonLotNo { get; set; }
        public string ifu { get; set; }
        public string Cartongtin { get; set; }
        public string Dbgtin { get; set; }
        public string CartonLabelspec { get; set; }
        public string Dbspec { get; set; }
        public string DbCatalogNo { get; set; }
        public DateTime? Calculateuseby { get; set; }
        public bool cartonMismatch { get; set; }
        public bool cartonscan { get; set; }

    }
}
