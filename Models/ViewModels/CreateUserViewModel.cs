using System.ComponentModel.DataAnnotations;

namespace FamilyMemories.Models.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "帳號")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "電子郵件")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "姓名")]
        public string? FullName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "啟用")]
        public bool IsActive { get; set; } = true;
    }
}
