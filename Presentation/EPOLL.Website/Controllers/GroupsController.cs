using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EPOLL.Website.DataAccess;
using EPOLL.Website.DataAccess.DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EPOLL.Website.DataAccess.IdentityCustomModels;
using EPOLL.Website.ApiModels;
using GroupUser = EPOLL.Website.ApiModels.GroupUser;
using System.Security.Claims;

namespace EPOLL.Website.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {
        private readonly EPollContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GroupsController(EPollContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Groups
        public async Task<IActionResult> Index()
        {
            var currentUser =  _userManager.Users.Where( x=> x.UserName == HttpContext.User.Identity.Name).FirstOrDefault();
            return View(await _context.Groups.Where(x=> x.OrganizationId == currentUser.OrganizationId).ToListAsync());
        }

        // GET: Groups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups
                .FirstOrDefaultAsync(m => m.GroupId == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }

        // GET: Groups/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GroupId,Name,Description,CreatedOn,UpdatedOn,IsEnabled")] Group @group)
        {
            if (ModelState.IsValid)
            {
                var currentUser = _userManager.Users.Where(x => x.UserName == HttpContext.User.Identity.Name).FirstOrDefault();

                group.OrganizationId = currentUser.OrganizationId.Value;
                _context.Add(group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@group);
        }

        // GET: Groups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups.FindAsync(id);
            if (@group == null)
            {
                return NotFound();
            }
            return View(@group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GroupId,OrganizationId,Name,Description,CreatedOn,UpdatedOn,IsEnabled")] Group @group)
        {
            if (id != @group.GroupId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(@group.GroupId))
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
            return View(@group);
        }

        // GET: Groups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups
                .FirstOrDefaultAsync(m => m.GroupId == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @group = await _context.Groups.Include(x=>x.GroupUsers).FirstOrDefaultAsync(x=> x.GroupId == id);
            _context.GroupUsers.RemoveRange(group.GroupUsers.ToArray());
            _context.Groups.Remove(@group);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ManageUsers(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups.Include(x => x.GroupUsers).FirstOrDefaultAsync(x => x.GroupId == id);
            if (@group == null)
            {
                return NotFound();
            }

            var query = _context.Users.AsQueryable();
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            if (currentUserRole == "OrganizationAdmin")
            {
                var userName = User.FindFirstValue(ClaimTypes.Name);
                var user = await _userManager.FindByNameAsync(userName);
                query = query.Where(x => x.OrganizationId == user.OrganizationId);
            }
            var allUsers = await query.Select(x => new GroupUser { Email = x.Email, UserId = x.Id, UserName = x.UserName }).ToListAsync();

            foreach (var item in allUsers.Where(w => group.GroupUsers.Select(x=> x.UserId).Contains(w.UserId)))
            {
                item.HasAdded = true;
            }

            var viewModel = new GroupUsersModel() { GroupId = id.Value, GroupName = group.Name,  Users = allUsers };
            return View(viewModel);
        }

        public async Task<IActionResult> AddToGroup(int groupId, int userId)
        {
            _context.GroupUsers.Add(new DataAccess.DomainModels.GroupUser { GroupId = groupId, UserId = userId });
            _ = await _context.SaveChangesAsync();
            return RedirectToAction("ManageUsers", new { id = groupId });
        }

        public async Task<IActionResult> RemoveFromGroup(int groupId, int userId)
        {
            var userGroup = _context.GroupUsers.Find(userId,groupId );
            if (userGroup != null) {
                _context.GroupUsers.Remove(userGroup);
            _ = await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageUsers", new { id = groupId });
        }

        private bool GroupExists(int id)
        {
            return _context.Groups.Any(e => e.GroupId == id);
        }
    }
}
