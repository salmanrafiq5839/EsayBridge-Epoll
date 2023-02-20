using EPOLL.Website.ApiModels;
using EPOLL.Website.DataAccess;
using EPOLL.Website.DataAccess.DomainModels;
using EPOLL.Website.DataAccess.IdentityCustomModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EPOLL.Website.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly EPollContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public DashboardController(EPollContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var organizations = await _context.Organizations.CountAsync();
            var polls = await _context.Polls.CountAsync();
            var groups = await _context.Groups.CountAsync();
            var users = await _context.Users.CountAsync();
            var model = new AdminStats { OrganizationCount = organizations, PollCount = polls, GroupCount = groups, UserCount = users };
            return View(model);
        }

        // GET: Organizations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Organizations/Create
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrganizationId,Name,Detail,IsEnabled")] Organization organization)
        {
            if (ModelState.IsValid)
            {
                organization.CreatedOn = DateTime.UtcNow;
                organization.UpdatedOn = DateTime.UtcNow;
                _context.Add(organization);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(organization);
        }

        // GET: Organizations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }
            return View(organization);
        }

        // POST: Organizations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrganizationId,Name,Detail,CreatedOn,UpdatedOn,IsEnabled")] Organization organization)
        {
            if (id != organization.OrganizationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(organization);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizationExists(organization.OrganizationId))
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

        // GET: Organizations/Edit/5
        public async Task<IActionResult> OrgEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }
            return View(organization);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrgEdit(int id, [Bind("OrganizationId,OrganizationAdminId,Name,Detail,CreatedOn,UpdatedOn,IsEnabled")] Organization organization)
        {
            if (id != organization.OrganizationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(organization);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizationExists(organization.OrganizationId))
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
            return View(organization);
        }

        public async Task<ActionResult> User()
        {
            var userId = int.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var groupsCount = await _context.GroupUsers.CountAsync(x => x.UserId == userId);
            var pollCount = await _context.Polls.CountAsync(x => x.OrganizationId == currentUser.OrganizationId);
            var userDashboard = new UserDashboardModel { GroupCount = groupsCount, PollCount = pollCount };
            return View(userDashboard);
        }

        public async Task<ActionResult> OrganizationAdmin()
        {
            var userId = int.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var usersCount = await _context.Users.CountAsync(x => x.OrganizationId == currentUser.OrganizationId);
            var groupsCount = await _context.Groups.CountAsync(x => x.OrganizationId == currentUser.OrganizationId);
            var pollCount = await _context.Polls.CountAsync(x => x.OrganizationId == currentUser.OrganizationId);
            var userDashboard = new UserDashboardModel { UsersCount = usersCount, GroupCount = groupsCount, PollCount = pollCount };
            return View(userDashboard);
        }
        private bool OrganizationExists(int id)
        {
            return _context.Organizations.Any(e => e.OrganizationId == id);
        }
    }
}
