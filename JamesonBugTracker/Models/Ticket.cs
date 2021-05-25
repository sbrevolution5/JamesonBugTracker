using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public DateTimeOffset Archived { get; set; }
        [DisplayName("Project")]
        public int ProjectId { get; set; }
        [DisplayName("Ticket Priority")]
        public int TicketPriorityId { get; set; }
        [DisplayName("Ticket Status")]
        public int TicketStatusId { get; set; }
        [DisplayName("Ticket Type")]
        public int TicketTypeId { get; set; }
        [DisplayName("Owner User")]
        public string OwnerUserId { get; set; }
        [DisplayName("Developer User")]
        public string DeveloperUserId { get; set; }
        // Navigational
        public List<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
        public List<TicketComment> Comments { get; set; } = new List<TicketComment>();
        public List<TicketHistory> History { get; set; } = new List<TicketHistory>();
        public List<Notification> Notification { get; set; } = new List<Notification>();
        public virtual Project Project { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public virtual TicketType TicketType { get; set; }
        public virtual BTUser OwnerUser { get; set; }
        public virtual BTUser DeveloperUser { get; set; }

    }
}
