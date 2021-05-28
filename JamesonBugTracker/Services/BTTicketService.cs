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
        private readonly IBTCompanyInfoService _companyInfoService;

        public BTTicketService(ApplicationDbContext context, IBTCompanyInfoService companyInfoService)
        {
            _context = context;
            _companyInfoService = companyInfoService;
        }
        private async  Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            return await _context.Ticket.FirstOrDefaultAsync(t => t.Id == ticketId);
        }
        private async Task<BTUser> GetUserByIdAsync(string userId)
        {
            return  await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task AssignTicketAsync(int ticketId, string userId)
        {
            BTUser user = await GetUserByIdAsync(userId); 
            Ticket ticket = await GetTicketByIdAsync(ticketId);
            ticket.DeveloperUser = user;
            await _context.SaveChangesAsync();
            return;
        }
        public async Task<List<Ticket>> GetAllPMTicketsAsync(string userId)
        {
            BTUser user = await GetUserByIdAsync(userId);
            List<Ticket> allTickets = new();
            foreach (var project in user.Projects)
            {
                foreach (var ticket in project.Tickets)
                {
                    allTickets.Add(ticket);
                }
            }
            return allTickets;
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
            List<Ticket> tickets = await _companyInfoService.GetAllTicketsAsync(companyId);
            return tickets;
        }

        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            var allTickets = await _companyInfoService.GetAllTicketsAsync(companyId);
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

        public Task<List<Ticket>> GetAllTicketsByRoleAsync(string role, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {

            var allTickets = await _companyInfoService.GetAllTicketsAsync(companyId);
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

        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            var allTickets = await _companyInfoService.GetAllTicketsAsync(companyId);
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

        public async Task<List<Ticket>> GetArchivedTicketsByCompanyAsync(int companyId)
        {
            List<Ticket> companyTickets = await _companyInfoService.GetAllTicketsAsync(companyId);
            List<Ticket> filteredTickets = companyTickets.Where(t => t.Archived == true).ToList();
            return filteredTickets;
        }

        public Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId)
        {
            //Is this role the role of the user who gets the project?  Isn't that always developer?!  
            throw new NotImplementedException();
        }
        public async Task<BTUser> GetTicketDeveloperAsync(int ticketId)
        {
            var ticket = await _context.Ticket.Where(t => t.Id == ticketId).FirstOrDefaultAsync();
            return ticket.DeveloperUser;
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
    }
}
