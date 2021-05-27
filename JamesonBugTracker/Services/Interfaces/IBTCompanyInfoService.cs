using JamesonBugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Services.Interfaces
{
    public interface IBTCompanyInfoService
    {
        Task<Company> GetCompanyInfoByIdAsync(int? companyId);

        Task<List<BTUser>> GetAllMembersAsync(int companyId);

        Task<List<BTUser>> GetAllProjectsAsync(int companyId);

        Task<List<BTUser>> GetAllTicketsAsync(int companyId);

        Task<List<BTUser>> GetMembersInRoleAsync(string roleName, int companyId);
    }
}
