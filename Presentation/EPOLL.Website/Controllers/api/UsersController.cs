using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EPOLL.Website.DataAccess;
using EPOLL.Website.DataAccess.DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using EPOLL.Website.ApiModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using EPOLL.Website.DataAccess.IdentityCustomModels;

namespace EPOLL.Website.Controllers.api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly EPollContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(EPollContext context, UserManager<ApplicationUser> userManager)
        {
            _dbContext = context;
            _userManager = userManager;
        }

        [HttpGet("Profile")]
        public async Task<ActionResult<UserModel>> GetUserProfile()
        {
            var currentUserName = HttpContext.User.Claims.FirstOrDefault();
            var currentUser = await _dbContext.Users.Include(x => x.Organization).Select(x => 
            new UserModel {
                Email = x.Email, 
                FullName = x.FullName,
                UserName = x.UserName, Id = x.Id,
                OrganizationId = x.OrganizationId ?? 0,
                OrganizationName = x.Organization.Name }).FirstOrDefaultAsync(x => x.UserName == currentUserName.Value);
            return currentUser;
        }

        [HttpPut]
        [Route("Update")]
        public async Task<ActionResult<UserModel>> UpdateUser(UserModel model)
        {
            var currentUserName = HttpContext.User.Claims.FirstOrDefault();
            var currentUser = await _userManager.FindByNameAsync(currentUserName.Value);
            currentUser.FullName = model.FullName;
            _ = await _userManager.UpdateAsync(currentUser);
            return new UserModel();
        }
    }
}
