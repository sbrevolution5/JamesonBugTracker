using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        [DisplayName("Ticket")]
        public int TicketId { get; set; }
        [DisplayName("User")]
        public string UserId { get; set; }
        public string Property { get; set; }
        [DisplayName("Old Value")]
        public string OldValue { get; set; }
        [DisplayName("New Value")]
        public string NewValue { get; set; }
        public DateTimeOffset Created { get; set; }
        public string Description { get; set; }
        public virtual Ticket Ticket { get; set; }
        public virtual BTUser User { get; set; }
    }
}
