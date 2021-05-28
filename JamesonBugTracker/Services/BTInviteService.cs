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
    public class BTInviteService : IBTInviteService
    {
        private readonly ApplicationDbContext _context;

        public BTInviteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<bool> AcceptInviteAsync(Guid? token, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnyInviteAsync(Guid token, string email)
        {
            throw new NotImplementedException();
        }

        public Task<Invite> GetInviteAsync(Guid token, string email)
        {
            throw new NotImplementedException();
        }

        public async Task<Invite> GetInviteAsync(int id)
        {
            return await _context.Invite.FirstOrDefaultAsync(i => i.Id == id);
        }

        public Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            throw new NotImplementedException();
        }
    }
}
