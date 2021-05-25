﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile FormFile { get; set; }
        public string FileName { get; set; }
        public string FileContentType { get; set; }
        public byte[] FileData { get; set; }
        public int TicketId { get; set; }
        public string UserId { get; set; }
        public virtual BTUser User { get; set; }
        public virtual Ticket Ticket { get; set; }
    }
}
