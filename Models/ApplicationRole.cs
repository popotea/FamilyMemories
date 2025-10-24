using Microsoft.AspNetCore.Identity;

namespace FamilyMemories.Models
{
    public class ApplicationRole : IdentityRole
    {
        // 可以添加角色描述等額外屬性
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
