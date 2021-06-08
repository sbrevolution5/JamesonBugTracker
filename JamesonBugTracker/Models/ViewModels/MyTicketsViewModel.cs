using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models.ViewModels
{
    public class MyTicketsViewModel
    {
        public IEnumerable<Ticket> DevTicketsResolved { get; set; }
        public IEnumerable<Ticket> DevTicketsUnresolved { get; set; }
        public IEnumerable<Ticket> SubmittedTickets { get; set; }
    }
}
