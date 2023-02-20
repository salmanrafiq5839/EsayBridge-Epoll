using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EPOLL.Website.DataAccess;
using EPOLL.Website.DataAccess.IdentityCustomModels;
using Microsoft.AspNetCore.Identity;
using EPOLL.Website.Infrastructure.Interfaces;
using System.Security.Claims;

namespace EPOLL.Website.Controllers
{
    public class UsersController : Controller
    {
        private readonly EPollContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISmtpService _smtpService;

        public UsersController(UserManager<ApplicationUser> userManager, EPollContext context, ISmtpService smtpService)
        {
            _context = context;
            _userManager = userManager;
            _smtpService = smtpService;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var query = _context.Users.AsQueryable();

            if(currentUserRole == "OrganizationAdmin")
            {
                var userName = User.FindFirstValue(ClaimTypes.Name);
                var user = await _userManager.FindByNameAsync(userName);
                query = query.Where(x => x.OrganizationId == user.OrganizationId);
            }
            
            return View(await query.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            if (currentUserRole == "OrganizationAdmin")
            {
                var userName = User.FindFirstValue(ClaimTypes.Name);
                var user = await _userManager.FindByNameAsync(userName);
                ViewBag.Organizations = await _context.Organizations.Where(x=>x.OrganizationId == user.OrganizationId).Select(x => new SelectListItem { Value = x.OrganizationId.ToString(), Text = x.Name }).ToListAsync();
            }
            else
            {
                ViewBag.Organizations = await _context.Organizations.Select(x => new SelectListItem { Value = x.OrganizationId.ToString(), Text = x.Name }).ToListAsync();
            }

            ViewBag.Error = TempData["errorMessage"];
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,OrganizationId,Email")] ApplicationUser applicationUser)
        {
            var tempPassword = "TestPass@1";
            if (ModelState.IsValid)
            {
                applicationUser.UserName = applicationUser.Email;
                applicationUser.NormalizedUserName = applicationUser.Email.ToUpper();
                applicationUser.NormalizedEmail = applicationUser.Email.ToUpper();

                var result = await _userManager.CreateAsync(applicationUser, tempPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(applicationUser, "User");
                    await _smtpService.SendEmail(applicationUser.Email, "Registered Successfully", $"You are Registered Successfully, Your password is {tempPassword}");
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["errorMessage"] = "Unable to add this user";
                }
            }
            return RedirectToAction(nameof(Create));
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.Users.FindAsync(id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            ViewBag.Organizations = await _context.Organizations.Select(x => new SelectListItem { Value = x.OrganizationId.ToString(), Text = x.Name }).ToListAsync();
            return View(applicationUser);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,OrganizationId,Email,LockoutEnabled")] ApplicationUser applicationUser)
        {
            if (id != applicationUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == applicationUser.Id);
                    currentUser.FullName = applicationUser.FullName;
                    currentUser.OrganizationId = applicationUser.OrganizationId;
                    currentUser.Email = applicationUser.Email;
                    currentUser.LockoutEnabled = applicationUser.LockoutEnabled;

                    _context.Update(currentUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicationUserExists(applicationUser.Id))
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
            return View(applicationUser);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var applicationUser = await _context.Users.FindAsync(id);
            _context.Users.Remove(applicationUser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApplicationUserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
