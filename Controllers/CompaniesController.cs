using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JamesonBugTracker.Data;
using JamesonBugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using JamesonBugTracker.Extensions;
using JamesonBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace JamesonBugTracker.Controllers
{
        [Authorize]

    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTCompanyInfoService _companyService;
        private readonly IBTFileService _fileService;
        private readonly IConfiguration _configuration;


        public CompaniesController(ApplicationDbContext context, IBTCompanyInfoService companyService, IConfiguration configuration, IBTFileService fileService)
        {
            _context = context;
            _companyService = companyService;
            _configuration = configuration;
            _fileService = fileService;
        }

        // GET: Companies
        public IActionResult Index()
        {
            return RedirectToAction("Details", new { id = User.Identity.GetCompanyId() });

        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (User.Identity.GetCompanyId() != id)
            {
                return RedirectToAction("Details", new { id = User.Identity.GetCompanyId() });
            }
            if (id == null)
            {
                return NotFound();
            }

            var company = await _companyService.GetCompanyInfoByIdAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,ImageFileName,ImageFileData,ImageFileContentType")] Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (User.Identity.GetCompanyId() != id)
            {
                return RedirectToAction("Edit", new { id = User.Identity.GetCompanyId() });
            }
            if (User.IsInRole("DemoUser"))
            {
                return RedirectToAction("DemoError", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Company.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Company company, IFormFile imageFile)
        {
            if (User.IsInRole("DemoUser"))
            {
                return RedirectToAction("DemoError", "Home");
            }
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile is not null)
                    {
                    company.ImageFileContentType = imageFile is null ? _configuration["DefaultCompanyImage"].Split('.')[1] : imageFile.ContentType;
                        using var image = Image.Load(imageFile.OpenReadStream());
                        image.Mutate(x => x.Resize(256, 256));
                    company.ImageFileData = imageFile is null ? await _fileService.EncodeFileAsync(_configuration["DefaultCompanyImage"]) : await _fileService.ConvertFileToByteArrayAsync(image,company.ImageFileContentType);
                    }
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
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
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (User.IsInRole("DemoUser"))
            {
                return RedirectToAction("DemoError", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Company
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Company.FindAsync(id);
            _context.Company.Remove(company);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return _context.Company.Any(e => e.Id == id);
        }
    }
}
