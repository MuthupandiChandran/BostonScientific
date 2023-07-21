using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BostonScientificAVS.Models
{
    public class CsvHeader
    {
      
        public string EmpID { get; set; }
        
        public string UserFullName { get; set; }
        
        public string UserRole { get; set; }
    }
}
