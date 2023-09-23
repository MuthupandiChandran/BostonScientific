namespace BostonScientificAVS.Models
{
    public class PageStatus
    {
        public Error pageError { get; set; }
        public PageData pageData { get; set; }
    }


    public class PageData
    {
        public string Barcode { get; set; }

    }
    public class Error
    {
        public string errorMsg { get; set; }
    }
}
