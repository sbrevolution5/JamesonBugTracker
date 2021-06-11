using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
using JamesonBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
namespace JamesonBugTracker.Services
{
    public class BTHistoryService : IBTHistoryService
    {
        private readonly ApplicationDbContext _context;

        public BTHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {
            if (oldTicket == null && newTicket is not null)
            {
                TicketHistory history = new()
                {
                    TicketId = newTicket.Id,
                    Property = "",
                    OldValue = "",
                    NewValue = "New Ticket",
                    Created = DateTime.Now,
                    UserId = userId,
                    Description = "New Ticket Created"
                };
                await _context.TicketHistory.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            else
            {
                if (oldTicket.Title != newTicket.Title)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Title",
                        OldValue = oldTicket.Title,
                        NewValue = newTicket.Title,
                        Created = DateTime.Now,
                        UserId = userId,
                        Description = $"Title updated from {oldTicket.Title} to {newTicket.Title}"
                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                if (oldTicket.Description != newTicket.Description)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Description",
                        OldValue = oldTicket.Description,
                        NewValue = newTicket.Description,
                        Created = DateTime.Now,
                        UserId = userId,
                        Description = $"Description updated from {oldTicket.Description} to {newTicket.Description}"
                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Developer User",
                        OldValue = oldTicket.DeveloperUser?.FullName ?? "Unassigned",
                        NewValue = newTicket.DeveloperUser?.FullName,
                        Created = DateTime.Now,
                        UserId = userId,
                        Description = $"Developer User updated from {oldTicket.DeveloperUser?.FullName ?? "Unassigned"} to {newTicket.DeveloperUser?.FullName}"
                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Ticket Type",
                        OldValue = oldTicket.TicketType.Name,
                        NewValue = newTicket.TicketType.Name,
                        Created = DateTime.Now,
                        UserId = userId,
                        Description = $"Ticket Type updated from {oldTicket.TicketType.Name} to {newTicket.TicketType.Name}"

                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Ticket Priority",
                        OldValue = oldTicket.TicketPriority.Name,
                        NewValue = newTicket.TicketPriority.Name,
                        Created = DateTime.Now,
                        UserId = userId,
                        Description = $"Ticket Priority updated from {oldTicket.TicketPriority.Name} to {newTicket.TicketPriority.Name}"

                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Ticket Status",
                        OldValue = oldTicket.TicketStatus.Name,
                        NewValue = newTicket.TicketStatus.Name,
                        Created = DateTime.Now,
                        UserId = userId,
                        Description = $"Ticket Status updated from {oldTicket.TicketStatus.Name} to {newTicket.TicketStatus.Name}"

                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                //TODO Do we keep this functionality or not?
                if (oldTicket.Comments.Count != newTicket.Comments.Count)
                {
                    TicketComment latestComment = newTicket.Comments.OrderByDescending(c => c.Created).FirstOrDefault();
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Comment",
                        OldValue = "",
                        NewValue = latestComment.Comment,
                        CommentId = latestComment.Id,
                        Created = DateTime.Now,
                        UserId = userId,
                        Description = $"{latestComment.User.FullName} commented: {latestComment.Comment}"

                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                await _context.SaveChangesAsync();
            }
            return;
        }

        public async Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId)
        {
            Company company = await _context.Company.Include(c => c.Projects).FirstOrDefaultAsync(c=>c.Id==companyId);
            List<TicketHistory> histories = new();
            foreach (var project in company.Projects)
            {
                var projHistories = await GetProjectTicketsHistoriesAsync(project.Id);
                histories = histories.Concat(projHistories).ToList();
            }
            return histories.ToList();
        }

        public async Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId)
        {
            var project = await _context.Project
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.History)
                                                .FirstOrDefaultAsync(p => p.Id == projectId);
            List<TicketHistory> histories = project.Tickets.SelectMany(t => t.History).ToList();
            return histories;
        }
    }
}
