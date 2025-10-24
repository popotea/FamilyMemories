using System;
using System.ComponentModel.DataAnnotations;

namespace FamilyMemories.Models
{
    public class Memory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public string ImagePath { get; set; } // Path to the uploaded image

        // Foreign key for ApplicationUser
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
