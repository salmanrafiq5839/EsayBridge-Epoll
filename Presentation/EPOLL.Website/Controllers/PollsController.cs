using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EPOLL.Website.DataAccess;
using EPOLL.Website.DataAccess.DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EPOLL.Website.DataAccess.IdentityCustomModels;
using System;
using EPOLL.Website.ApiModels;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using EPOLL.Website.Infrastructure.Interfaces;
using System.IO;

namespace EPOLL.Website.Controllers
{
    [Authorize]
    public class PollsController : Controller
    {
        private readonly EPollContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISmtpService _smtpService;
        private readonly IQrCodeService _qrCodeService;
        private readonly IErrorReporter _logger;

        public PollsController(IErrorReporter logger,IQrCodeService qrCodeService,EPollContext context, UserManager<ApplicationUser> userManager, ISmtpService smtpService)
        {
            _context = context;
            _userManager = userManager;
            _smtpService = smtpService;
            _logger = logger;
            _qrCodeService = qrCodeService;
        }

        // GET: Polls
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return View(await _context.Polls.Where(x=> x.OrganizationId == user.OrganizationId).ToListAsync());
        }

        // GET: Polls/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var poll = await _context.Polls.Include(x=>x.Questions).Include("Questions.Answers")
                .FirstOrDefaultAsync(m => m.PollId == id);
            if (poll == null)
            {
                return NotFound();
            }


            return View(poll);
        }

        // GET: Polls/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var organizationGroups = await _context.Groups.Where(x => x.OrganizationId == currentUser.OrganizationId).ToListAsync();
            var selectList = new List<SelectListItem>();
            selectList.Add(new SelectListItem { Value = "0", Text = "Please select on group" });

            foreach (var group in organizationGroups) {
                selectList.Add(new SelectListItem { Value = group.GroupId.ToString(), Text = group.Name });
            }
            ViewBag.Groups = selectList;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PollId,Name,GroupId,Description, Question, Answer1, Answer2, Answer3, Answer4")] PollApiModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var poll = new Poll();
                poll.OrganizationId = user.OrganizationId;
                if(model.GroupId > 0)
                {
                    poll.GroupId = model.GroupId;
                }
                poll.Name = model.Name;
                poll.Description = model.Description;
                poll.Questions.Add(new Question
                {
                    Title = model.Question,
                    Answers = new List<Answer>() {
                        new Answer { Title = model.Answer1 },
                        new Answer { Title = model.Answer2 },
                        new Answer { Title = model.Answer3 },
                        new Answer { Title = model.Answer4 }
                    }
                });

                _context.Add(poll);
                await _context.SaveChangesAsync();


                var emailList = await GenerateList(model.GroupId == 0 ? null : model.GroupId);
                foreach (var email in emailList)
                {
                     await _smtpService.SendEmail(email.Email, "Poll request", $"https://epoll.azurewebsites.net/poll/perform/{poll.PollId}");
                }

                var bitmap = _qrCodeService.GenerateQrCode($"{poll.PollId}");
                var path = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\qrcodes", $"qrcode{poll.PollId}.png");
                await _logger.CaptureAsync(path);
                bitmap.Save(path);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Polls/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var poll = await _context.Polls.Include(x=>x.Questions).Include("Questions.Answers").FirstOrDefaultAsync( x=>x.PollId == id);
            if (poll == null)
            {
                return NotFound();
            }


            var pollApiModel = new PollApiModel {
                PollId = poll.PollId,
                Name = poll.Name,
                Description = poll.Description,
                OrganizationId = poll.OrganizationId.Value,
                GroupId = poll.GroupId,
                Question = poll.Questions.FirstOrDefault()?.Title,

            };

            int answerIndex = 1;

            if(poll.Questions.FirstOrDefault() != null)
            foreach(var answer in poll.Questions.FirstOrDefault()?.Answers.OrderBy(x => x.AnswerId).ToList())
            {
                if (answerIndex == 1)
                {
                    pollApiModel.Answer1 = answer.Title;
                }
                else if (answerIndex == 2)
                {
                    pollApiModel.Answer2 = answer.Title;
                }
                else if (answerIndex == 3)
                {
                    pollApiModel.Answer3 = answer.Title;
                }
                else if (answerIndex == 4)
                {
                    pollApiModel.Answer4 = answer.Title;
                }
                answerIndex += 1;
            }

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var organizationGroups = await _context.Groups.Where(x => x.OrganizationId == currentUser.OrganizationId).ToListAsync();
            var selectList = new List<SelectListItem>();
            selectList.Add(new SelectListItem { Value = "0", Text = "Please select on group" });

            foreach (var group in organizationGroups)
            {
                selectList.Add(new SelectListItem { Value = group.GroupId.ToString(), Text = group.Name });
            }
            ViewBag.Groups = selectList;
            return View(pollApiModel);
        }

        // POST: Polls/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PollId,GroupId, OrganizationId,Name,Description, Question, Answer1, Answer2, Answer3, Answer4")] PollApiModel model)
        {

            if (id != model.PollId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var poll = await _context.Polls.Include(x => x.Questions).Include("Questions.Answers").FirstOrDefaultAsync(x => x.PollId == id);
                    if (poll == null)
                    {
                        return NotFound();
                    }

                    foreach (var question in poll.Questions)
                    {
                        foreach(var answer in question.Answers)
                        {
                            _context.Remove(answer);
                        }                    
                        _context.Remove(question);
                    }

                    poll.Name = model.Name;
                    poll.Description = model.Description;
                    if (model.GroupId > 0)
                    {
                        poll.GroupId = model.GroupId;
                    }
                    else
                    {
                        poll.GroupId = null;
                    }
                    poll.Questions.Add(new Question
                    {
                        Title = model.Question,
                        Answers = new List<Answer>() {
                        new Answer { Title = model.Answer1 },
                        new Answer { Title = model.Answer2 },
                        new Answer { Title = model.Answer3 },
                        new Answer { Title = model.Answer4 }
                    }
                    });
                    _context.Update(poll);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PollExists(model.PollId))
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
            return View(model);
        }

        // GET: Polls/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var poll = await _context.Polls
                .FirstOrDefaultAsync(m => m.PollId == id);
            if (poll == null)
            {
                return NotFound();
            }

            return View(poll);
        }

        // POST: Polls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var poll = await _context.Polls.FindAsync(id);
            _context.Polls.Remove(poll);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PollExists(int id)
        {
            return _context.Polls.Any(e => e.PollId == id);
        }

        private async Task<List<EmailReceiver>> GenerateList(int? groupId = null)
        {
            if (groupId == null)
            {
                return await _context.Users.Select(x => new EmailReceiver { Email = x.Email, Name = x.FullName }).ToListAsync();

            }
            else
            {
                return await _context.Groups.Include(x => x.GroupUsers)
                    .Include("GroupUsers.User").Where(x => x.GroupId == groupId).
                    Select(x => x.GroupUsers.Select(y => new EmailReceiver { Name = y.User.FullName, Email = y.User.Email }).ToList()).FirstOrDefaultAsync();
            }
        }
    }
}
