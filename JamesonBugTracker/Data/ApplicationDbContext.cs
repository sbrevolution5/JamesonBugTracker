using JamesonBugTracker.Models;
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
        public DbSet<JamesonBugTracker.Models.Ticket> Tickets { get; set; }
        public DbSet<JamesonBugTracker.Models.TicketAttachment> TicketAttachments { get; set; }
        public DbSet<JamesonBugTracker.Models.TicketComment> TicketComments { get; set; }
        public DbSet<JamesonBugTracker.Models.TicketHistory> TicketHistories { get; set; }
        public DbSet<JamesonBugTracker.Models.TicketPriority> TicketPriorities { get; set; }
        public DbSet<JamesonBugTracker.Models.TicketStatus> TicketStatuses { get; set; }
        public DbSet<JamesonBugTracker.Models.TicketType> TicketTypes { get; set; }
        public DbSet<JamesonBugTracker.Models.Notification> Notifications { get; set; }
    }
}
