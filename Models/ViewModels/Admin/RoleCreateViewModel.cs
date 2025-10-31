using System.ComponentModel.DataAnnotations;

namespace FamilyMemories.Models.ViewModels.Admin
{
 public class RoleCreateViewModel
 {
 [Required]
 [Display(Name = "Role name")]
 public string Name { get; set; }

 [Display(Name = "Description")]
 public string Description { get; set; }
 }
}