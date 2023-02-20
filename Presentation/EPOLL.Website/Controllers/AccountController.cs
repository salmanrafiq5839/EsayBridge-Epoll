using EPOLL.Website.ApiModels;
using EPOLL.Website.DataAccess;
using EPOLL.Website.DataAccess.IdentityCustomModels;
using EPOLL.Website.Infrastructure.Helpers;
using EPOLL.Website.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EPOLL.Website.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EPollContext _context;
        private readonly ISmtpService _smtpService;

        public AccountController(EPollContext context,UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ISmtpService smtpService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _smtpService = smtpService;
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, true, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var currentUser = await _userManager.FindByNameAsync(model.Email);
                    var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
                    if (currentUserRoles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else if (currentUserRoles.Contains("OrganizationAdmin"))
                    {
                        return RedirectToAction("Index", "OrgAdmin");
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {

            if (ModelState.IsValid)
            {
                var orgAlreadyExists = await _context.Organizations.AnyAsync(x => x.Name.ToLower() == model.OrganizationName.Trim().ToLower());
                if (orgAlreadyExists)
                {
                    ViewBag.Error = "Organization already exists";
                    return View();
                }

                
                var user = new ApplicationUser { FullName = model.Text, UserName = model.Email, Email = model.Email, IsOrganizationAdmin = true };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // assing OrganizationAdmin role
                    await _userManager.AddToRoleAsync(user, "OrganizationAdmin");
                    var organization = new DataAccess.DomainModels.Organization
                    {
                        Name = model.OrganizationName
                    };
                    _context.Organizations.Add(organization);
                    _ = _context.SaveChangesAsync();
                    user.OrganizationId = organization.OrganizationId;
                    user.IsOrganizationAdmin = true;
                    _ = _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    //_logger.LogInformation(3, "User created a new account with password.");
                    return RedirectToAction(nameof(HomeController.Index), "Polls");
                }
                //AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public IActionResult Forgot()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Forgot(ForgotModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var newPassword = Strings.RandomString(6);
                var result = await _userManager.ResetPasswordAsync(user, passwordResetToken, newPassword);
                if (result.Succeeded)
                {
                    await _smtpService.SendEmail(user.Email, "Your new password is ready!", $"Your new password is :{newPassword}");
                    return RedirectToAction(nameof(Login));
                }
            }
            return View();
        }

    }
}
