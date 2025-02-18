﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using JamesonBugTracker.Services.Interfaces;

namespace JamesonBugTracker.Controllers
{
        [Authorize]
    public class TicketAttachmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTHistoryService _historyService;
        private readonly IBTProjectService _projectService;

        public TicketAttachmentsController(ApplicationDbContext context, UserManager<BTUser> userManager, IBTTicketService ticketService, IBTHistoryService historyService, IBTProjectService projectService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _historyService = historyService;
            _projectService = projectService;
        }

        // GET: TicketAttachments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TicketAttachment.Include(t => t.Ticket).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TicketAttachments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketAttachment = await _context.TicketAttachment
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketAttachment == null)
            {
                return NotFound();
            }

            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Create
        public IActionResult Create()
        {
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Description");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: TicketAttachments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FormFile,Image,Description,Created,TicketId,UserId")] TicketAttachment ticketAttachment)
        {
            if (ModelState.IsValid)
            {
                BTUser currentUser = await _userManager.GetUserAsync(User);
                int ticketId = ticketAttachment.TicketId;
                Ticket ticket = await _context.Ticket.FirstOrDefaultAsync(t => t.Id == ticketId);
                if ((User.IsInRole("ProjectManager") && currentUser.Id != (await _projectService.GetProjectManagerAsync(ticket.ProjectId)).Id) || 
                    (User.IsInRole("Developer") && currentUser.Id != ticket.DeveloperUserId) || 
                    (User.IsInRole("Submitter") && currentUser.Id != ticket.OwnerUserId)
                    )
                {
                    TempData["StatusMessage"] = "Error: You aren't authorized to put an attachment on this ticket";
                    return RedirectToAction("Details", "Tickets", new { id = ticketId });
                }
                Ticket oldTicket = await _ticketService.GetOneTicketNotTrackedAsync(ticketId);
                MemoryStream ms = new MemoryStream();
                await ticketAttachment.FormFile.CopyToAsync(ms);


                ticketAttachment.FileData = ms.ToArray();
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.Created = DateTimeOffset.Now;
                ticketAttachment.UserId = _userManager.GetUserId(User);


                _context.Add(ticketAttachment);
                await _context.SaveChangesAsync();
                Ticket newTicket = await _ticketService.GetOneTicketNotTrackedAsync(ticketId);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, currentUser.Id);

                return RedirectToAction("Details","Tickets",new {id = ticketAttachment.TicketId });
            }
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Description", ticketAttachment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketAttachment.UserId);
            return View(ticketAttachment);
        }
        // GET: TicketAttachments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketAttachment = await _context.TicketAttachment.FindAsync(id);
            if (ticketAttachment == null)
            {
                return NotFound();
            }
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Description", ticketAttachment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // POST: TicketAttachments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Created,FileName,FileContentType,FileData,TicketId,UserId")] TicketAttachment ticketAttachment)
        {
            if (id != ticketAttachment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketAttachment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketAttachmentExists(ticketAttachment.Id))
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
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Description", ticketAttachment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketAttachment = await _context.TicketAttachment
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketAttachment == null)
            {
                return NotFound();
            }

            return View(ticketAttachment);
        }

        // POST: TicketAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketAttachment = await _context.TicketAttachment.FindAsync(id);
            _context.TicketAttachment.Remove(ticketAttachment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketAttachmentExists(int id)
        {
            return _context.TicketAttachment.Any(e => e.Id == id);
        }
    }
}
