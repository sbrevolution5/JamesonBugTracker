using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models.ViewModels
{
    public class ProjectManagerViewModel
    {
        public int ProjectId { get; set; }
        public SelectList Managers { get; set; }
        public string NewManagerId { get; set; }
    }
}
