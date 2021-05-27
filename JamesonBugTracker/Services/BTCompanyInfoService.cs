using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
using JamesonBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Services
{

    public class BTCompanyInfoService : IBTCompanyInfoService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;

        public BTCompanyInfoService(ApplicationDbContext context, UserManager<BTUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<List<BTUser>> GetAllMembersAsync(int companyId)
        {
            List<BTUser> bTUsers = new();
            bTUsers = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();
            return bTUsers;
        }
        /// <summary>
        /// Gets all projects associated with the company.  This also eagerly loads their children (tickets and members)
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<List<Project>> GetAllProjectsAsync(int companyId)
        {
            List<Project> projects = new();
            projects = await _context.Project
                                             .Include(p => p.Members)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.OwnerUser)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.DeveloperUser)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketPriority)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketType)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Attachments)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.History)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Comments)
                                             .Where(p => p.CompanyId == companyId)
                                             .ToListAsync();
            return projects;
        }

        public async Task<List<Ticket>> GetAllTicketsAsync(int companyId)
        {
            //problem TODO tickets doesn't have a company Id, needs a list of projects from that company, and any tickets that belong to those projects.
            List<Ticket> tickets = new();
            List<Project> projects = await GetAllProjectsAsync(companyId);
            tickets = projects.SelectMany(p => p.Tickets).ToList();
            return tickets;
        }

        public async Task<Company> GetCompanyInfoByIdAsync(int? companyId)
        {
            Company company = new();
            if (companyId is not null)
            {
                company = await _context.Company.Include(c => c.Members)
                                                .Include(c => c.Projects)
                                                .Include(c => c.Invites)
                                                .FirstOrDefaultAsync(c => c.Id == companyId);

            }
            return company;
        }

        public async Task<List<BTUser>> GetMembersInRoleAsync(string roleName, int companyId)
        {
            List<BTUser> allUsers = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
            List<BTUser> usersInRole = allUsers.Where(u => u.CompanyId == companyId).ToList();
            return usersInRole;
        }
    }
}
