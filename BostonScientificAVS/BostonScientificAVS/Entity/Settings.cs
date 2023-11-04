using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BostonScientificAVS.Entity
{
    public class Settings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string ConfigKey { get; set; } 
        public string? ConfigValue { get; set; }
    }
}
