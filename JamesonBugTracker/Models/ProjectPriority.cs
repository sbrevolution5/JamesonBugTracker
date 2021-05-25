using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models
{
    public class ProjectPriority
    {
        public int Id { get; set; }

        [DisplayName("Project")]
        public int ProjectId { get; set; }
        public string Priority { get; set; }
        public virtual Project Project { get; set; }
    }
}
