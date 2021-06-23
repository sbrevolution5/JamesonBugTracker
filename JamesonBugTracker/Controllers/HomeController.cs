using JamesonBugTracker.Data;
using JamesonBugTracker.Extensions;
using JamesonBugTracker.Models;
using JamesonBugTracker.Models.ViewModels;
using JamesonBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;

        public HomeController(ILogger<HomeController> logger, IBTCompanyInfoService companyInfoService, IBTProjectService projectService, IBTTicketService ticketService, UserManager<BTUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _companyInfoService = companyInfoService;
            _projectService = projectService;
            _ticketService = ticketService;
            _userManager = userManager;
            _context = context;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        //GET: Dashboard
        public async Task<IActionResult> Dashboard()
        {
            
            string userId = _userManager.GetUserId(User);
            int companyId = User.Identity.GetCompanyId().Value;

            List<Ticket> companyTickets = (await _companyInfoService.GetAllTicketsAsync(companyId)).Where(t=>!t.Archived).ToList();
            List<Project> companyProjects = await _companyInfoService.GetAllProjectsAsync(companyId);
            DashboardViewModel viewModel = new()
            {
                Projects = companyProjects.Where(p=>!p.Archived).ToList(),
                SubmittedTickets = companyTickets.Where(t => t.OwnerUserId == userId).ToList(),
                Members = await _companyInfoService.GetAllMembersAsync(companyId),
                CurrentUser = await _userManager.GetUserAsync(User),
                UnassignedTickets = companyTickets.Where(p=>p.DeveloperUserId == null).ToList(),
            };
            viewModel.UnresolvedDevelopmentTickets = companyTickets.Where(t => t.OwnerUserId == userId).Where(t=>t.TicketStatus.Name !="Resolved").ToList();
            if (viewModel.UnresolvedDevelopmentTickets.Count == 0)
            {
                viewModel.UnresolvedDevelopmentTickets = companyTickets;
                viewModel.HasNoDevTickets = true;
            }
            viewModel.DevelopmentTicketsCount = companyTickets.Count;
            return View(viewModel);
        }
        
        [HttpPost]
        public async Task<JsonResult> ProjChartMethod(bool dev)
        {
            //bool dev, int userId
            int companyId = User.Identity.GetCompanyId().Value;
            string userId = _userManager.GetUserId(User);

            List<Project> projects = (await _projectService.GetAllProjectsByCompanyAsync(companyId)).OrderBy(p => p.Id).ToList();

            DonutViewModel chartData = new();
            chartData.labels = projects.Select(p => p.Name).ToArray();

            List<SubData> dsArray = new();
            List<int> tickets = new();
            List<string> colors = new();
            //Antonio's Random Colors
            foreach (Project prj in projects)
            {
                if (dev)
                {

                    tickets.Add(prj.Tickets.Where(t=>t.DeveloperUserId == userId).Count());
                }
                else
                {
                    tickets.Add(prj.Tickets.Where(t => t.OwnerUserId == userId).Count());
                }

                //    // This code will randomly select a color for each element of the data 
                //    Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                //    string colorHex = string.Format("#{0:X6}", randomColor.ToArgb() & 0X00FFFFFF);

                //    colors.Add(colorHex);
            }

            SubData temp = new()
            {
                data = tickets.ToArray(),
                backgroundColor = colors.ToArray()
            };
            dsArray.Add(temp);

            chartData.datasets = dsArray.ToArray();

            return Json(chartData);
        }
        [HttpPost]
        public async Task<JsonResult> StatusChartMethod(bool dev)
        {
            //bool dev, int userId
            int companyId = User.Identity.GetCompanyId().Value;
            string userId = _userManager.GetUserId(User);
            Random rnd = new();

           
            List<Ticket> tickets;
            if (dev)
            {

                tickets = await _context.Ticket.Where(t => t.DeveloperUserId == userId && !t.Archived).ToListAsync();
            }
            else
            {
                tickets = await _context.Ticket.Where(t => t.OwnerUserId == userId && !t.Archived).ToListAsync();

            }
            var statuses = _context.TicketStatus.ToList();
            DonutViewModel chartData = new();
            chartData.labels = statuses.Select(t => t.Name).ToArray();

            List<SubData> dsArray = new();
            List<int> howManyTickets = new();
            List<string> colors = new();
            //Antonio's Random Colors
            foreach (TicketStatus status in statuses)
            {
                howManyTickets.Add(tickets.Where(t => t.TicketStatusId == status.Id).Count());

                //    // This code will randomly select a color for each element of the data 
                //    Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                //    string colorHex = string.Format("#{0:X6}", randomColor.ToArgb() & 0X00FFFFFF);

                //    colors.Add(colorHex);
            }

            SubData temp = new()
            {
                data = howManyTickets.ToArray(),
                backgroundColor = colors.ToArray()
            };
            dsArray.Add(temp);

            chartData.datasets = dsArray.ToArray();

            return Json(chartData);
        }
        [HttpPost]
        public async Task<JsonResult> PriorityChartMethod(bool dev)
        {
            //bool dev, int userId
            int companyId = User.Identity.GetCompanyId().Value;
            string userId = _userManager.GetUserId(User);
            List<Ticket> tickets;
            if (dev)
            {

                tickets = await _context.Ticket.Where(t => t.DeveloperUserId == userId && !t.Archived).ToListAsync();
            }
            else
            {
                tickets = await _context.Ticket.Where(t => t.OwnerUserId == userId && !t.Archived).ToListAsync();

            }
            List<TicketPriority> priorities = _context.TicketPriority.ToList();
            DonutViewModel chartData = new();
            chartData.labels = priorities.Select(t => t.Name).ToArray();

            List<SubData> dsArray = new();
            List<int> howManyTickets = new();
            List<string> colors = new();
            //Antonio's Random Colors
            foreach (TicketPriority priority in priorities)
            {
                howManyTickets.Add(tickets.Where(t => t.TicketPriorityId == priority.Id).Count());

                //    // This code will randomly select a color for each element of the data 
                //    Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                //    string colorHex = string.Format("#{0:X6}", randomColor.ToArgb() & 0X00FFFFFF);

                //    colors.Add(colorHex);
            }

            SubData temp = new()
            {
                data = howManyTickets.ToArray(),
                backgroundColor = colors.ToArray()
            };
            dsArray.Add(temp);

            chartData.datasets = dsArray.ToArray();

            return Json(chartData);
        }
        [HttpPost]
        public async Task<JsonResult> TypeChartMethod(bool dev)
        {
            //bool dev, int userId
            int companyId = User.Identity.GetCompanyId().Value;
            string userId = _userManager.GetUserId(User);
            List<Ticket> tickets;
            if (dev)
            {

                tickets = await _context.Ticket.Where(t => t.DeveloperUserId == userId && !t.Archived).ToListAsync();
            }
            else
            {
                tickets = await _context.Ticket.Where(t => t.OwnerUserId == userId && !t.Archived).ToListAsync();

            }
            var types = _context.TicketType.ToList();
            DonutViewModel chartData = new();
            chartData.labels = types.Select(t => t.Name).ToArray();

            List<SubData> dsArray = new();
            List<int> howManyTickets = new();
            List<string> colors = new();
            foreach (TicketType type in types)
            {
                howManyTickets.Add(tickets.Where(t => t.TicketTypeId == type.Id).Count());

            //Antonio's Random Colors
                //    // This code will randomly select a color for each element of the data 
                //    Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                //    string colorHex = string.Format("#{0:X6}", randomColor.ToArgb() & 0X00FFFFFF);

                //    colors.Add(colorHex);
            }

            SubData temp = new()
            {
                data = howManyTickets.ToArray(),
                backgroundColor = colors.ToArray()
            };
            dsArray.Add(temp);

            chartData.datasets = dsArray.ToArray();

            return Json(chartData);
        }
        [HttpPost]
        public async Task<JsonResult> ProjTypeChartMethod(int projId)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            string userId = _userManager.GetUserId(User);
            List<Ticket> tickets = await _ticketService.GetAllTicketsByProjectAsync(projId);
            var types = _context.TicketType.ToList();
            DonutViewModel chartData = new();
            chartData.labels = types.Select(t => t.Name).ToArray();

            List<SubData> dsArray = new();
            List<int> howManyTickets = new();
            List<string> colors = new();
            //Antonio's Random Colors
            foreach (TicketType type in types)
            {
                howManyTickets.Add(tickets.Where(t => t.TicketTypeId == type.Id).Count());

                //    // This code will randomly select a color for each element of the data 
                //    Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                //    string colorHex = string.Format("#{0:X6}", randomColor.ToArgb() & 0X00FFFFFF);

                //    colors.Add(colorHex);
            }

            SubData temp = new()
            {
                data = howManyTickets.ToArray(),
                backgroundColor = colors.ToArray()
            };
            dsArray.Add(temp);

            chartData.datasets = dsArray.ToArray();

            return Json(chartData);
        }
        [HttpPost]
        public async Task<JsonResult> ProjStatusChartMethod(int projId)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            string userId = _userManager.GetUserId(User);
            Random rnd = new();


            List<Ticket> tickets = await _ticketService.GetAllTicketsByProjectAsync(projId);

            var statuses = _context.TicketStatus.ToList();
            DonutViewModel chartData = new();
            chartData.labels = statuses.Select(t => t.Name).ToArray();

            List<SubData> dsArray = new();
            List<int> howManyTickets = new();
            List<string> colors = new();
            //Antonio's Random Colors
            foreach (TicketStatus status in statuses)
            {
                howManyTickets.Add(tickets.Where(t => t.TicketStatusId == status.Id).Count());

                //    // This code will randomly select a color for each element of the data 
                //    Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                //    string colorHex = string.Format("#{0:X6}", randomColor.ToArgb() & 0X00FFFFFF);

                //    colors.Add(colorHex);
            }

            SubData temp = new()
            {
                data = howManyTickets.ToArray(),
                backgroundColor = colors.ToArray()
            };
            dsArray.Add(temp);

            chartData.datasets = dsArray.ToArray();

            return Json(chartData);
        }
        [HttpPost]
        public async Task<JsonResult> ProjPriorityChartMethod(int projId)
        {
            //bool dev, int userId
            int companyId = User.Identity.GetCompanyId().Value;
            string userId = _userManager.GetUserId(User);
            List<Ticket> tickets = await _ticketService.GetAllTicketsByProjectAsync(projId);
            var priorities = _context.TicketPriority.ToList();
            DonutViewModel chartData = new();
            chartData.labels = priorities.Select(t => t.Name).ToArray();

            List<SubData> dsArray = new();
            List<int> howManyTickets = new();
            List<string> colors = new();
            //Antonio's Random Colors
            foreach (TicketPriority priority in priorities)
            {
                howManyTickets.Add(tickets.Where(t => t.TicketPriorityId == priority.Id).Count());

                //    // This code will randomly select a color for each element of the data 
                //    Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                //    string colorHex = string.Format("#{0:X6}", randomColor.ToArgb() & 0X00FFFFFF);

                //    colors.Add(colorHex);
            }

            SubData temp = new()
            {
                data = howManyTickets.ToArray(),
                backgroundColor = colors.ToArray()
            };
            dsArray.Add(temp);

            chartData.datasets = dsArray.ToArray();

            return Json(chartData);
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Landing()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult DemoError()
        {
            return View();
        }
    }
}
