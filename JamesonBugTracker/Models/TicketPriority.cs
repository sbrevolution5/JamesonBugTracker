using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }
        [DisplayName("TicketPriority")]
        public string Name { get; set; }
    }
}
