using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTimeOffset Created { get; set; }
        public int TicketId { get; set; }
        public string RecipientId { get; set; }
        public int SenderId { get; set; }
        public bool Viewed { get; set; }
        public virtual Ticket Ticket { get; set; }
        public virtual BTUser Recipient { get; set; }
        //TODO Change type
        public int Sender { get; set; }
    }
}
