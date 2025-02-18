﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(75)]
        public string Title { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [DataType(DataType.Date)]
        public DateTimeOffset Created { get; set; }

        [DataType(DataType.Date)]
        public DateTimeOffset? Updated { get; set; }
        [DataType(DataType.Date)]
        public DateTimeOffset? Completed { get; set; }

        [DisplayName("Date Archived")]
        [DataType(DataType.Date)]
        public DateTimeOffset? ArchiveDate { get; set; }
        
        public bool Archived { get; set; }
        
        [DisplayName("Project")]
        public int ProjectId { get; set; }
        
        [DisplayName("Ticket Priority")]
        public int TicketPriorityId { get; set; }
        
        [DisplayName("Ticket Status")]
        public int TicketStatusId { get; set; }
        
        [DisplayName("Ticket Type")]
        public int TicketTypeId { get; set; }
        
        [DisplayName("Ticket Owner")]
        public string OwnerUserId { get; set; }
        
        [DisplayName("Ticket Developer")]
        public string DeveloperUserId { get; set; }
        
        // Navigational
        public virtual ICollection<TicketAttachment> Attachments { get; set; } //= new HashSet<TicketAttachment>();
        public virtual ICollection<TicketComment> Comments { get; set; } //= new HashSet<TicketComment>();
        public virtual ICollection<TicketHistory> History { get; set; } //= new HashSet<TicketHistory>();
        public virtual ICollection<Notification> Notification { get; set; } //= new HashSet<Notification>();
        public virtual Project Project { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public virtual TicketType TicketType { get; set; }
        public virtual BTUser OwnerUser { get; set; }
        public virtual BTUser DeveloperUser { get; set; }

    }
}
