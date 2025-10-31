using System.ComponentModel.DataAnnotations;

namespace FamilyMemories.Models.ViewModels.Admin
{
 public class RoleEditViewModel
 {
 public string Id { get; set; }

 [Required]
 [Display(Name = "Role name")]
 public string Name { get; set; }

 [Display(Name = "Description")]
 public string Description { get; set; }
 }
}