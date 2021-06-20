using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
using JamesonBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketService(ApplicationDbContext context)
        {
            _context = context;
        }
        private async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            return await _context.Ticket.FirstOrDefaultAsync(t => t.Id == ticketId);
        }
        private async Task<BTUser> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task UnassignTicketAsync(int ticketId)
        {
            Ticket ticket = await GetTicketByIdAsync(ticketId);
            if(ticket is not null)
            {
                try
                {
                    ticket.TicketStatusId = (await LookupTicketStatusIdAsync("Unassigned")).Value;
                    ticket.DeveloperUserId = null;
                    ticket.Updated = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return;
                }
                catch 
                {

                    throw;
                }
            }
        }
        public async Task AssignTicketAsync(int ticketId, string userId)
        {
            Ticket ticket = await GetTicketByIdAsync(ticketId);
            if (ticket is not null)
            {

                try
                {
                    // you must assign the Id property, not the virtual/navigation property.  Otherwise it won't change anything in database (except ticket status)
                    ticket.TicketStatusId = (await LookupTicketStatusIdAsync("Development")).Value;
                    ticket.DeveloperUserId = userId;
                    ticket.Updated = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public async Task<List<Ticket>> GetAllPMTicketsAsync(string userId)
        {
            try
            {
                List<Ticket> allTickets = new();
                BTUser user = await GetUserByIdAsync(userId);
                foreach (var project in user.Projects)
                {
                    foreach (var ticket in project.Tickets)
                    {
                        allTickets.Add(ticket);
                    }
                }
                return allTickets.OrderByDescending(t => t.Updated).ToList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
            try
            {

                //we don't use the company info service so that we can more easily seperate the service from the project
                List<Ticket> tickets = await _context.Project
                                                 .Include(p => p.Company)
                                                 .Where(p => p.CompanyId == companyId)
                                                 .SelectMany(p => p.Tickets)
                                                    .Include(t => t.OwnerUser)
                                                    .Include(t => t.DeveloperUser)
                                                    .Include(t => t.TicketPriority)
                                                    .Include(t => t.TicketStatus)
                                                    .Include(t => t.TicketType)
                                                    .Include(t => t.Attachments)
                                                    .Include(t => t.History)
                                                    .Include(t => t.Comments)
                                                    .Include(t=> t.Project)
                                                    .OrderByDescending(t => t.Updated)
                                                    
                                                 .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {

            try
            {
                var allTickets = await GetAllTicketsByCompanyAsync(companyId);
                List<Ticket> ticketsWithPriority = new();
                foreach (var ticket in allTickets)
                {
                    if (ticket.TicketPriority.Name == priorityName)
                    {
                        ticketsWithPriority.Add(ticket);
                    }
                }
                return ticketsWithPriority;
            }
            catch { throw; }

        }

        public async Task<List<Ticket>> GetAllTicketsByRoleAsync(string role, string userId)
        {
            //don't filter users by role or try to match role
            try
            {
                List<Ticket> tickets = await _context.Ticket.ToListAsync();
                if (role == "Developer")
                {
                    tickets = await _context.Ticket.Include(t => t.OwnerUser)
                                                .Include(t => t.DeveloperUser)
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.Attachments)
                                                .Include(t => t.History)
                                                .Include(t => t.Comments)
                                                .Where(t => t.DeveloperUserId == userId)
                                                .OrderByDescending(t => t.Updated)
                                                
                                                .ToListAsync();
                }
                else if (role == "ProjectManager")
                {
                    tickets = await GetAllPMTicketsAsync(userId);
                }
                else if (role == "Admin")
                {
                    BTUser admin = await GetUserByIdAsync(userId);
                    tickets = await GetAllTicketsByCompanyAsync(admin.CompanyId.Value);
                }
                else if (role == "Submitter")//submitter
                {
                    tickets = await _context.Ticket.Include(t => t.OwnerUser)
                                                .Include(t => t.DeveloperUser)
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.Attachments)
                                                .Include(t => t.History)
                                                .Include(t => t.Comments)
                                                .Where(t => t.OwnerUserId == userId)
                                                
                                                .ToListAsync();
                }

                return tickets;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<Ticket>> GetArchivedUserTicketsAsync(string userId,string roleSubOrDev)
        {
            var userTickets = await GetAllTicketsByRoleAsync(roleSubOrDev, userId);
            List<Ticket> filteredTickets = userTickets.Where(t => t.Archived == true).ToList();
            return filteredTickets;
        }
        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {
            try
            {
                var allTickets = await GetAllTicketsByCompanyAsync(companyId);
                List<Ticket> ticketsWithStatus = new();
                foreach (var ticket in allTickets)
                {
                    if (ticket.TicketStatus.Name == statusName)
                    {
                        ticketsWithStatus.Add(ticket);
                    }
                }
                return ticketsWithStatus;
            }
            catch { throw; }
        }
        public async Task<List<Ticket>> GetProjectTicketsNotResolvedOrArchivedAsync(int companyId, int projectId)
        {
            List<Ticket> tickets = new();
            tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => (t.ProjectId == projectId && (t.Archived == false&&t.TicketStatus.Name != "Resolved"))).ToList();
            return tickets;
            
        }
        public async Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            tickets = (await GetAllTicketsByPriorityAsync(companyId, priorityName)).Where(t => t.ProjectId == projectId).ToList();
            return tickets;
        }
        public async Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            tickets = (await GetAllTicketsByStatusAsync(companyId, statusName)).Where(t => t.ProjectId == projectId).ToList();
            return tickets;
        }
        public async Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            tickets = (await GetAllTicketsByTypeAsync(companyId, typeName)).Where(t => t.ProjectId == projectId).ToList();
            return tickets;
        }


        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            try
            {
                try
                {
                    var allTickets = await GetAllTicketsByCompanyAsync(companyId);
                    List<Ticket> ticketsOfType = new();
                    foreach (var ticket in allTickets)
                    {
                        if (ticket.TicketType.Name == typeName)
                        {
                            ticketsOfType.Add(ticket);
                        }
                    }
                    return ticketsOfType;
                }
                catch { throw; }
            }
            catch { throw; }
        }

        public async Task<List<Ticket>> GetArchivedTicketsByCompanyAsync(int companyId)
        {
            List<Ticket> companyTickets = await GetAllTicketsByCompanyAsync(companyId);
            List<Ticket> filteredTickets = companyTickets.Where(t => t.Archived == true).ToList();
            return filteredTickets;
        }

        public async Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId)
        {
            var tickets = await _context.Ticket.Where(t => t.ProjectId == projectId).ToListAsync();
            if (role == "Developer")
            {
                tickets = tickets.Where(t => t.DeveloperUserId == userId).ToList();

            }
            else
            {
                tickets = tickets.Where(t => t.OwnerUserId == userId).ToList();

            }
            return tickets;
        }
        public async Task<BTUser> GetTicketDeveloperAsync(int ticketId)
        {
            BTUser developer = new();
            var ticket = await _context.Ticket.Where(t => t.Id == ticketId).FirstOrDefaultAsync();
            if (ticket?.DeveloperUserId != null)
            {
                developer = ticket.DeveloperUser;
            }
            return developer;
        }
        public async Task SetTicketStatusAsync(int ticketId, string StatusName)
        {
            var ticket = await _context.Ticket.Where(t => t.Id == ticketId).FirstOrDefaultAsync();
            var status = await LookupTicketStatusIdAsync(StatusName);
            if (status is not null)
            {
                try
                {
                    ticket.TicketStatusId = (int)status;
                    ticket.Updated = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
                catch { throw; }
            }
            return;
        }

        public async Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            TicketPriority ticketPriority = await _context.TicketPriority.FirstOrDefaultAsync(t => t.Name == priorityName);
            return ticketPriority.Id;
        }

        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            TicketStatus ticketStatus = await _context.TicketStatus.FirstOrDefaultAsync(t => t.Name == statusName);
            return ticketStatus.Id;
        }

        public async Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            TicketType ticketType = await _context.TicketType.FirstOrDefaultAsync(t => t.Name == typeName);
            return ticketType.Id;
        }

        public async Task<List<Ticket>> GetAllDeveloperTicketsByResolvedAsync(string userId, bool isResolvedOrNot)
        {
            var userTickets = await GetAllTicketsByRoleAsync("Developer", userId);
            var filteredTickets = userTickets.Where(t => ((t.TicketStatus.Name == "Resolved") == isResolvedOrNot) && t.TicketStatus.Name != "Archived").ToList();
            return filteredTickets;
        }

        public async Task<List<Ticket>> GetAllUnassignedTicketsByProjectAsync(int projectId, int companyId)
        {
            var newTickets = await GetProjectTicketsByStatusAsync("New", companyId, projectId);
            var unassignedTickets = await GetProjectTicketsByStatusAsync( "Unassigned",companyId, projectId);
            return newTickets.Concat(unassignedTickets).Where(t => !t.Archived).ToList();
        }
        public async Task<List<Ticket>> GetAllUnassignedTicketsAsync(int companyId)
        {
            var newTickets = await GetAllTicketsByStatusAsync(companyId, "New");
            var unassignedTickets = await GetAllTicketsByStatusAsync(companyId, "Unassigned");
            return newTickets.Concat(unassignedTickets).Where(t=>!t.Archived).ToList();
        }

        public async Task<Ticket> GetOneTicketNotTrackedAsync(int ticketId)
        {
            Ticket oldTicket = await _context.Ticket
                                                    .Include(t => t.TicketPriority)
                                                    .Include(t=> t.Comments).ThenInclude(c=>c.User)
                                                    .Include(t => t.TicketStatus)
                                                    .Include(t => t.TicketType)
                                                    .Include(t => t.Project)
                                                    .Include(t => t.DeveloperUser)
                                                    .Include(t => t.Attachments).ThenInclude(a => a.User)
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync(t => t.Id == ticketId);
            return oldTicket;
        }

        public async Task<List<Ticket>> GetAllTicketsByProjectAsync(int projectId)
        {
            var tickets = await _context.Ticket.Include(t => t.OwnerUser)
                                                .Include(t => t.DeveloperUser)
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.Attachments)
                                                .Include(t => t.History)
                                                .Include(t => t.Comments)
                                                .Where(t => t.ProjectId == projectId)
                                                
                                                .ToListAsync();
            return tickets;
        }
    }
}
