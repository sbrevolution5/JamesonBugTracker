using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
using JamesonBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Services
{
    public class BTRolesService : IBTRolesService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BTUser> _userManager;

        // using dependency injection, this service isn't responsible for creating/managing the instance
        // analogy: Teacher hands kindergarten the fingerprints, so the kid isn't responsible for that.
        public BTRolesService(ApplicationDbContext context, 
                              RoleManager<IdentityRole> roleManager, 
                              UserManager<BTUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
        {
            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<string> GetRoleNameByIdAsync(string roleId)
        {
            var role = _context.Roles.Find(roleId);
            string result = await _roleManager.GetRoleNameAsync(role);
            return result;
        }

        public async Task<bool> IsUserInRoleAsync(BTUser user, string roleName)
        {
            bool result = await _userManager.IsInRoleAsync(user, roleName);
            return result;
        }

        public async Task<IEnumerable<string>> ListUserRolesAsync(BTUser user)
        {
            IEnumerable<string> result = await _userManager.GetRolesAsync(user);
            return result;
        }

        public async Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
        {
            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<List<BTUser>> UsersNotInRoleAsync(string roleName)
        {
            var userDB = _context.Users;
            List<BTUser> usersNotInRole = new();
            try{
                // TODO Modify for multi tenancy
                foreach (var user in _context.Users.ToList())
                {
                    if (!await _userManager.IsInRoleAsync(user, roleName))
                    {
                        usersNotInRole.Add(user);
                    }
                }
            }
            catch
            {
                throw;

            }
            return usersNotInRole;
        }
    }
}
