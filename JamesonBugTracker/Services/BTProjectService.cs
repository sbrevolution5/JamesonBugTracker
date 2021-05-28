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
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTRolesService _rolesService;

        public BTProjectService(ApplicationDbContext context, IBTCompanyInfoService companyInfoService)
        {
            _context = context;
            _companyInfoService = companyInfoService;
        }
        /// <summary>
        /// returns false if project has a manager, or supplied user is not a manager.  Otherwise adds manager and returns true
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            //if the user supplied isn't a manager, return false
            BTUser newManager = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (!await _rolesService.IsUserInRoleAsync(newManager, "ProjectManager"))
            {
                return false;
            }
            var project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);
            //This returns a user who is a member, or it returns null?
            foreach (var user in project.Members)
            {
                // If user is a project manager, return false.  
                if (await _rolesService.IsUserInRoleAsync(user, "ProjectManager"))
                {
                    return false;
                }
            }
            //else, assign the new project manager to the project, then return true
            project.Members.Add(newManager);
            return true;
        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            var project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            project.Members.Add(user);
            var res = await _context.SaveChangesAsync();
            if (res >= 1)
            {
                return true;
            }
            return false;
        }

        public async Task<List<BTUser>> DevelopersOnProjectAsync(int projectId)
        {
            return await GetProjectMembersByRoleAsync(projectId, "Developer");
        }

        public async Task<List<Project>> GetAllProjectsByCompanyAsync(int companyId)
        {
            var companyProjects = (await _companyInfoService.GetAllProjectsAsync(companyId)).ToList();
            return companyProjects;

        }

        public Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priorityName)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyAsync(int companyId)
        {
            var companyProjects = await GetAllProjectsByCompanyAsync(companyId);
            var archivedCompanyProjects = companyProjects.Where(p => p.Archived).ToList();
            return archivedCompanyProjects;
        }

        public async Task<List<BTUser>> GetMembersWithoutPMAsync(int projectId)
        {
            var project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);
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

        public Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            //What if there isn't one?!
            throw new NotImplementedException();
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            var project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);
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
            var project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);
            foreach (var user in project.Members)
            {
                if (user.Id == userId)
                {
                    return true;
                }
            }
            return false;
        }

        public Task<List<Project>> ListUserProjectsAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveProjectManagerAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<List<BTUser>> SubmittersOnProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<List<BTUser>> UsersNotOnProjectAsync(int projectId, int companyId)
        {
            throw new NotImplementedException();
        }
    }
}
