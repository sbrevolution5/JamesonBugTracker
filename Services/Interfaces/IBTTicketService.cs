﻿using JamesonBugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Services.Interfaces
{
    public interface IBTTicketService
    {
        Task UnassignTicketAsync(int ticketId);

        Task AssignTicketAsync(int ticketId, string userId);

        Task<BTUser> GetTicketDeveloperAsync(int ticketId);

        Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId);

        Task<List<Ticket>> GetArchivedTicketsByCompanyAsync(int companyId);

        Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName);

        Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName);

        Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName);

        Task<List<Ticket>> GetAllPMTicketsAsync(string userId);
        Task<List<Ticket>> GetAllUnassignedTicketsAsync(int companyId);

        Task<List<Ticket>> GetAllTicketsByRoleAsync(string role, string userId);
        Task<List<Ticket>> GetAllDeveloperTicketsByResolvedAsync(string userId, bool isResolvedOrNot);
        Task<List<Ticket>> GetAllTicketsByProjectAsync(int projectId);

        Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId);

        Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId);
        Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId);
        Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId);
        Task<List<Ticket>> GetAllUnassignedTicketsByProjectAsync(int projectId, int companyId);

        Task SetTicketStatusAsync(int ticketId, string StatusName);
        Task<Ticket> GetOneTicketNotTrackedAsync(int ticketId);
        Task<List<Ticket>> GetArchivedUserTicketsAsync(string userId, string roleSubOrDev);

        Task<int?> LookupTicketPriorityIdAsync(string priorityName);

        Task<int?> LookupTicketStatusIdAsync(string statusName);

        Task<int?> LookupTicketTypeIdAsync(string typeName);

        Task<List<Ticket>> GetProjectTicketsNotResolvedOrArchivedAsync(int companyId, int projectId);

    }
}
