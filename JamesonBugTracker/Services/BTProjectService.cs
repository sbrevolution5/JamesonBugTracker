using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
using JamesonBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTProjectService(ApplicationDbContext context, IBTCompanyInfoService companyInfoService, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }
        private async Task<Project> GetProjectByIdAsync(int projectId)
        {
            return await _context.Project.Include(p => p.Members)
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
                                             .FirstOrDefaultAsync(p => p.Id == projectId);
        }
        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            try
            {

                Project project = await GetProjectByIdAsync(projectId);
                //This returns a user who is a member, or it returns null?
                foreach (var user in project.Members)
                {
                    // If user is a project manager, return false.  
                    if (await _rolesService.IsUserInRoleAsync(user, "ProjectManager"))
                    {
                        project.Members.Remove(user);
                    }
                }
                //else, assign the new project manager to the project, then return true
                BTUser newManager = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                project.Members.Add(newManager);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            try
            {
                BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user is not null)
                {

                    Project project = await GetProjectByIdAsync(projectId);
                    if (!await IsUserOnProjectAsync(userId, projectId))
                    {
                        try
                        {
                            project.Members.Add(user);
                            await _context.SaveChangesAsync();
                            return true;
                        }
                        catch
                        {
                            throw;
                        }

                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"***ERROR*** - Error Adding user to project. --> {ex.Message}");
                return false;
            }
        }

        public async Task<List<BTUser>> DevelopersOnProjectAsync(int projectId)
        {
            return await GetProjectMembersByRoleAsync(projectId, "Developer");
        }

        public async Task<List<Project>> GetAllProjectsByCompanyAsync(int companyId)
        {
            List<Project> projects = new();
            projects = await _context.Project
                                             .Include(p => p.Company)
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
        public async Task<List<Project>> GetAllUnarchivedProjectsByCompanyAsync(int companyId)
        {
            List<Project> companyProjects = await GetAllProjectsByCompanyAsync(companyId);
            var unarchived = companyProjects.Where(p => !p.Archived);
            return unarchived;
        }

        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priorityName)
        {
            var companyProjects = await GetAllProjectsByCompanyAsync(companyId);
            List<Project> byPriority = new();
            foreach (var project in companyProjects)
            {
                if (project.ProjectPriority.Name == priorityName)
                {
                    byPriority.Add(project);
                }
            }
            return byPriority;
        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyAsync(int companyId)
        {
            var companyProjects = await GetAllProjectsByCompanyAsync(companyId);
            var archivedCompanyProjects = companyProjects.Where(p => p.Archived == true).ToList();
            return archivedCompanyProjects;
        }

        public async Task<List<BTUser>> GetMembersWithoutPMAsync(int projectId)
        {
            Project project = await GetProjectByIdAsync(projectId);
            List<BTUser> nonPMUsers = new();
            foreach (var user in project.Members)
            {
                if (!await _rolesService.IsUserInRoleAsync(user, "ProjectManager"))
                {
                    nonPMUsers.Add(user);
                }
            }
            return nonPMUsers;
        }

        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            BTUser projectManager = (await GetProjectMembersByRoleAsync(projectId, "ProjectManager")).FirstOrDefault();
            if (projectManager is null)
            {
                projectManager = (await GetProjectMembersByRoleAsync(projectId, "Admin")).FirstOrDefault();
            }
            return projectManager;
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            Project project = await _context.Project.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId); // Explicit call to _context.Projects....etc.
            List<BTUser> membersByRole = new();
            foreach (var user in project.Members)
            {
                if (await _rolesService.IsUserInRoleAsync(user, role))
                {
                    membersByRole.Add(user);
                }
            }
            return membersByRole;
        }

        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            Project project = await GetProjectByIdAsync(projectId);
            foreach (var user in project.Members)
            {
                if (user.Id == userId)
                {
                    return true;
                }
            }
            return false;
        }


        public async Task<List<Project>> ListUserProjectsAsync(string userId)
        {
            var projectList = (await _context.Users.Include(u => u.Projects)
                .ThenInclude(p => p.ProjectPriority)
                                    .Include(u => u.Projects).ThenInclude(p => p.Members)
                                    .Include(u => u.Projects)
                                    .ThenInclude(p => p.Tickets)
                                                .ThenInclude(t => t.DeveloperUser)
                                    .Include(u => u.Projects)
                                        .ThenInclude(p => p.Tickets)
                                            .ThenInclude(t => t.TicketPriority)
                                    .Include(u => u.Projects)
                                        .ThenInclude(p => p.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                    .Include(u => u.Projects)
                                        .ThenInclude(p => p.Tickets)
                                                .ThenInclude(t => t.TicketType)
                                    .Include(u => u.Projects)
                                        .ThenInclude(p => p.Tickets)
                                            .ThenInclude(t => t.Attachments)
                                    .Include(u => u.Projects)
                                        .ThenInclude(p => p.Tickets)
                                            .ThenInclude(t => t.History)
                                    .Include(u => u.Projects)
                                        .ThenInclude(p => p.Tickets)
                                            .ThenInclude(t => t.Comments)
                                    .Include(u => u.Projects)
                                        .ThenInclude(p => p.Company)
                                .FirstOrDefaultAsync(u => u.Id == userId)).Projects.ToList();
            return projectList;
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            try
            {
                Project project = await GetProjectByIdAsync(projectId);
                BTUser projectManager = (await GetProjectMembersByRoleAsync(projectId, "ProjectManager")).FirstOrDefault();
                project.Members.Remove(projectManager);
                await _context.SaveChangesAsync();
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"***ERROR*** - Error removing project manager. --> {ex.Message}");
                throw;
            }
        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                Project project = await GetProjectByIdAsync(projectId);
                project.Members.Remove(user);
                await _context.SaveChangesAsync();
                return;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"***ERROR*** - Error Removing user from project. --> {ex.Message}");
                throw;
            }
        }


        public async Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
        {
            try
            {
                List<BTUser> usersToRemove = await GetProjectMembersByRoleAsync(projectId, role);
                Project project = await GetProjectByIdAsync(projectId);
                foreach (var user in project.Members)
                {
                    if (usersToRemove.Contains(user))
                    {
                        project.Members.Remove(user);
                    }
                }
                await _context.SaveChangesAsync();
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"***ERROR*** - Error Removing users from project by role. --> {ex.Message}");
                throw;
            }


        }

        public Task<List<BTUser>> SubmittersOnProjectAsync(int projectId)
        {
            return GetProjectMembersByRoleAsync(projectId, "Submitter");
        }

        public async Task<List<BTUser>> UsersNotOnProjectAsync(int projectId, int companyId)
        {
            List<BTUser> allNonProjectUsers = await _context.Users.ToListAsync();
            Project project = await GetProjectByIdAsync(projectId);
            foreach (var user in project.Members)
            {
                allNonProjectUsers.Remove(user);
            }
            return allNonProjectUsers;
        }
        private async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            TicketStatus ticketStatus = await _context.TicketStatus.FirstOrDefaultAsync(t => t.Name == statusName);
            return ticketStatus.Id;
        }
        public async Task<List<Project>> GetProjectsWithUnassignedTicketsAsync(int companyId)
        {
            List<Project> companyProjects = await GetAllProjectsByCompanyAsync(companyId);
            List<Project> results = new();
            foreach (var project in companyProjects)
            {

                var allTickets = project.Tickets.ToList();
                int newId = (int)await LookupTicketStatusIdAsync("New");
                int unId = (int)await LookupTicketStatusIdAsync("Unassigned");

                if (allTickets.Any(t => t.TicketStatusId == newId || t.TicketStatusId == unId))
                {

                    results.Add(project);
                }
            }
            return results;

        }

        public async Task<SelectList> GetSelectListOfProjectMembersWithoutPM(int projectID)
        {
            var result = new SelectList(await GetMembersWithoutPMAsync(projectID), "Id", "FullName");
            return result;
        }
    }
}
