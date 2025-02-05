using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models.ViewModels
{
    public class UnassignedTicketsViewModel
    {
        public List<Ticket> UnassignedTickets { get; set; }
        public List<Project> Projects { get; set; }
        public List<SelectList> UserSelectLists { get; set; }
    }
}
