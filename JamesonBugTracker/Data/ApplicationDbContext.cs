﻿using JamesonBugTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JamesonBugTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<BTUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<JamesonBugTracker.Models.Project> Projects { get; set; }
        public DbSet<JamesonBugTracker.Models.Company> Companies { get; set; }
        public DbSet<JamesonBugTracker.Models.Invite> Invites { get; set; }
        public DbSet<JamesonBugTracker.Models.ProjectPriority> ProjectPriorities { get; set; }
        public DbSet<JamesonBugTracker.Models.Ticket> Ticket { get; set; }
        public DbSet<JamesonBugTracker.Models.TicketAttachment> TicketAttachment { get; set; }
        public DbSet<JamesonBugTracker.Models.TicketComment> TicketComment { get; set; }
        public DbSet<JamesonBugTracker.Models.TicketHistory> TicketHistory { get; set; }
        public DbSet<JamesonBugTracker.Models.TicketPriority> TicketPriority { get; set; }
        public DbSet<JamesonBugTracker.Models.TicketStatus> TicketStatus { get; set; }
        public DbSet<JamesonBugTracker.Models.TicketType> TicketType { get; set; }
        public DbSet<JamesonBugTracker.Models.Notification> Notification { get; set; }
    }
}
