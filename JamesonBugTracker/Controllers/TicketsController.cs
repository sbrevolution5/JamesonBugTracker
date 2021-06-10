using System;
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

namespace JamesonBugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTTicketService _ticketService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTHistoryService _historyService;

        public TicketsController(ApplicationDbContext context, IBTTicketService ticketService, UserManager<BTUser> userManager, IBTProjectService projectService, IBTCompanyInfoService companyInfoService, IBTHistoryService historyService)
        {
            _context = context;
            _ticketService = ticketService;
            _userManager = userManager;
            _projectService = projectService;
            _companyInfoService = companyInfoService;
            _historyService = historyService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Ticket.Include(t => t.DeveloperUser)
                                                      .Include(t => t.OwnerUser)
                                                      .Include(t => t.Project)
                                                      .Include(t => t.TicketPriority)
                                                      .Include(t => t.TicketStatus)
                                                      .Include(t => t.TicketType);
            return View(await applicationDbContext.ToListAsync());
        }
        public async Task<IActionResult> AllTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var allTickets = await _companyInfoService.GetAllTicketsAsync(companyId);
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
                SubmittedTickets = subTickets
            };
            return View(viewModel);
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
                .Include(t => t.Comments)
                .ThenInclude(c => c.User)
                .Include(t => t.History)
                .FirstOrDefaultAsync(m => m.Id == id);
            ViewData["AssignUsers"] = new SelectList(await _projectService.GetMembersWithoutPMAsync(ticket.ProjectId), "Id", "FullName", ticket.DeveloperUserId);

            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create(int? id, bool db = false)
        {
            BTUser user = await _userManager.GetUserAsync(User);
            int companyId = User.Identity.GetCompanyId().Value;
            Ticket ticket = new();
            if (User.IsInRole("Admin"))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.ListUserProjectsAsync(user.Id), "Id", "Name");
            }
            if (id is not null)
            {
                ticket.ProjectId = id.Value;
            }
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name");
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
            if (ModelState.IsValid)
            {
                ticket.OwnerUserId = _userManager.GetUserId(User);
                ticket.Created = DateTime.Now;
                ticket.Updated = DateTime.Now;
                ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync("New")).Value;
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                await _historyService.AddHistoryAsync(null, ticket, ticket.OwnerUserId);
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
            int companyId = User.Identity.GetCompanyId().Value;
            if (User.IsInRole("Admin"))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.ListUserProjectsAsync(user.Id), "Id", "Name");
            }
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name");
            return View(ticket);
        }

        // GET: Tickets/Edit/5
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
            BTUser user = await _userManager.GetUserAsync(User);
            int companyId = User.Identity.GetCompanyId().Value;
            if (User.IsInRole("Admin"))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.ListUserProjectsAsync(user.Id), "Id", "Name");
            }
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name");
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Created,Updated,Description,ProjectId,TicketPriorityId,TicketStatusId,TicketTypeId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                //get companyid, currentuser, project manager OLD
                int companyId = User.Identity.GetCompanyId().Value;
                BTUser projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                BTUser currentUser = await _userManager.GetUserAsync(User);

                Ticket oldTicket = await _ticketService.GetOneTicketNotTrackedAsync(ticket.Id);
                try
                {
                    ticket.OwnerUserId = _userManager.GetUserId(User);
                    ticket.Updated = DateTime.Now;
                    ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync("New")).Value;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
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
                await _historyService.AddHistoryAsync(oldTicket, newTicket, currentUser.Id);
                return RedirectToAction("Details", "Projects", new { id = ticket.ProjectId });

            }
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Description", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Id", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Id", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Id", ticket.TicketTypeId);
            return View(ticket);
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

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Archive")]
        [Authorize(Roles = "Admin,ProjectManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            ticket.ArchiveDate = DateTime.Now;
            await _context.SaveChangesAsync();
            await _ticketService.SetTicketStatusAsync(id, "Archived");
            return RedirectToAction("Dashboard","Home");
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUser(string userId, int ticketId, bool db=false)
        {
            
            try
            {
                BTUser currentUser = await _userManager.GetUserAsync(User);
                Ticket oldTicket = await _ticketService.GetOneTicketNotTrackedAsync(ticketId);
                await _ticketService.AssignTicketAsync(ticketId, userId);
                Ticket newTicket = await _ticketService.GetOneTicketNotTrackedAsync(ticketId);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, currentUser.Id);

            }
            catch { throw; }
            if (db)
            {
                return RedirectToAction("Dashboard","Home");

            }
            return RedirectToAction("Details",new { id = ticketId });
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

            }
            catch { throw; }
            return RedirectToAction("Details",new { id = ticketId });
        }
        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.Id == id);
        }
    }
}
