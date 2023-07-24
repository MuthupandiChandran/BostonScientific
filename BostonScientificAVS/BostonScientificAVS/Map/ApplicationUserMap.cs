using BostonScientificAVS.Models;
using CsvHelper.Configuration;

namespace BostonScientificAVS.Map
{
    public class ApplicationUserMap: ClassMap<ApplicationUser>
    {

        public ApplicationUserMap()
        {
            Map(m => m.EmpID).Index(0);
            Map(m => m.UserFullName).Index(1);
            Map(m => m.UserRole).Index(2);

        }




    }
}
