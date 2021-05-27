using JamesonBugTracker.Models;
using JamesonBugTracker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Services
{
    public class BTCompanyInfoService : IBTCompanyInfoService
    {
        public Task<List<BTUser>> GetAllMembersAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<BTUser>> GetAllProjectsAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<BTUser>> GetAllTicketsAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<Company> GetCompanyInfoByIdAsync(int? companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<BTUser>> GetMembersInRoleAsync(string roleName, int companyId)
        {
            throw new NotImplementedException();
        }
    }
}
