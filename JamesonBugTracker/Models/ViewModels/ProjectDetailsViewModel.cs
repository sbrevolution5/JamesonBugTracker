using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models.ViewModels
{
    public class ProjectDetailsViewModel
    {
        public Project Project { get; set; }
        public List<Ticket> OpenTickets { get; set; }
        public List<Ticket> ResolvedTickets { get; set; }
    }
}
