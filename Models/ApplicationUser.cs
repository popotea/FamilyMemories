using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Text.Json;

namespace FamilyMemories.Models
{
    public class ApplicationUser : IdentityUser
    {
        // 可以在此加入額外的使用者屬性
        // 例如：全名、生日、頭像等
    public string? FullName { get; set; }
        public DateTime? BirthDate { get; set; }
    public string? AvatarPath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true; // 使用者是否啟用

    public virtual ICollection<Memory>? Memories { get; set; }

        /// <summary>
        /// 個人權限（可額外分配，優先於角色權限）
        /// </summary>
        [PersonalData]
        public List<Permission> UserPermissions { get; set; } = new List<Permission>();
        private string _userPermissionsJson
        {
            get => JsonSerializer.Serialize(UserPermissions ?? new List<Permission>());
            set => UserPermissions = string.IsNullOrWhiteSpace(value) || value == "null" ? new List<Permission>() : JsonSerializer.Deserialize<List<Permission>>(value) ?? new List<Permission>();
        }

        /// <summary>
        /// 權限 JSON 字串（供前端顯示用）
        /// </summary>
        public string UserPermissionsJson => JsonSerializer.Serialize(UserPermissions ?? new List<Permission>());
    }
}
