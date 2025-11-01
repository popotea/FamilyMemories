using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FamilyMemories.Data;
using FamilyMemories.Models;

namespace FamilyMemories.Pages.Memories
{
    public class IndexModel : PageModel
    {
        private readonly FamilyMemories.Data.ApplicationDbContext _context;

        public IndexModel(FamilyMemories.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Memory> Memories { get;set; } = default!;

        // 前後端分離 - 資料由前端 API 取得，不在伺服器端渲染
        public void OnGet()
        {
            // 不需要在伺服器端載入資料，前端會用 API 取得
        }
    }
}
