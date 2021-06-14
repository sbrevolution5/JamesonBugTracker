using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
using JamesonBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Services
{
    public class BTNotificationService : IBTNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<BTUser> _userManager;

        public BTNotificationService(ApplicationDbContext context, IEmailSender emailSender, UserManager<BTUser> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public async Task AdminsNotificationAsync(Notification notification, int companyId)
        {
            
                List<BTUser> allUsers = (await _userManager.GetUsersInRoleAsync("Admin")).ToList();
                List<BTUser> usersInRole = allUsers.Where(u => u.CompanyId == companyId).ToList();
            foreach (BTUser user in usersInRole)
            {
                notification.RecipientId = user.Id;
                //await SaveNotificationAsync(notification);
                await EmailNotificationAsync(notification, notification.Title);
                
            }
            
        }

        public async Task EmailNotificationAsync(Notification notification, string emailSubject)
        {
            BTUser btUser = await _context.Users.FindAsync(notification.RecipientId);
            string btUserEmail = btUser?.Email;
            BTUser sender = await _context.Users.FindAsync(notification.SenderId);
            string message = $"{notification.Sender.FullName} {notification.Message}";
            try
            {

            await _emailSender.SendEmailAsync(btUserEmail, emailSubject, message);
            }
            catch { throw; }
        }

        public async Task<List<Notification>> GetReceivedNotificationsAsync(string userId)
        {
            List<Notification> recievedNotifiactions = await _context.Notification
                                                                        .Include(n => n.Sender)
                                                                        .Include(n => n.Recipient)
                                                                        .Include(n => n.Ticket)
                                                                            .ThenInclude(t => t.Project)
                                                                        .Include(n => n.Ticket)
                                                                            .ThenInclude(t => t.TicketStatus)
                                                                        .Include(n => n.Ticket)
                                                                            .ThenInclude(t => t.TicketPriority)
                                                                        .Include(n => n.Ticket)
                                                                            .ThenInclude(t => t.TicketType)
                                                                        .Include(n => n.Ticket)
                                                                            .ThenInclude(t => t.Attachments)
                                                                        .Include(n => n.Ticket)
                                                                            .ThenInclude(t => t.Comments)
                                                                        .Where(n => n.RecipientId == userId).ToListAsync();
            return recievedNotifiactions;
        }

        public async Task<List<Notification>> GetSentNotificationsAsync(string userId)
        {
            List<Notification> sentNotifications = await _context.Notification
                                                                        .Include(n => n.Sender)
                                                                        .Include(n => n.Recipient)
                                                                        .Include(n => n.Ticket)
                                                                            .ThenInclude(t=>t.Project)
                                                                        .Include(n => n.Ticket)
                                                                            .ThenInclude(t=>t.TicketStatus)
                                                                        .Include(n => n.Ticket)
                                                                            .ThenInclude(t=>t.TicketPriority)
                                                                        .Include(n => n.Ticket)
                                                                            .ThenInclude(t=>t.TicketType)
                                                                        .Include(n => n.Ticket)
                                                                            .ThenInclude(t=>t.Attachments)
                                                                        .Include(n => n.Ticket)
                                                                            .ThenInclude(t=>t.Comments)
                                                                        .Where(n => n.SenderId == userId).ToListAsync();
            return sentNotifications;
        }
        public async Task<List<Notification>> GetUnseenRecievedNotificationsAsync(string userId)
        {
            var recieved = await GetReceivedNotificationsAsync(userId);
            return recieved.Where(n => n.Viewed == false).ToList();
        }
        public async Task MembersNotificationAsync(Notification notification, List<BTUser> members)
        {
            try
            {
                foreach (var btUser in members)
                {
                    notification.RecipientId = btUser.Id;
                    //await SaveNotificationAsync(notification);
                    await EmailNotificationAsync(notification, "New Notification from Jameson Bug Tracker");
                }
            }
            catch
            {
                throw;
                throw;
            }
        }

        public async Task SaveNotificationAsync(Notification notification)
        {
            try{
                await _context.AddAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        public Task SMSNotificationAsync(string phone, Notification notification)
        {
            throw new NotImplementedException();
        }
    }
}
