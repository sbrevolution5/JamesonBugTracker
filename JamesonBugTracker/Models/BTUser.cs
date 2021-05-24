using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models
{
    public class BTUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [NotMapped]
        public string FullName
        {
            get
            { return FirstName + LastName; }
        }
    }
}
