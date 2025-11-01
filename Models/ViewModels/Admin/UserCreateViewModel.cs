using System.ComponentModel.DataAnnotations;

namespace FamilyMemories.Models.ViewModels.Admin
{
    public class UserCreateViewModel
    {
        [Required]
        [Display(Name = "帳號")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "姓名")]
        public string? FullName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "確認密碼")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "啟用")]
        public bool IsActive { get; set; } = true;
    }
}
