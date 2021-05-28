using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
using JamesonBugTracker.Services.Interfaces;
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

        public Task AssignTicketAsync(int ticketId, string userId)
        {
            BTUser user = _context.Users.FirstOrDefault(u => u.Id == userId);
            Ticket ticket = _context.Ticket.FirstOrDefault(t => t.Id == ticketId);
            ticket.DeveloperUser = user; // or should it be DeveloperUserId?? TODO
            throw new NotImplementedException();
        }

        public Task<List<Ticket>> GetAllPMTicketsAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
            List<Ticket> tickets = await _companyInfoService.GetAllTicketsAsync(companyId);
            return tickets;
        }

        public Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Ticket>> GetAllTicketsByRoleAsync(string role, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Ticket>> GetArchivedTicketsByCompanyAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId)
        {
            throw new NotImplementedException();
        }
        // WHY IS THIS ASYNC TODO
        public Task<BTUser> GetTicketDeveloperAsync(int ticketId)
        {
            var ticket = _context.Ticket.Where(t => t.Id == ticketId).FirstOrDefault();
            //return ticket.DeveloperUser;
            throw new NotImplementedException();
        }

        public Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            throw new NotImplementedException();
        }

        public Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            throw new NotImplementedException();
        }

        public Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            throw new NotImplementedException();
        }
    }
}
