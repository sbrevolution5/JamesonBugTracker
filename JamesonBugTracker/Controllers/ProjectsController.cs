using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
using JamesonBugTracker.Services.Interfaces;
using JamesonBugTracker.Models.ViewModels;
using JamesonBugTracker.Extensions;
using Microsoft.AspNetCore.Identity;

namespace JamesonBugTracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _companyInfoService;


        public ProjectsController(ApplicationDbContext context, IBTProjectService projectService, UserManager<BTUser> userManager, IBTCompanyInfoService companyInfoService)
        {
            _context = context;
            _projectService = projectService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Project.Include(p => p.Company)
                                                       .Include(p => p.ProjectPriority)
                                                       .Include(p => p.Members);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Projects
        public async Task<IActionResult> AllProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var companyProjects = await _companyInfoService.GetAllProjectsAsync(companyId);
            return View(companyProjects);
        }

        // GET: Projects
        public async Task<IActionResult> MyProjects()
        {
            var userId = _userManager.GetUserId(User);
            var userProjects = await _projectService.ListUserProjectsAsync(userId);
            return View(userProjects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Tickets)
                .ThenInclude(t => t.TicketPriority)
                .Include(p => p.Tickets)
                .ThenInclude(t => t.TicketStatus)
                .Include(p => p.Tickets)
                .ThenInclude(t => t.TicketType)
                .Include(p => p.Members)
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {

            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,EndDate,ProjectPriorityId")] Project project)
        {
            if (ModelState.IsValid)
            {
                project.CompanyId = User.Identity.GetCompanyId().Value;
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Set<Company>(), "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Set<Company>(), "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,StartDate,EndDate,ArchiveDate,Archived,ImageFileName,ImageFileData,ImageFileContentType,ProjectPriorityId")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
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
            ViewData["CompanyId"] = new SelectList(_context.Set<Company>(), "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        //[Authorize(Roles = "Admin,ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AddMembers(int id)
        {
            //We extended Identity 
            int companyId = User.Identity.GetCompanyId().Value;
            ProjectMembersViewModel model = new();
            var project = (await _projectService.GetAllProjectsByCompanyAsync(companyId))
                                               .FirstOrDefault(p => p.Id == id);
            model.Project = project;
            List<BTUser> users = await _companyInfoService.GetAllMembersAsync(companyId);
            List<string> members = project.Members.Select(m=>m.Id).ToList();  // we can do this because our project eagerly loaded its members
            model.Users = new MultiSelectList(users, "Id", "FullName", members);
            return View(model);
        }
        //POST: AssignUsers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMembers(ProjectMembersViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedUsers != null)
                {
                    //Get list of all project members, but without the project manager. 
                    //these will be removed from project, We do not want to ever remove project manager.
                    List<string> memberIds = (await _projectService.GetMembersWithoutPMAsync(model.Project.Id)).Select(m => m.Id).ToList();
                    foreach (string id in memberIds)
                    {
                        await _projectService.RemoveUserFromProjectAsync(id, model.Project.Id);
                    }
                    foreach (string id in model.SelectedUsers)
                    {
                        await _projectService.AddUserToProjectAsync(id, model.Project.Id);
                    }
                    return RedirectToAction("Details", "Projects", new {id = model.Project.Id});
                }
                else
                {
                    //send an error message to user, that they didnt'  select anyone
                }
            }
            return View(model);
        }
        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Project.FindAsync(id);
            _context.Project.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Project.Any(e => e.Id == id);
        }
    }
}
