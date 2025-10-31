using System.Collections.Generic;

namespace FamilyMemories.Models.ViewModels.Admin
{
 public class EditUserRolesViewModel
 {
 public string UserId { get; set; }
 public string UserName { get; set; }
 public List<RoleCheckboxViewModel> Roles { get; set; }
 }
}