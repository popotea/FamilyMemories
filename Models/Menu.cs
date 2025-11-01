using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyMemories.Models
{
    public class Menu
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(32)]
        public string Title { get; set; }
        [MaxLength(128)]
        public string Url { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
