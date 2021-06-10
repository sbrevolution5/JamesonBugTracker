using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models.ViewModels
{
    public class MyArchivedTicketsViewModel
    {
        public IEnumerable<Ticket> DevTicketsArchived { get; set; }
        public IEnumerable<Ticket> SubTicketsArchived { get; set; }
    }
}
