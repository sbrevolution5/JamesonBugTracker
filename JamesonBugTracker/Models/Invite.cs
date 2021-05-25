using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models
{
    public class Invite
    {
        public int Id { get; set; }
        [DisplayName("Invite Date")]
        public DateTimeOffset InviteDate { get; set; }
        //Todo Type?
        public int CompanyToken { get; set; }
        public int CompanyId { get; set; }
        public int ProjectId { get; set; }
        [DisplayName("Invite Recipient")]
        public string InviteeId { get; set; }
        [DisplayName("Invite Sender")]
        public string SenderId { get; set; }
        [DisplayName("Recipient Email")]
        public string InviteeEmail { get; set; }
        [DisplayName("Recipient First Name")]
        public string FirstName { get; set; }
        [DisplayName("Recipient Last Name")]
        public string LastName { get; set; }
        public bool IsValid { get; set; }
        public virtual Company Company { get; set; }
        public virtual Project Project { get; set; }
        public virtual BTUser Invitee { get; set; }
        public virtual BTUser Sender { get; set; }
    }
}
