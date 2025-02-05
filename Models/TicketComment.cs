using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        [Required]
        public string Comment { get; set; }
        [DisplayName("Date")]
        [DataType(DataType.Date)]
        public DateTimeOffset Created { get; set; }
        [DisplayName("Ticket")]
        public int TicketId { get; set; }
        [DisplayName("Author")]
        public string UserId { get; set; }
        public virtual Ticket Ticket { get; set; }
        public virtual BTUser User { get; set; }
    }
}
