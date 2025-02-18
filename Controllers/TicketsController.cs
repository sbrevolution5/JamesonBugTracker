﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
using JamesonBugTracker.Extensions;
using JamesonBugTracker.Models.ViewModels;
using JamesonBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace JamesonBugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTTicketService _ticketService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTHistoryService _historyService;
        private readonly IBTNotificationService _notificationService;

        public TicketsController(ApplicationDbContext context, IBTTicketService ticketService, UserManager<BTUser> userManager, IBTProjectService projectService, IBTCompanyInfoService companyInfoService, IBTHistoryService historyService, IBTNotificationService notificationService)
        {
            _context = context;
            _ticketService = ticketService;
            _userManager = userManager;
            _projectService = projectService;
            _companyInfoService = companyInfoService;
            _historyService = historyService;
            _notificationService = notificationService;
        }
        public async Task<IActionResult> AllTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var allTickets = (await _companyInfoService.GetAllTicketsAsync(companyId)).Where(t=>!t.Archived);
            return View(allTickets);
        }
        // GET: Tickets/MyTickets
        public async Task<IActionResult> MyTickets()
        {
            string userId = _userManager.GetUserId(User);
            List<Ticket> devTicketsResolved = await _ticketService.GetAllDeveloperTicketsByResolvedAsync(userId, true);
            List<Ticket> devTicketsUnresolved = await _ticketService.GetAllDeveloperTicketsByResolvedAsync(userId, false);
            List<Ticket> subTickets = await _ticketService.GetAllTicketsByRoleAsync("Submitter", userId);
            MyTicketsViewModel viewModel = new()
            {
                DevTicketsResolved = devTicketsResolved,
                DevTicketsUnresolved = devTicketsUnresolved,
                SubmittedTickets = subTickets,
            };
            return View(viewModel);
        }
        public async Task<IActionResult> MyArchivedTickets()
        {
            string userId = _userManager.GetUserId(User);
            List<Ticket> subTicketsArchived = await _ticketService.GetArchivedUserTicketsAsync(userId, "Submitter");
            List<Ticket> devTicketsArchived = await _ticketService.GetArchivedUserTicketsAsync(userId, "Developer");
            MyArchivedTicketsViewModel viewModel = new()
            {
                DevTicketsArchived = devTicketsArchived,
                SubTicketsArchived = subTicketsArchived
            };
            return View(viewModel);
        }
        public async Task<IActionResult> AllArchivedTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            List<Ticket> archivedTickets = await _ticketService.GetArchivedTicketsByCompanyAsync(companyId);
            
            return View(archivedTickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            int companyId = User.Identity.GetCompanyId().Value;
            var ticket = await _context.Ticket
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .ThenInclude(p => p.Company)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.Attachments)
                .Include(t => t.Comments)
                .ThenInclude(c => c.User)
                .Include(t => t.History)
                .ThenInclude(h=>h.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            ViewData["AssignUsers"] = new SelectList(await _projectService.GetAssignableMembersAsync(ticket.ProjectId), "Id", "FullName", ticket.DeveloperUserId);
            if (ticket.Project.CompanyId != companyId)
            {
                return NotFound();
            }
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create(int? id, bool db = false)
        {
            BTUser user = await _context.Users.Include(u=>u.Projects).FirstOrDefaultAsync(u=> u.Id == _userManager.GetUserId(User));
            int companyId = User.Identity.GetCompanyId().Value;
            Ticket ticket = new();
            if (user.Projects.Count == 0 && !User.IsInRole("Admin") && !User.IsInRole("ProjectManager"))
            {
                TempData["StatusMessage"] = "Error: You aren't assigned to any projects, please contact an Admin or Project Manager to ensure you are assigned before creating tickets";
                return RedirectToAction("Dashboard", "Home");
            }
            if (User.IsInRole("Admin"))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllUnarchivedProjectsByCompanyAsync(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.ListUserUnarchivedProjectsAsync(user.Id), "Id", "Name");
            }
            if (id is not null)
            {
                ticket.ProjectId = id.Value;
            }
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>().OrderBy(t=>t.Id), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>().OrderBy(t=>t.Name), "Id", "Name");
            ViewData["ReturnDb"] = db;
            return View(ticket);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,ProjectId,TicketPriorityId,TicketTypeId")] Ticket ticket, bool db)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            if (ModelState.IsValid)
            {
                BTUser btUser = await _userManager.GetUserAsync(User);
                ticket.OwnerUserId = btUser.Id;
                ticket.Created = DateTime.Now;
                ticket.Updated = DateTime.Now;
                ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync("New")).Value;
                await _context.AddAsync(ticket);
                await _context.SaveChangesAsync();
                #region Add History
                await _historyService.AddHistoryAsync(null, await _ticketService.GetOneTicketNotTrackedAsync(ticket.Id), ticket.OwnerUserId);
                #endregion
                #region Notification
                BTUser projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                Notification notification = new()
                {
                    TicketId = ticket.Id,
                    SenderId = btUser.Id,
                    Created = DateTime.Now,
                    Message = $"created a new ticket: {ticket.Title}",
                    Title = "New Ticket"
                };
                if (projectManager is not null)
                {

                    try
                    {
                        notification.RecipientId = projectManager.Id;
                        await _notificationService.SaveNotificationAsync(notification);

                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    //await _notificationService.MembersNotificationAsync(notification, projectManager);
                }
                #endregion
                if (db == true)
                {
                    return RedirectToAction("Details", "Projects", new { id = ticket.ProjectId });
                }
                else
                {                   
                    return RedirectToAction("Details", new { id = ticket.Id });
                }
            }
            BTUser user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Admin"))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllUnarchivedProjectsByCompanyAsync(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList((await _projectService.ListUserProjectsAsync(user.Id)).Where(p => !p.Archived), "Id", "Name");
            }
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>().OrderBy(t => t.Id), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>().OrderBy(t => t.Name), "Id", "Name");
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize(Roles = "Admin,ProjectManager,Developer,Submitter")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            if (User.IsInRole("DemoUser"))
            {
                return RedirectToAction("DemoError", "Home");
            }
            BTUser user = await _userManager.GetUserAsync(User);
            int companyId = User.Identity.GetCompanyId().Value;
            if (User.IsInRole("ProjectManager") && (await _projectService.GetProjectManagerAsync(ticket.ProjectId)).Id != user.Id)
            {
                TempData["StatusMessage"] = "Error:  You cannot edit a ticket that is not a part of your project.";
                return RedirectToAction("Details", new { id = ticket.Id });
            }
            if ((User.IsInRole("Developer") && user.Id != ticket.DeveloperUserId )|| (User.IsInRole("Submitter") && user.Id != ticket.OwnerUserId))
            {
                TempData["StatusMessage"] = "Error:  You cannot edit a ticket that you did not submit or are not developing!";
                return RedirectToAction("Details", new { id = ticket.Id });
            }
            if (User.IsInRole("Admin"))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllUnarchivedProjectsByCompanyAsync(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList((await _projectService.ListUserProjectsAsync(user.Id)).Where(p=>!p.Archived), "Id", "Name");
            }
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>().OrderBy(t => t.Id), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>().OrderBy(t => t.Name), "Id", "Name",ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Created,Updated,Description,ProjectId,TicketPriorityId,TicketStatusId,TicketTypeId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (User.IsInRole("DemoUser"))
            {
                return RedirectToAction("DemoError", "Home");
            }
            if (id != ticket.Id)
            {
                return NotFound();
            }

            Notification notification;
            if (ModelState.IsValid)
            {
                //get companyid, currentuser, project manager OLD
                int companyId = User.Identity.GetCompanyId().Value;
                BTUser projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                BTUser currentUser = await _userManager.GetUserAsync(User);

                //Ticket oldTicket = await _ticketService.GetOneTicketNotTrackedAsync(ticket.Id);
                try
                {
                    ticket.Updated = DateTime.Now;
                    ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync("New")).Value;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                    notification = new()
                    {
                        TicketId = ticket.Id,
                        Title = $"Ticket modified on project - {ticket.Project.Name}", 
                        Message = $"Ticket: [{ticket.Id}]:{ticket.Title} updated by {currentUser?.FullName}",
                        Created = DateTime.Now,
                        SenderId = currentUser?.Id,
                        RecipientId = projectManager?.Id
                    }; 
                    if (projectManager is not null)
                    {

                        try
                        {
                            notification.RecipientId = projectManager.Id;
                            await _notificationService.SaveNotificationAsync(notification);

                        }
                        catch (Exception)
                        {

                            throw;
                        }
                        //await _notificationService.MembersNotificationAsync(notification, projectManager);
                    }
                    else
                    {
                        await _notificationService.AdminsNotificationAsync(notification, companyId);
                    }
                    if (ticket.DeveloperUserId != null)
                    {
                        notification = new()
                        {
                            TicketId = ticket.Id,
                            Title = "A Ticket assigned to you was updated",
                            Message = $"updated ticket: [{ticket.Id}]:{ticket.Title}",
                            Created = DateTimeOffset.Now,
                            SenderId = currentUser?.Id,
                            RecipientId = ticket.DeveloperUserId
                        };
                        await _notificationService.SaveNotificationAsync(notification);
                        await _notificationService.EmailNotificationAsync(notification, notification.Title);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                Ticket newTicket = await _ticketService.GetOneTicketNotTrackedAsync(ticket.Id);
                //await _historyService.AddHistoryAsync(oldTicket, newTicket, currentUser.Id);
                return RedirectToAction("Details", "Projects", new { id = ticket.ProjectId });

            }
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Description", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>().OrderBy(t => t.Id), "Id", "Name",ticket.TicketPriorityId);
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>().OrderBy(t => t.Name), "Id", "Name",ticket.TicketTypeId);
            return View(ticket);
        }
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            UnassignedTicketsViewModel viewModel = new()
            {
                Projects = await _projectService.GetProjectsWithUnassignedTicketsAsync(companyId),
                UserSelectLists = new(),
                UnassignedTickets = await _ticketService.GetAllUnassignedTicketsAsync(companyId)
            };

            return View(viewModel);
        }
        // GET: Tickets/Delete/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }


        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Archive")]
        [Authorize(Roles = "Admin,ProjectManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (User.IsInRole("DemoUser"))
            {
                return RedirectToAction("DemoError", "Home");
            }
            var ticket = await _context.Ticket.FindAsync(id);
            ticket.ArchiveDate = DateTime.Now;
            ticket.Archived = true;
            await _context.SaveChangesAsync();
            await _ticketService.SetTicketStatusAsync(id, "Archived");
            TempData["StatusMessage"] = $"{ticket.Title} was successfully archived";
            return RedirectToAction("Dashboard", "Home");
        }
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> UnArchive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }
            if (ticket.Project.Archived == true)
            {
                TempData["StatusMessage"] = "Error: The project the ticket belongs to is archived.  You must unarchive this project before unarchiving the ticket";

                return RedirectToAction("UnArchive", "Projects", new { id = ticket.ProjectId});

            }

            return View(ticket);
        }


        // POST: Tickets/Delete/5
        [HttpPost, ActionName("UnArchive")]
        [Authorize(Roles = "Admin,ProjectManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnArchiveConfirmed(int id)
        {
            if (User.IsInRole("DemoUser"))
            {
                return RedirectToAction("DemoError", "Home");
            }
            var ticket = await _context.Ticket.FindAsync(id);
            ticket.ArchiveDate = null;
            ticket.Archived = false;
            await _context.SaveChangesAsync();
            await _ticketService.SetTicketStatusAsync(id, "Development");
            return RedirectToAction("Details", "Tickets", new { id = ticket.Id});
        }
        [HttpPost]
        [Authorize(Roles = "Admin,ProjectManager")]

        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUser(string userId, int ticketId, bool db = false)
        {

            try
            {
                BTUser currentUser = await _userManager.GetUserAsync(User);
                Ticket oldTicket = await _ticketService.GetOneTicketNotTrackedAsync(ticketId);
                await _ticketService.AssignTicketAsync(ticketId, userId);
                Ticket newTicket = await _ticketService.GetOneTicketNotTrackedAsync(ticketId);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, currentUser.Id);
                #region Alert New and old user that ticket was changed 
                if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
                {
                    Notification notification = new()
                    {
                        TicketId = newTicket.Id,
                        Title = "You were assigned a New Ticket",
                        Message = $" just assigned you to {newTicket.Id}:{newTicket.Title}",
                        SenderId = currentUser.Id,
                        RecipientId = newTicket.DeveloperUserId,
                        Created = DateTimeOffset.Now,
                    };
                    await _notificationService.SaveNotificationAsync(notification);
                    //If user assigns themselves, they won't get an email.
                    if (newTicket.DeveloperUserId != currentUser.Id)
                    {

                        await _notificationService.EmailNotificationAsync(notification, notification.Title);
                    }else
                    {
                        notification.Viewed = true;
                        await _context.SaveChangesAsync();
                    }
                    notification = new()
                    {
                        TicketId = newTicket.Id,
                        Title = "You were unassigned from a ticket",
                        Message = $" just assigned {newTicket.Id}:{newTicket.Title} to {newTicket.DeveloperUser.FullName} instead of you.",
                        SenderId = currentUser.Id,
                        RecipientId = oldTicket.DeveloperUserId,
                        Created = DateTimeOffset.Now,
                    };
                    await _notificationService.SaveNotificationAsync(notification);
                    //if user unassigns themselves they won't get an email
                    if (oldTicket.DeveloperUserId != currentUser.Id && oldTicket.DeveloperUserId is not null)
                    {

                        await _notificationService.EmailNotificationAsync(notification, notification.Title);
                    }
                }
                #endregion
            }
            catch { throw; }
            if (db)
            {
                return RedirectToAction("Dashboard", "Home");

            }
            return RedirectToAction("Details", new { id = ticketId });
        }
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> UnassignUser(int ticketId)
        {

            try
            {
                BTUser currentUser = await _userManager.GetUserAsync(User);
                Ticket oldTicket = await _ticketService.GetOneTicketNotTrackedAsync(ticketId);
                await _ticketService.UnassignTicketAsync(ticketId);
                Ticket newTicket = await _ticketService.GetOneTicketNotTrackedAsync(ticketId);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, currentUser.Id);
                #region Alert New and old user that ticket was changed 
                if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
                {
                    Notification notification = new()
                    {
                        TicketId = newTicket.Id,
                        Title = "You were unassigned from a ticket",
                        Message = $" removed you from {newTicket.Id}:{newTicket.Title} ",
                        SenderId = currentUser.Id,
                        RecipientId = oldTicket.DeveloperUserId,
                        Created = DateTimeOffset.Now,
                    };
                    await _notificationService.SaveNotificationAsync(notification);
                    //if user unassigns themselves they won't get an email
                    if (oldTicket.DeveloperUserId != currentUser.Id && oldTicket.DeveloperUserId is not null)
                    {

                        await _notificationService.EmailNotificationAsync(notification, notification.Title);
                    }
                }
                #endregion
            }
            catch { throw; }
            return RedirectToAction("Details", new { id = ticketId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int ticketId, string statusName)
        {
            try
            {
                BTUser currentUser = await _userManager.GetUserAsync(User);
                Ticket oldTicket = await _ticketService.GetOneTicketNotTrackedAsync(ticketId);
                await _ticketService.SetTicketStatusAsync(ticketId, statusName);
                Ticket newTicket = await _ticketService.GetOneTicketNotTrackedAsync(ticketId);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, currentUser.Id);
                if (statusName == "Resolved")
                {
                    Ticket ticket = await _context.Ticket.Where(t => t.Id == ticketId).FirstOrDefaultAsync();
                    ticket.Completed = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

            }
            catch { throw; }
            if (statusName == "Resolved")
            {
                return RedirectToAction("MyTickets");
            }
            return RedirectToAction("Details", new { id = ticketId });
        }
        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.Id == id);
        }
        public IActionResult ShowFile(int id)
        {
            TicketAttachment ticketAttachment = _context.TicketAttachment.Find(id);
            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }
    }
}
