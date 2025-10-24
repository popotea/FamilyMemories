using Microsoft.AspNetCore.Identity;

namespace FamilyMemories.Models
{
    public class ApplicationUser : IdentityUser
    {
        // 可以在此加入額外的使用者屬性
        // 例如：全名、生日、頭像等
        public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string AvatarPath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Memory> Memories { get; set; }
    }
}
