using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
using JamesonBugTracker.Models.Enums;
using JamesonBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using static JamesonBugTracker.Extensions.CustomAttributes;

namespace JamesonBugTracker.Areas.Identity.Pages.Account
{

    [AllowAnonymous]
    public class RegisterByInviteModel : PageModel
    {
        private readonly SignInManager<BTUser> _signInManager;
        private readonly IBTProjectService _projectService;
        private readonly UserManager<BTUser> _userManager;
        private readonly ILogger<RegisterByInviteModel> _logger;
        private readonly IBTInviteService _inviteService;
        private readonly IEmailSender _emailSender;
        private readonly IBTFileService _fileService;
        private readonly IConfiguration _configuration;

        public RegisterByInviteModel(
            UserManager<BTUser> userManager,
            SignInManager<BTUser> signInManager,
            IBTProjectService projectService,
            ILogger<RegisterByInviteModel> logger,
            IBTInviteService inviteService,
            IEmailSender emailSender, IConfiguration configuration, IBTFileService fileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _projectService = projectService;
            _logger = logger;
            _inviteService = inviteService;
            _emailSender = emailSender;
            _configuration = configuration;
            _fileService = fileService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();


        public class InputModel
        {


            [Display(Name = "Profile Picture")]
            [MaxFileSize(2 * 1024 * 1024)]
            [AllowedExtensions(new string[] { ".jpg", ".png" })]
            public IFormFile ImageFile { get; set; }
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Display(Name = "Company")]
            public string Company { get; set; }

            [Required]
            [Display(Name = "CompanyId")]
            public int CompanyId { get; set; }

            [Required]
            [Display(Name = "ProjectId")]
            public int ProjectId { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(int id)
        {
            //Use "id" to find the invite  
            Invite invite = await _inviteService.GetInviteAsync(id);

            //Load Inputmodel with Invite information according to inviteId
            Input.Email = invite.RecipientEmail;
            Input.FirstName = invite.FirstName;
            Input.LastName = invite.LastName;
            Input.Company = invite.Company.Name;
            Input.CompanyId = invite.CompanyId;
            Input.ProjectId = invite.ProjectId;

        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                using var image = Image.Load(Input.ImageFile.OpenReadStream());
                image.Mutate(x => x.Resize(256, 256));
                var user = new BTUser
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    UserName = Input.Email,
                    Email = Input.Email,
                    CompanyId = Input.CompanyId,
                    AvatarFormFile = Input.ImageFile,
                    AvatarFileContentType = Input.ImageFile is null ? _configuration["DefaultUserImage"].Split('.')[1] : Input.ImageFile.ContentType,
                    AvatarFileData = Input.ImageFile is null ? await _fileService.EncodeFileAsync(_configuration["DefaultUserImage"]) : await _fileService.ConvertFileToByteArrayAsync(image,Input.ImageFile.ContentType),
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {

                    await _projectService.AddUserToProjectAsync(user.Id, Input.ProjectId);

                    _logger.LogInformation("User created a new account with password.");

                    // -- Add new registrant a role of "Submitter" -- //
                    await _userManager.AddToRoleAsync(user, Roles.Submitter.ToString());

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Create", "Tickets");
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ViewData["CompanyName"] = Input.Company;
            // If we got this far, something failed, redisplay form
            return Page();
        }
    }

}

