using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using static JamesonBugTracker.Extensions.CustomAttributes;

namespace JamesonBugTracker.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<BTUser> _signInManager;
        private readonly UserManager<BTUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IBTFileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly IBTRolesService _roleService;

        public RegisterModel(
            UserManager<BTUser> userManager,
            SignInManager<BTUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender, IBTFileService fileService, IConfiguration configuration, ApplicationDbContext context, IBTRolesService roleService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _fileService = fileService;
            _configuration = configuration;
            _context = context;
            _roleService = roleService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Display(Name = "Profile Picture")]
            [MaxFileSize(2 * 1024 * 1024)]
            [AllowedExtensions(new string[] { ".jpg", ".png" })]
            public IFormFile ImageFile { get; set; }
            [Display(Name = "Company Icon")]
            [MaxFileSize(2 * 1024 * 1024)]
            [AllowedExtensions(new string[] { ".jpg", ".png" })]
            public IFormFile CompanyImageFile { get; set; }
            [Required]
            [DisplayName("Company Name")]
            [StringLength(50)]
            public string CompanyName { get; set; }
            [Required]
            [DisplayName("Company Description")]
            [StringLength(500)]
            public string CompanyDescription { get; set; }
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
            [Required]
            [DisplayName("First Name")]
            [StringLength(50)]
            public string FirstName { get; set; }
            [Required]
            [DisplayName("Last Name")]
            [StringLength(50)]
            public string LastName { get; set; }

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

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                //reduce size of images
                using var image = Image.Load(Input.CompanyImageFile.OpenReadStream());
                image.Mutate(x => x.Resize(256,256));
                var newCompany = new Company()
                {
                    Name = Input.CompanyName,
                    Description = Input.CompanyDescription,

                    ImageFileContentType = Input.CompanyImageFile is null ? _configuration["DefaultCompanyImage"].Split('.')[1] : Input.CompanyImageFile.ContentType,
                    ImageFileData = Input.CompanyImageFile is null ? await _fileService.EncodeFileAsync(_configuration["DefaultCompanyImage"]) : await _fileService.ConvertFileToByteArrayAsync(image)
                };
                await _context.AddAsync(newCompany);
                await _context.SaveChangesAsync();
                using var userImage = Image.Load(Input.ImageFile.OpenReadStream());
                userImage.Mutate(x => x.Resize(256, 256));
                var user = new BTUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    AvatarFormFile = Input.ImageFile,
                    AvatarFileContentType = Input.ImageFile is null ? _configuration["DefaultUserImage"].Split('.')[1] : Input.ImageFile.ContentType,
                    AvatarFileData = Input.ImageFile is null ? await _fileService.EncodeFileAsync(_configuration["DefaultUserImage"]) : await _fileService.ConvertFileToByteArrayAsync(userImage),
                    CompanyId = newCompany.Id
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    await _roleService.AddUserToRoleAsync(user, "Admin");
                    _logger.LogInformation("User created a new account with password.");
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
                        //TODO ViewData with message to confirm email
                        return RedirectToPage("Login");
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
