using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EPOLL.Website.ApiModels;
using EPOLL.Website.DataAccess.IdentityCustomModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EPOLL.Website.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(IConfiguration config, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _config = config;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            IActionResult response = Unauthorized();
            var user = await AuthenticateUser(model);

            if (user != null)
            {
                var tokenString = GenerateJSONWebToken(user);
                response = Ok(new { token = tokenString });
            }
            return response;
        }

        private string GenerateJSONWebToken(ApplicationUser model)
        {
            var claims = new[]
                {
                  new Claim(JwtRegisteredClaimNames.Sub, model.Email),
                  new Claim(JwtRegisteredClaimNames.Sid, $"{model.Id}"),
                };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddYears(1),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<ApplicationUser> AuthenticateUser(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Email);

            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (result.Succeeded)
                {
                    return user;
                }
                
            }return null;
        }
    }
}
