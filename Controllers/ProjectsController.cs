﻿using System;
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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace JamesonBugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTFileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly IBTNotificationService _notificationService;


        public ProjectsController(ApplicationDbContext context, IBTProjectService projectService, UserManager<BTUser> userManager, IBTCompanyInfoService companyInfoService, IBTTicketService ticketService, IBTFileService fileService, IConfiguration configuration, IBTNotificationService notificationService)
        {
            _context = context;
            _projectService = projectService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
            _ticketService = ticketService;
            _fileService = fileService;
            _configuration = configuration;
            _notificationService = notificationService;
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
            var companyProjects = await _projectService.GetAllUnarchivedProjectsByCompanyAsync(companyId);
            return View(companyProjects);
        }
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var companyProjects = await _projectService.GetArchivedProjectsByCompanyAsync(companyId);
            return View(companyProjects);
        }

        // GET: Projects
        public async Task<IActionResult> MyProjects()
        {
            var userId = _userManager.GetUserId(User);
            var userProjects = await _projectService.ListUserUnarchivedProjectsAsync(userId);
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
            ProjectDetailsViewModel viewModel = new()
            {
                Project = project,
                OpenTickets = await _ticketService.GetProjectTicketsNotResolvedOrArchivedAsync(project.CompanyId, project.Id),
                ResolvedTickets = await _ticketService.GetProjectTicketsByStatusAsync("Resolved", project.CompanyId, project.Id),
                ArchivedTickets = await _ticketService.GetProjectTicketsByStatusAsync("Archived", project.CompanyId, project.Id)
            };
            return View(viewModel);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin,ProjectManager")]

        public IActionResult Create()
        {

            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>().OrderBy(t => t.Id), "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,EndDate,ProjectPriorityId")] Project project, IFormFile customFile)
        {
            if (ModelState.IsValid)
            {
                project.CompanyId = User.Identity.GetCompanyId().Value;
                if (customFile is not null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(customFile);
                    project.ImageFileContentType = customFile.ContentType;
                }
                else
                {
                    project.ImageFileData = await _fileService.EncodeFileAsync(_configuration["DefaultProjectImage"]);
                    project.ImageFileContentType = _configuration["DefaultProjectImage"].Split('.')[1];
                }
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction("AddMembers", new { id = project.Id });
            }
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>().OrderBy(t => t.Id), "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin,ProjectManager")]

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
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>().OrderBy(t => t.Id), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]

        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,StartDate,EndDate,ArchiveDate,Archived,ImageFileName,ImageFileData,ImageFileContentType,ProjectPriorityId")] Project project, IFormFile customFile)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (customFile is not null)
                    {
                        using var image = Image.Load(customFile.OpenReadStream());
                        image.Mutate(x => x.Resize(256, 256));
                        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(image, customFile.ContentType);
                        project.ImageFileContentType = customFile.ContentType;
                    }
                    else if (project.ImageFileData is null)
                    {
                        project.ImageFileData = await _fileService.EncodeFileAsync(_configuration["DefaultProjectImage"]);
                        project.ImageFileContentType = _configuration["DefaultProjectImage"].Split('.')[1];
                    }
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
                return RedirectToAction("MyProjects");
            }
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>().OrderBy(t => t.Id), "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AddMembers(int id)
        {
            //We extended Identity 
            int companyId = User.Identity.GetCompanyId().Value;
            ProjectMembersViewModel model = new();
            var project = (await _projectService.GetAllProjectsByCompanyAsync(companyId))
                                               .FirstOrDefault(p => p.Id == id);
            if (User.IsInRole("ProjectManager") && (await _projectService.GetProjectManagerAsync(id)).Id != _userManager.GetUserId(User))
            {
                TempData["StatusMessage"] = "Error: You are not the project manager for this project";
                return RedirectToAction("Details", new { id = id });
            }
            model.Project = project;
            List<BTUser> users = await _companyInfoService.GetAllMembersAsync(companyId);
            List<string> members = project.Members.Select(m => m.Id).ToList();  // we can do this because our project eagerly loaded its members
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
                    var senderId = _userManager.GetUserId(User);
                    //Get list of all project members, but without the project manager. 
                    //these will be removed from project, We do not want to ever remove project manager.
                    List<string> memberIds = (await _projectService.GetMembersWithoutPMAsync(model.Project.Id)).Select(m => m.Id).ToList();
                    foreach (string id in memberIds)
                    {
                        await _projectService.RemoveUserFromProjectAsync(id, model.Project.Id);
                    }

                    foreach (string id in model.SelectedUsers)
                    {
                        //for each selected user not in original list
                        if (!memberIds.Any(i => i == id))
                        {
                            Notification notification = new()
                            {
                                Created = DateTime.Now,
                                Message = $"assigned you to project: {model.Project.Name}",
                                SenderId = senderId,
                                RecipientId = id
                            };
                            await _notificationService.SaveNotificationAsync(notification);
                        }
                        await _projectService.AddUserToProjectAsync(id, model.Project.Id);
                    }

                    return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
                }
                else
                {
                    //send an error message to user, that they didnt'  select anyone
                }
            }
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> RemoveManager(int id)
        {
            await _projectService.RemoveProjectManagerAsync(id);
            return RedirectToAction("Details", new { id = id });
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
            public async Task<IActionResult> AddManager(int id)
        {
            //We extended Identity 
            int companyId = User.Identity.GetCompanyId().Value;
            ProjectManagerViewModel model = new();
            var project = (await _projectService.GetAllProjectsByCompanyAsync(companyId))
                                               .FirstOrDefault(p => p.Id == id);

            model.ProjectId = id;
            List<BTUser> users = await _companyInfoService.GetMembersInRoleAsync("ProjectManager", companyId);
            List<string> members = project.Members.Select(m => m.Id).ToList();  // we can do this because our project eagerly loaded its members
            model.Managers = new SelectList(users, "Id", "FullName", (await _projectService.GetProjectManagerAsync(project.Id)).Id);
            return View(model);
        }
        //POST: AddManager
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddManager(int id, [Bind("ProjectId,NewManagerId,Managers")] ProjectManagerViewModel model)
        {
            if (User.IsInRole("DemoUser"))
            {
                return RedirectToAction("DemoError", "Home");
            }
            if (ModelState.IsValid)
            {
                var oldManager = await _projectService.GetProjectManagerAsync(id);
                    await _projectService.RemoveProjectManagerAsync(model.ProjectId);
                Project project = await _context.Project.FindAsync(id);
                    await _projectService.AddProjectManagerAsync(model.NewManagerId, model.ProjectId);
                if (model.NewManagerId != null && model.NewManagerId != oldManager.Id)
                {
                    var senderId = _userManager.GetUserId(User);
                    //for each selected user not in original list
                    Notification notification = new()
                    {
                        Created = DateTime.Now,
                        Message = $"You are now managing project: {project.Name}",
                        SenderId = senderId,
                        RecipientId = model.NewManagerId
                    };
                    await _notificationService.SaveNotificationAsync(notification);
                    if (oldManager.Id != _userManager.GetUserId(User))
                    {

                        notification = new()
                        {
                            Created = DateTime.Now,
                            Message = $"You are no longer managing project: {project.Name}",
                            SenderId = senderId,
                            RecipientId = oldManager.Id
                        };
                        await _notificationService.SaveNotificationAsync(notification);
                    }
                    await _projectService.AddUserToProjectAsync(model.NewManagerId, model.ProjectId);

                }
                else
                {
                    //send an error message to user, that they didnt'  select anyone
                }
                    return RedirectToAction("Details", "Projects", new { id = model.ProjectId });
            }
            return View(model);
        }
        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin,ProjectManager")]

        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .Include(p => p.Tickets)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (User.IsInRole("DemoUser"))
            {
                return RedirectToAction("DemoError", "Home");
            }
            var project = await _context.Project.Include(p => p.Tickets).FirstOrDefaultAsync(p => p.Id == id);
            project.ArchiveDate = DateTime.Now;
            project.Archived = true;
            foreach (var ticket in project.Tickets)
            {

                ticket.ArchiveDate = DateTime.Now;
                ticket.Archived = true;
                await _ticketService.SetTicketStatusAsync(ticket.Id, "Archived");
            }
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = $"{project.Name} was successfully archived";
            return RedirectToAction("Dashboard", "Home");
        }
        [Authorize(Roles = "Admin,ProjectManager")]

        public async Task<IActionResult> UnArchive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .Include(p => p.Tickets)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]

        public async Task<IActionResult> UnArchiveConfirmed(int id, bool tickets)
        {
            if (User.IsInRole("DemoUser"))
            {
                return RedirectToAction("DemoError", "Home");
            }
            var project = await _context.Project.Include(p => p.Tickets).FirstOrDefaultAsync(p => p.Id == id);
            project.ArchiveDate = null;
            project.Archived = false;
            if (tickets == true)
            {

                foreach (var ticket in project.Tickets)
                {

                    ticket.ArchiveDate = null;
                    ticket.Archived = false;
                    await _ticketService.SetTicketStatusAsync(ticket.Id, "Development");
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = id });
        }

        private bool ProjectExists(int id)
        {
            return _context.Project.Any(e => e.Id == id);
        }
    }
}
