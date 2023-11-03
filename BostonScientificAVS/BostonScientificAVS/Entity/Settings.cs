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
        [MaxLength(8)]
        public string Key { get; set; } 
        public string? Value { get; set; }
    }
}
