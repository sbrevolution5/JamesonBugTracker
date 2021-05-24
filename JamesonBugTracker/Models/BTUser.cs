using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models
{
    public class BTUser : IdentityUser
    {
        [Required]
        [DisplayName("First Name")]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Required]
        [DisplayName("Last Name")]
        [StringLength(50)]
        public string LastName { get; set; }
        [NotMapped]
        [DisplayName("Full Name")]
        public string FullName
        {
            get
            { return $"{FirstName} + {LastName}"; }
        }
    }
}
