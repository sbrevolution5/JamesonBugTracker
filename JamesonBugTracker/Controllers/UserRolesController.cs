﻿using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
using JamesonBugTracker.Models.Enums;
using JamesonBugTracker.Models.ViewModels;
using JamesonBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Controllers
{
    //[Authorize(Roles="Admin")]
    public class UserRolesController : Controller
    {
        private readonly IBTRolesService _rolesService;
        private readonly UserManager<BTUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserRolesController(ApplicationDbContext context, UserManager<BTUser> userManager, IBTRolesService bTRolesService)
        {
            _userManager = userManager;
            _rolesService = bTRolesService;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> model = new();
            //TODO Company users
            List<BTUser> users = _context.Users.ToList();
            foreach (var user in users)
            {
                ManageUserRolesViewModel vm = new();
                vm.BTUser = user;
                var selected = await _rolesService.ListUserRolesAsync(user);
                vm.Roles = new MultiSelectList(_context.Roles, "Name", "Name", selected);
                model.Add(vm);
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            BTUser user = _context.Users.Find(member.BTUser.Id);
            IEnumerable<string> roles = await _rolesService.ListUserRolesAsync(user);
            await _rolesService.RemoveUserFromRolesAsync(user, roles); // TODO refactor
            string userRole = member.SelectedRoles.FirstOrDefault();

            if (Enum.TryParse(userRole, out Roles roleValue))
            {
                await _rolesService.AddUserToRoleAsync(user, userRole);
                return RedirectToAction("ManageUserRoles");

            }
            return RedirectToAction("ManageUserRoles");
        }
    }
}