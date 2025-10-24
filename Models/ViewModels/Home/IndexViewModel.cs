using System.Collections.Generic;
using FamilyMemories.Models;

namespace FamilyMemories.Models.ViewModels.Home
{
    public class IndexViewModel
    {
        public string WelcomeMessage { get; set; }
        public string AppDescription { get; set; }
        public bool IsLoggedIn { get; set; }
        public string CurrentUserName { get; set; }
        public string CurrentUserId { get; set; }
        public string CurrentDate => DateTime.Now.ToString("yyyy年MM月dd日");
        public List<PhotoViewModel> Photos { get; set; }
    }
}
