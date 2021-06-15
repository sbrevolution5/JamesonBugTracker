using JamesonBugTracker.Extensions;
using JamesonBugTracker.Models;
using JamesonBugTracker.Models.ViewModels;
using JamesonBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;

        public HomeController(ILogger<HomeController> logger, IBTCompanyInfoService companyInfoService, IBTProjectService projectService, IBTTicketService ticketService, UserManager<BTUser> userManager)
        {
            _logger = logger;
            _companyInfoService = companyInfoService;
            _projectService = projectService;
            _ticketService = ticketService;
            _userManager = userManager;
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

            DashboardViewModel viewModel = new()
            {
                Projects = await _projectService.GetAllProjectsByCompanyAsync(companyId),
                UnresolvedDevelopmentTickets = await _ticketService.GetAllDeveloperTicketsByResolvedAsync(userId, false),
                DevelopmentTickets = await _ticketService.GetAllTicketsByRoleAsync("Developer", userId),
                SubmittedTickets = await _ticketService.GetAllTicketsByRoleAsync("Submitter", userId),
                Members = await _companyInfoService.GetAllMembersAsync(companyId),
                CurrentUser = await _userManager.GetUserAsync(User),
                UnassignedTickets = await _ticketService.GetAllUnassignedTicketsAsync(companyId),
            };
            return View(viewModel);
        }
        [HttpPost]
        public async Task<JsonResult> TypeChartMethod()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Random rnd = new();

            List<Project> projects = (await _projectService.GetAllProjectsByCompanyAsync(companyId)).OrderBy(p => p.Id).ToList();

            DonutViewModel chartData = new();
            chartData.labels = projects.Select(p => p.Name).ToArray();

            List<SubData> dsArray = new();
            List<int> tickets = new();
            List<string> colors = new();

            foreach (Project prj in projects)
            {
                tickets.Add(prj.Tickets.Count());

                // This code will randomly select a color for each element of the data 
                Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                string colorHex = string.Format("#{0:X6}", randomColor.ToArgb() & 0X00FFFFFF);

                colors.Add(colorHex);
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
