using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using PinkUmbrella.Models;
using Tides.Models;

namespace PinkUmbrella.ViewModels.Archive
{
    public class UploadMediaViewModel
    {
        public List<IFormFile> Files { get; set; }

        public string Description { get; set; }
        
        public string Title { get; set; }

        public string Attribution { get; set; }
        
        public Visibility Visibility { get; set; }
    }
}