using System;
using System.ComponentModel.DataAnnotations;

namespace FamilyMemories.Models.ViewModels.Admin
{
    public class EditMemoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "標題不能為空")]
        [StringLength(200, ErrorMessage = "標題長度不能超過200個字符")]
        public string Title { get; set; }

        [Required(ErrorMessage = "描述不能為空")]
        [StringLength(1000, ErrorMessage = "描述長度不能超過1000個字符")]
        public string Description { get; set; }

        [Required(ErrorMessage = "日期不能為空")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public string ImagePath { get; set; }
        public string UserName { get; set; }
    }
}