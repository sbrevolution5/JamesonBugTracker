using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string Subject { get; set; }
        
        public string Message { get; set; }
        
        [DataType(DataType.Date)]
        [DisplayName("Date")]
        public DateTimeOffset Created { get; set; }
        
        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        [DisplayName("Recipient")]
        public string RecipientId { get; set; }
        
        [DisplayName("Sender")]
        public string SenderId { get; set; }
        
        public bool Viewed { get; set; }
        public virtual Ticket Ticket { get; set; }
        public virtual BTUser Recipient { get; set; }
        public virtual BTUser Sender { get; set; }
        public string Title { get; set; }
    }
}
