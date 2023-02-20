using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPOLL.Website.ApiModels;
using EPOLL.Website.DataAccess;
using EPOLL.Website.DataAccess.IdentityCustomModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPOLL.Website.Controllers
{
    public class OrgAdminController : Controller
    {
        private readonly EPollContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public OrgAdminController(EPollContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var currentOrganization =  await _context.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == user.OrganizationId);
            var polls = await _context.Polls.CountAsync(x=> x.OrganizationId == currentOrganization.OrganizationId);
            var groups = await _context.Groups.Where(x=> x.OrganizationId == user.OrganizationId).CountAsync();
            var users = await _context.Users.CountAsync(x => x.OrganizationId == user.OrganizationId);
            var model = new OrganizationAdminStats { UsersCount = users, PollsCount = polls, GroupsCount = groups };
            return View(model);
        }
    }
}