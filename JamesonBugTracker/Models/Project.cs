using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        [DisplayName("Company")]
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [DisplayName("Start Date")]
        public DateTimeOffset StartDate { get; set; }
        [DisplayName("Start Date")]
        public DateTimeOffset EndDate { get; set; }
        //Image properties
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile FormFile { get; set; }
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
        [DisplayName("File Extension")]
        public string FileContentType { get; set; }
        public int MyProperty { get; set; }
        [DisplayName("Project Priority")]
        public int ProjectPriorityId { get; set; }
        public bool Archived { get; set; }
        public virtual ProjectPriority ProjectPriority { get; set; }
        public virtual Company Company { get; set; }
        public virtual List<BTUser> Members { get; set; }
        public virtual List<Ticket> Tickets { get; set; }
    }
}
