﻿
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Entity
{
    //[Index(nameof(EmpID), IsUnique = true)]
    public class ApplicationUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "EmpID is required")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]  
        public string EmpID { get; set; }
        [Required(ErrorMessage = "UserFullName is required")]
        [RegularExpression("^[A-Za-z]+$", ErrorMessage = "UserFullName should only contain alphabets")]
        public string UserFullName { get; set; }
        [Required]
        public UserRole UserRole { get; set; }

    }

    public enum UserRole
    {
        Admin =1,
        Supervisor =2,
        Operator =3
    }


}
