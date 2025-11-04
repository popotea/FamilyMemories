using FamilyMemories.Models;
using FamilyMemories.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
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
			var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();
			return View(users);
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CreateUserViewModel model)
		{
			if (!ModelState.IsValid) return View(model);

			var user = new ApplicationUser
			{
				UserName = model.UserName,
				Email = model.Email,
				FullName = model.FullName ?? model.UserName,
				EmailConfirmed = true,
				IsActive = model.IsActive
			};

			var result = await _userManager.CreateAsync(user, model.Password);
			if (result.Succeeded)
			{
				TempData["SuccessMessage"] = "使用者建立成功";
				return RedirectToAction(nameof(Index));
			}

			foreach (var err in result.Errors)
				ModelState.AddModelError(string.Empty, err.Description);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(string id)
		{
			if (string.IsNullOrEmpty(id)) return BadRequest();

			var user = await _userManager.FindByIdAsync(id);
			if (user == null) return NotFound();

			// Prevent deleting yourself
			if (string.Equals(user.UserName, User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
			{
				TempData["ErrorMessage"] = "不能刪除目前登入的使用者。";
				return RedirectToAction(nameof(Index));
			}

			// If user is in Admin role, ensure there is at least one other Admin
			if (await _userManager.IsInRoleAsync(user, "Admin"))
			{
				var admins = await _userManager.GetUsersInRoleAsync("Admin");
				if (admins == null || admins.Count <= 1)
				{
					TempData["ErrorMessage"] = "系統中必須至少保留一位 Admin，無法刪除此 Admin。";
					return RedirectToAction(nameof(Index));
				}
			}

			var result = await _userManager.DeleteAsync(user);
			if (result.Succeeded)
			{
				TempData["SuccessMessage"] = "使用者已刪除";
			}
			else
			{
				TempData["ErrorMessage"] = string.Join(';', result.Errors.Select(e => e.Description));
			}

			return RedirectToAction(nameof(Index));
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