using JamesonBugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Services.Interfaces
{
    public interface IBTRolesService
    {
        public Task<bool> IsUserInRoleAsync(BTUser user, string roleName);
        public Task<IEnumerable<string>> ListUserRolesAsync(BTUser user);

        public Task<string> GetRoleNameByIdAsync(string roleId);
        public Task<bool> AddUserToRoleAsync(BTUser user, string roleName);

        public Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName);

        public Task<List<BTUser>> UsersNotInRoleAsync(string roleName, int companyId);
        public Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roles);
    }
}
