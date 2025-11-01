using System.Collections.Generic;

namespace FamilyMemories.Models.ViewModels.Admin
{
 public class UserRolesListViewModel
 {
 public string UserId { get; set; }
 public string UserName { get; set; }
 public string Email { get; set; }
 public IList<string> Roles { get; set; }
 public bool IsActive { get; set; }
 }
}