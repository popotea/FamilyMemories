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
	public class UserManagerController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public UserManagerController(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<IActionResult> Index()
		{
			var users = _userManager.Users.ToList();
			return View(users);
		}

		// 編輯使用者權限
		public async Task<IActionResult> EditPermissions(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}

			ViewBag.UserName = user.UserName;
			ViewBag.UserId = user.Id;
			ViewBag.CurrentPermissions = user.UserPermissions;
			ViewBag.AllPermissions = Enum.GetValues(typeof(Permission)).Cast<Permission>().ToList();

			return View(user);
		}

		// 儲存使用者權限
		[HttpPost]
		public async Task<IActionResult> EditPermissions(string id, List<Permission> permissions)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}

			user.UserPermissions = permissions ?? new List<Permission>();
			var result = await _userManager.UpdateAsync(user);

			if (result.Succeeded)
			{
				TempData["SuccessMessage"] = $"使用者 {user.UserName} 的權限已更新";
				return RedirectToAction(nameof(Index));
			}

			TempData["ErrorMessage"] = "更新失敗";
			return RedirectToAction(nameof(EditPermissions), new { id });
		}
	}
}
// ...existing code from Controllers/Admin/UserManagerController.cs will be moved here...