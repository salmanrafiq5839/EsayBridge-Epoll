using EPOLL.Website.DataAccess;
using EPOLL.Website.DataAccess.IdentityCustomModels;
using EPOLL.Website.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EPOLL.Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly EPollContext _dbContext;
        private readonly ISmtpService _smtpService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(UserManager<ApplicationUser> userManager, ISmtpService smtpService,EPollContext context)
        {
            _dbContext = context;
            _smtpService = smtpService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";


            return View();
        }

        public IActionResult Services()
        {
            ViewData["Message"] = "Your services page.";

            return View();
        }

        public async Task<IActionResult> Profile()
        {
            var currentUserName = HttpContext.User.Identity.Name;
            var currentUser = await _dbContext.Users.Include(x => x.Organization).FirstOrDefaultAsync(x => x.UserName == currentUserName);
            
            return View(currentUser);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
