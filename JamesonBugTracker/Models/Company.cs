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
    public class Company
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        [DisplayName("Company Name")]
        public string Name { get; set; }

        [DisplayName("Company Description")]
        public string Description { get; set; }

        //image/company logo
        [NotMapped]
        [DataType(DataType.Upload)]
        //[MaxFileSize(1024*1024)]
        //[AllowedExtensions(".jpg",".png" )]
        public IFormFile ImageFormFile { get; set; }
        
        [DisplayName("File Name")]
        public string ImageFileName { get; set; }
        public byte[] ImageFileData { get; set; }

        [DisplayName("File Extension")]
        public string ImageFileContentType { get; set; }

        // Navigation 
        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<BTUser> Members { get; set; }
        public virtual ICollection<Invite> Invites { get; set; }
    }
}
