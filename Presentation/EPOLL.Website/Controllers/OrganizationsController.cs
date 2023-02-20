using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EPOLL.Website.DataAccess;
using EPOLL.Website.DataAccess.DomainModels;
using Microsoft.AspNetCore.Authorization;
using System;
using EPOLL.Website.ApiModels;
using EPOLL.Website.DataAccess.IdentityCustomModels;
using Microsoft.AspNetCore.Identity;
using EPOLL.Website.Infrastructure.Interfaces;

namespace EPOLL.Website.Controllers
{
    [Authorize]
    public class OrganizationsController : Controller
    {
        private readonly EPollContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISmtpService _smtpService;

        public OrganizationsController(
        ISmtpService smtpService ,UserManager<ApplicationUser> userManager,EPollContext context)
        {
            _context = context;
            _userManager = userManager;
            _smtpService = smtpService;
        }

        // GET: Organizations
        public async Task<IActionResult> Index()
        {
            var organizations = await _context.Organizations.Include(x=>x.OrganizationUsers).Select(x => new OrganizationPostModel { OrganizationId = x.OrganizationId, Name = x.Name, Detail = x.Detail, AdminEmail = x.OrganizationUsers.Where(y=>y.IsOrganizationAdmin).Select(y=>y.Email).FirstOrDefault() }).ToListAsync();
            
            return View(organizations);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrganizationPostModel model)
        {
            var newOrg = new Organization { Name = model.Name, Detail = model.Detail, IsEnabled = model.IsEnabled };
            var selectUser = await _userManager.FindByNameAsync(model.AdminEmail);
            if (selectUser != null)
            {
                var alreadyAdmin = await _context.Users.AnyAsync(x => x.Id == selectUser.Id && x.IsOrganizationAdmin);

                if (!alreadyAdmin)
                {
                    await _userManager.AddToRoleAsync(selectUser, "OrganizationAdmin");
                    _ = await _context.Organizations.AddAsync(newOrg);
                    selectUser.IsOrganizationAdmin = true;
                    _context.Update(selectUser);
                    _ = await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                ViewBag.Error = "Already an organization admin";
            }
            else
            {
                _ = await _context.AddAsync(newOrg);

                var tempPassword = "TestPass@1";
                var applicationUser = new ApplicationUser();
                applicationUser.Email = model.AdminEmail;
                applicationUser.UserName = model.AdminEmail;
                applicationUser.NormalizedUserName = applicationUser.Email.ToUpper();
                applicationUser.NormalizedEmail = applicationUser.Email.ToUpper();
                applicationUser.IsOrganizationAdmin = true;
                applicationUser.Organization = newOrg;

                var result = await _userManager.CreateAsync(applicationUser, tempPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(applicationUser, "OrganizationAdmin");
                    await _smtpService.SendEmail(applicationUser.Email, "Registered Successfully", $"You are Registered Successfully, Your password is {tempPassword}");
                    return RedirectToAction("Index");
                }


            }
                return View();
        }

        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations.FirstOrDefaultAsync(x=> x.OrganizationId == id);
            if (organization == null)
            {
                return NotFound();
            }

            var organizationApiModel = new OrganizationPostModel
            {
                OrganizationId = organization.OrganizationId,
                Name = organization.Name,
                Detail = organization.Detail,
                IsEnabled = organization.IsEnabled
            };
            return View(organizationApiModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(OrganizationPostModel model)
        {
            var oldOrganization = await _context.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == model.OrganizationId);

            var selectUser = await _userManager.FindByNameAsync(model.AdminEmail);

            if (selectUser != null)
            {
                
                var alreadyAdmin = await _context.Users.AnyAsync(x => x.Id == selectUser.Id && x.OrganizationId == oldOrganization.OrganizationId && x.IsOrganizationAdmin);
                if (!alreadyAdmin)
                {
                    /*if (!alreadyAdmin)
                    {
                        await _userManager.AddToRoleAsync(selectUser, "OrganizationAdmin");
                    }
                    oldOrganization.Name = model.Name;
                    if (!alreadyAdmin) {
                        oldOrganization.OrganizationAdminId = selectUser.Id;
                    }*/
                    oldOrganization.Detail = model.Detail;
                    oldOrganization.IsEnabled = model.IsEnabled;
                    _context.Organizations.Update(oldOrganization);
                    _ = await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                ViewBag.Error = "Already an organization admin";
            }
            ViewBag.Error = "User doesnot exists";
            return View();
        }
        // GET: Organizations/Details/5
        public async Task<IActionResult> OrgDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations
                .FirstOrDefaultAsync(m => m.OrganizationId == id);
            if (organization == null)
            {
                return NotFound();
            }

            return View(organization);
        }

        // GET: Organizations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations
                .FirstOrDefaultAsync(m => m.OrganizationId == id);
            if (organization == null)
            {
                return NotFound();
            }

            return View(organization);
        }

        // POST: Organizations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == id);
            if (organization == null)
            {
                return NotFound();
            }

            var organizationApiModel = new OrganizationPostModel
            {
                OrganizationId = organization.OrganizationId,
                Name = organization.Name,
                Detail = organization.Detail,
                IsEnabled = organization.IsEnabled,
            };
            return View(organizationApiModel);
        }



        private bool OrganizationExists(int id)
        {
            return _context.Organizations.Any(e => e.OrganizationId == id);
        }
    }
}
