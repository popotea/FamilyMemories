using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace FamilyMemories.Models
{
    public class ApplicationRole : IdentityRole
    {
    public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 角色擁有的權限
        /// </summary>
        [PersonalData]
        public List<Permission> RolePermissions { get; set; } = new List<Permission>();
        private string _rolePermissionsJson
        {
            get => JsonSerializer.Serialize(RolePermissions ?? new List<Permission>());
            set => RolePermissions = string.IsNullOrWhiteSpace(value) || value == "null" ? new List<Permission>() : JsonSerializer.Deserialize<List<Permission>>(value) ?? new List<Permission>();
        }

        /// <summary>
        /// 權限 JSON 字串（供前端顯示用）
        /// </summary>
        public string RolePermissionsJson => JsonSerializer.Serialize(RolePermissions ?? new List<Permission>());
    }
}
