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
        [Required]
        [StringLength(50)]
        [DisplayName("Project Name")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Project Description")]
        public string Description { get; set; }
        
        [DisplayName("Start Date")]
        [DataType(DataType.Date)]
        public DateTimeOffset StartDate { get; set; }
        
        [DisplayName("Start Date")]
        [DataType(DataType.Date)]
        public DateTimeOffset EndDate { get; set; }
        
        [DisplayName("Date Archived")]
        [DataType(DataType.Date)]
        public DateTimeOffset? ArchiveDate { get; set; }

        public bool Archived { get; set; }

        //Image properties
        [NotMapped]
        [DataType(DataType.Upload)]
        //[MaxFileSize(1024*1024)]
        //[AllowedExtensions(".jpg",".png" )]
        public IFormFile ImageFormFile { get; set; }
        public string ImageFileName { get; set; }
        public byte[] ImageFileData { get; set; }
        [DisplayName("File Extension")]
        public string ImageFileContentType { get; set; }
        public int MyProperty { get; set; }
        [DisplayName("Project Priority")]
        public int ProjectPriorityId { get; set; }
        public virtual ProjectPriority ProjectPriority { get; set; }
        public virtual Company Company { get; set; }
        public virtual List<BTUser> Members { get; set; }
        public virtual List<Ticket> Tickets { get; set; }
    }
}
