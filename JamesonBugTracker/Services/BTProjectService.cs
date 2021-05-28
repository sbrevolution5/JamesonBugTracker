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
        private async Task<Project> GetProjectByIdAsync(int projectId)
        {
            return await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);
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
            Project project = await GetProjectByIdAsync(projectId);
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
            Project project = await GetProjectByIdAsync(projectId);
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

        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priorityName)
        {
            var companyProjects = await _companyInfoService.GetAllProjectsAsync(companyId);
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
            var archivedCompanyProjects = companyProjects.Where(p => p.Archived).ToList();
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
            //What if there isn't one?!
            // can we return a nullable? or do we return default
            BTUser projectManager = (await GetProjectMembersByRoleAsync(projectId, "ProjectManager")).FirstOrDefault();
            return projectManager;
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            Project project = await GetProjectByIdAsync(projectId);
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

        //TODO Does this need to be async?!
        public List<Project> ListUserProjects(string userId)
        {
            // All projects
            List<Project> userProjects = new();
            foreach (var project in _context.Project)
            {
                foreach (var user in project.Members)
                {
                    //If user is in project, add to list of projects
                    if (user.Id == userId)
                    {
                        userProjects.Add(project);
                        //TODO skip remaining loops?
                        //Maybe a while loop checking if we're at the end of the user list or if we found the user???
                        // or find a way to return like a method
                    }
                }
            }
            return userProjects;
        }

        //TODO Now returns a bool signifiying if removal was successful or not
        public async Task<bool> RemoveProjectManagerAsync(int projectId)
        {
            Project project = await GetProjectByIdAsync(projectId);
            BTUser projectManager = (await GetProjectMembersByRoleAsync(projectId, "ProjectManager")).FirstOrDefault();
            return project.Members.Remove(projectManager);
        }
        //Should this be boolean, in case user is not on project in the first place? TODO
        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            Project project = await GetProjectByIdAsync(projectId);
            project.Members.Remove(user);
            return;
        }


        public async Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
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
            return;

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
    }
}
