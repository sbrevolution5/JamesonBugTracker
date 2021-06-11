using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
using JamesonBugTracker.Models.ViewModels;
using JamesonBugTracker.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using JamesonBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace JamesonBugTracker.Controllers
{
        [Authorize]
    public class InvitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDataProtector _protector;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IEmailSender _emailService;
        private readonly IBTInviteService _inviteService;
        public InvitesController(ApplicationDbContext context, IDataProtector protector, UserManager<BTUser> userManager, IBTProjectService projectService, IEmailSender emailService, IBTInviteService inviteService)
        {
            _context = context;
            _protector = protector;
            _userManager = userManager;
            _projectService = projectService;
            _emailService = emailService;
            _inviteService = inviteService;
        }

        // GET: Invites
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Invite.Include(i => i.Company).Include(i => i.Project).Include(i => i.Recipient).Include(i => i.Sender);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Invites/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invite
                .Include(i => i.Company)
                .Include(i => i.Project)
                .Include(i => i.Recipient)
                .Include(i => i.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // GET: Invites/Create
        public async Task<IActionResult> Create()
        {
            InviteViewModel model = new();

            if (User.IsInRole("Admin"))
            {
                model.ProjectsList = new SelectList(_context.Project, "Id", "Name");
            }
            else if (User.IsInRole("ProjectManager"))
            {
                string userId = _userManager.GetUserId(User);
                List<Project> projects = await _projectService.ListUserProjectsAsync(userId);
                model.ProjectsList = new SelectList(projects, "Id", "Name");
            }

            return View(model);
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(InviteViewModel viewModel)
        {
            var companyId = User.Identity.GetCompanyId();

            Guid guid = Guid.NewGuid();

            var token = _protector.Protect(guid.ToString());
            var email = _protector.Protect(viewModel.Email);

            var callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email }, protocol: Request.Scheme);

            var body = "Please join my Company." + Environment.NewLine + "Please click the following link to join <a href=\"" + callbackUrl + "\">COLLABORATE</a>";
            var destination = viewModel.Email;
            var subject = "Company Invite";


            await _emailService.SendEmailAsync(destination, subject, body);

            //Create record in the Invites table
            Invite model = new()
            {
                RecipientEmail = viewModel.Email,
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                CompanyToken = guid,
                CompanyId = companyId.Value,
                ProjectId = viewModel.ProjectId,
                InviteDate = DateTimeOffset.Now,
                SenderId = _userManager.GetUserId(User),
                IsValid = true
            };

            _context.Invite.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Dashboard", "Home");
        }

        // GET: Invites/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invite.FindAsync(id);
            if (invite == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", invite.CompanyId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Description", invite.ProjectId);
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id", invite.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", invite.SenderId);
            return View(invite);
        }
        public async Task<IActionResult> ProcessInvite(string token, string email)
        {
            if (token == null)
            {
                return NotFound();
            }

            Guid companyToken = Guid.Parse(_protector.Unprotect(token));
            string inviteeEmail = _protector.Unprotect(email);

            //Use InviteService to validate invite code 
            Invite invite = await _inviteService.GetInviteAsync(companyToken, inviteeEmail);

            if (invite != null)
            {
                return View(invite);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessInvite(Invite invite)
        {
            return RedirectToPage("RegisterByInvite", new { invite });
        }
        // POST: Invites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InviteDate,CompanyToken,CompanyId,ProjectId,RecipientId,SenderId,RecipientEmail,FirstName,LastName,IsValid")] Invite invite)
        {
            if (id != invite.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InviteExists(invite.Id))
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
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", invite.CompanyId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Description", invite.ProjectId);
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id", invite.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", invite.SenderId);
            return View(invite);
        }

        // GET: Invites/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invite
                .Include(i => i.Company)
                .Include(i => i.Project)
                .Include(i => i.Recipient)
                .Include(i => i.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // POST: Invites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invite = await _context.Invite.FindAsync(id);
            _context.Invite.Remove(invite);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InviteExists(int id)
        {
            return _context.Invite.Any(e => e.Id == id);
        }
    }
}
