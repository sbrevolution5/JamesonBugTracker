﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using JamesonBugTracker.Models;
using JamesonBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using static JamesonBugTracker.Extensions.CustomAttributes;

namespace JamesonBugTracker.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly SignInManager<BTUser> _signInManager;
        private readonly IBTFileService _fileService;
        private readonly IConfiguration _configuration;

        public IndexModel(
            UserManager<BTUser> userManager,
            SignInManager<BTUser> signInManager, IBTFileService fileService, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fileService = fileService;
            _configuration = configuration;
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }
        public byte[] ImageData { get; set; }
        public string ImageContentType { get; set; }

        public class InputModel
        {
            [Display(Name ="First Name")]
            public string FirstName { get; set; }
            [Display(Name = "Last Name")]
            public string LastName { get; set; }
            public IFormFile ImageFile { get; set; }
            public byte[] ImageData { get; set; }
            public string ContentType { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(BTUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            //var imageFile = _fileService.ConvertByteArrayToFile(user.AvatarFileData, user.AvatarFileContentType);
            Username = userName;
            ImageData = user.AvatarFileData;
            ImageContentType = user.AvatarFileContentType;
            Input = new InputModel
            {
                LastName = user.LastName,
                FirstName = user.FirstName,
                PhoneNumber = phoneNumber,
                ImageData = user.AvatarFileData,
                ContentType = user.AvatarFileContentType
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.IsInRole("DemoUser"))
            {
                return RedirectToAction("DemoError", "Home");
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User.IsInRole("DemoUser"))
            {
                return RedirectToAction("DemoError", "Home");
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            if (Input.FirstName is not null)
            {
                user.FirstName = Input.FirstName;
                await _userManager.UpdateAsync(user);
            }
            if (Input.LastName is not null)
            {
                user.LastName = Input.LastName;
                await _userManager.UpdateAsync(user);
            }
            if (Input.ImageFile is not null)
            {
                using var image = Image.Load(Input.ImageFile.OpenReadStream());
                image.Mutate(x => x.Resize(256, 256));
                user.AvatarFileContentType = Input.ImageFile.ContentType;
                user.AvatarFileData = await _fileService.ConvertFileToByteArrayAsync(image, user.AvatarFileContentType);
                await _userManager.UpdateAsync(user);
            }
            else if (user.AvatarFileData is null)
            {
                user.AvatarFileData = await _fileService.EncodeFileAsync(_configuration["DefaultUserImage"]);
                user.AvatarFileContentType = _configuration["DefaultUserImage"].Split('.')[1];
                await _userManager.UpdateAsync(user);
            }
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
