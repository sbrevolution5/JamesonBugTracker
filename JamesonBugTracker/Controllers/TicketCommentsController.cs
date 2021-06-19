using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
using Microsoft.AspNetCore.Identity;
using JamesonBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace JamesonBugTracker.Controllers
{
        [Authorize]
    public class TicketCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTHistoryService _historyService;
        private readonly IBTNotificationService _notificationService;
        private readonly IBTProjectService _projectService;




        public TicketCommentsController(ApplicationDbContext context, UserManager<BTUser> userManager, IBTTicketService ticketService, IBTHistoryService historyService, IBTNotificationService notificationService, IBTProjectService projectService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _historyService = historyService;
            _notificationService = notificationService;
            _projectService = projectService;
        }

        // GET: TicketComments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TicketComment.Include(t => t.Ticket).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TicketComments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketComment = await _context.TicketComment
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketComment == null)
            {
                return NotFound();
            }

            return View(ticketComment);
        }

        // GET: TicketComments/Create
        public IActionResult Create()
        {
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Description");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: TicketComments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Comment,TicketId")] TicketComment ticketComment)
        {
            if (ModelState.IsValid)
            {

                var userId = _userManager.GetUserId(User);
                var oldTicket = await _ticketService.GetOneTicketNotTrackedAsync(ticketComment.TicketId);
                if ((User.IsInRole("Developer") && userId != oldTicket.DeveloperUserId) || (User.IsInRole("Submitter") && userId != oldTicket.OwnerUserId))
                {
                    TempData["StatusMessage"] = "Error:  You cannot comment on a ticket that you did not submit or are not developing!";
                    return RedirectToAction("Details", new { id = oldTicket.Id });
                }
                BTUser projectManager = await _projectService.GetProjectManagerAsync(oldTicket.ProjectId);
                ticketComment.UserId = userId;
                ticketComment.Created = DateTime.Now;
                _context.Add(ticketComment);
                await _context.SaveChangesAsync();
                var newTicket = await _ticketService.GetOneTicketNotTrackedAsync(ticketComment.TicketId);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, userId);
                if (ticketComment.UserId != newTicket.DeveloperUserId)
                {
                    Notification notification = new()
                    {
                        TicketId = newTicket.Id,
                        Title = $"{ticketComment.User.FullName} commented on {newTicket.Title}",
                        Message = $" commented on \"{newTicket.Title}\"",
                        Created = DateTimeOffset.Now,
                        SenderId = userId,
                        RecipientId = newTicket.DeveloperUserId
                    };
                    await _notificationService.SaveNotificationAsync(notification);
                }
                if (ticketComment.UserId != projectManager.Id)
                {
                    Notification notification = new()
                    {
                        TicketId = newTicket.Id,
                        Title = $" commented on \"{newTicket.Title}\"",

                        Message = $" commented on \"{newTicket.Title}\"",
                        Created = DateTimeOffset.Now,
                        SenderId = userId,
                        RecipientId = projectManager.Id
                    };
                    await _notificationService.SaveNotificationAsync(notification);
                }
                return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
            }
            return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
        }

        // GET: TicketComments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketComment = await _context.TicketComment.FindAsync(id);
            if (ticketComment == null)
            {
                return NotFound();
            }
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Description", ticketComment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketComment.UserId);
            return View(ticketComment);
        }

        // POST: TicketComments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
        {
            if (id != ticketComment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketComment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketCommentExists(ticketComment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Description", ticketComment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketComment.UserId);
            return View(ticketComment);
        }

        // GET: TicketComments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketComment = await _context.TicketComment
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketComment == null)
            {
                return NotFound();
            }

            return View(ticketComment);
        }

        // POST: TicketComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketComment = await _context.TicketComment.FindAsync(id);
            _context.TicketComment.Remove(ticketComment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketCommentExists(int id)
        {
            return _context.TicketComment.Any(e => e.Id == id);
        }
    }
}
