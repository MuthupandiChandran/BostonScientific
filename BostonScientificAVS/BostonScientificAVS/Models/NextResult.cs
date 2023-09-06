﻿namespace BostonScientificAVS.Models
{
    public class NextResult
    {
        public bool allMatch { get; set; }
        public Mismatchess mismatches { get; set; }
        public Lhss lhsData { get; set; }
        public Rhss rhsData { get; set; }
        public WorkOrderInfo workOrderInfo { get; set; }

        public bool udpMessage { get; set; }

    }

    public class Mismatchess
    {
        public bool GTINMismatch { get; set; }
        public bool lotNoMismatch { get; set; }
        public bool labelSpecMismatch { get; set; }
        public bool calculatedUseByMismatch { get; set; }
        public bool catalogNumMismatch { get; set; }

    }

    public class Rhss
    {
        public string productLabelGTIN { get; set; }
        public string productLotNo { get; set; }
        public string productLabelSpec { get; set; }
        public string productUseBy { get; set; }
        public string workOrderCatalogNo { get; set; }
    }

    public class Lhss
    {
        public string dbGTIN { get; set; }
        public string workOrderLotNo { get; set; }

        public string dbLabelSpec { get; set; }

        public string calculatedUseBy { get; set; }
        public string dbCatalogNo { get; set; }
    }



    public class WorkOrderInfoo
    {
        public int? totalCount { get; set; }
        public int? passedCount { get; set; }
        public int? failedCount { get; set; }
        public int? scannedCount { get; set; }

        public string workOrderLotNo { get; set; }

    }

}
