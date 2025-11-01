using FamilyMemories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace FamilyMemories.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class RoleManagerController : Controller
	{
		private readonly RoleManager<ApplicationRole> _roleManager;

		public RoleManagerController(RoleManager<ApplicationRole> roleManager)
		{
			_roleManager = roleManager;
		}

		public async Task<IActionResult> Index()
		{
			var roles = _roleManager.Roles.ToList();
			return View(roles);
		}

		// 編輯角色權限
		public async Task<IActionResult> EditPermissions(string id)
		{
			var role = await _roleManager.FindByIdAsync(id);
			if (role == null)
			{
				return NotFound();
			}

			ViewBag.RoleName = role.Name;
			ViewBag.RoleId = role.Id;
			ViewBag.CurrentPermissions = role.RolePermissions;
			ViewBag.AllPermissions = Enum.GetValues(typeof(Permission)).Cast<Permission>().ToList();

			return View(role);
		}

		// 儲存角色權限
		[HttpPost]
		public async Task<IActionResult> EditPermissions(string id, List<Permission> permissions)
		{
			var role = await _roleManager.FindByIdAsync(id);
			if (role == null)
			{
				return NotFound();
			}

			role.RolePermissions = permissions ?? new List<Permission>();
			var result = await _roleManager.UpdateAsync(role);

			if (result.Succeeded)
			{
				TempData["SuccessMessage"] = $"角色 {role.Name} 的權限已更新";
				return RedirectToAction(nameof(Index));
			}

			TempData["ErrorMessage"] = "更新失敗";
			return RedirectToAction(nameof(EditPermissions), new { id });
		}
	}
}
// ...existing code from Controllers/Admin/RoleManagerController.cs will be moved here...