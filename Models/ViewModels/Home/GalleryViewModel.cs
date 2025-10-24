using System.Collections.Generic;
using FamilyMemories.Models;

namespace FamilyMemories.Models.ViewModels.Home
{
    public class GalleryViewModel
    {
        public IList<Memory> Memories { get; set; } = new List<Memory>();
        public bool IsAuthenticated { get; set; }
    }
}
