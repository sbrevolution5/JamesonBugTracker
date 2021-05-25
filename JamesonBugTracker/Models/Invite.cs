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
        
        [DisplayName("Code")]
        public Guid CompanyToken { get; set; }
        
        [DisplayName("Company")]
        public int CompanyId { get; set; }
        
        [DisplayName("Project")]
        public int ProjectId { get; set; }
        
        [DisplayName("Invite Recipient")]
        public string RecipientId { get; set; }
        
        [DisplayName("Invite Sender")]
        public string SenderId { get; set; }
        
        [DisplayName("Recipient Email")]
        public string RecipientEmail { get; set; }
        
        [DisplayName("Recipient First Name")]
        public string FirstName { get; set; }

        [DisplayName("Recipient Last Name")]
        public string LastName { get; set; }
        
        [DisplayName("Is Valid")]
        public bool IsValid { get; set; }
        
        public virtual Company Company { get; set; }
        
        public virtual Project Project { get; set; }
        
        public virtual BTUser Recipient { get; set; }
        
        public virtual BTUser Sender { get; set; }
    }
}
