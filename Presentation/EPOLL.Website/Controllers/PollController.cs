using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPOLL.Website.ApiModels;
using EPOLL.Website.DataAccess;
using EPOLL.Website.DataAccess.IdentityCustomModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPOLL.Website.Controllers
{
    public class PollController : Controller
    {

        private readonly EPollContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PollController(EPollContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ActionResult> Perform(int id)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var alreadyPolled = await _context.PollResponses.AnyAsync(x => x.UserId == currentUser.Id && x.PollId == id);
            if(alreadyPolled)
                return Redirect("/");

            var poll = await _context.Polls.Include(x => x.Questions).Include("Questions.Answers").FirstOrDefaultAsync(x => x.PollId == id);
            if (poll == null)
            {
                return NotFound();
            }


            var pollApiModel = new PollApiModel
            {
                PollId = poll.PollId,
                Name = poll.Name,
                Description = poll.Description,
                OrganizationId = poll.OrganizationId.Value,
                GroupId = poll.GroupId,
                Question = poll.Questions.FirstOrDefault()?.Title,
                QuestionId = poll.Questions.FirstOrDefault()?.QuestionId,

            };

            int answerIndex = 1;

            if (poll.Questions.FirstOrDefault() != null)
                foreach (var answer in poll.Questions.FirstOrDefault()?.Answers.OrderBy(x => x.AnswerId).ToList())
                {
                    if (answerIndex == 1)
                    {
                        pollApiModel.Answer1 = answer.Title;
                        pollApiModel.Answer1Id = answer.AnswerId;
                    }
                    else if (answerIndex == 2)
                    {
                        pollApiModel.Answer2 = answer.Title;
                        pollApiModel.Answer2Id = answer.AnswerId;
                    }
                    else if (answerIndex == 3)
                    {
                        pollApiModel.Answer3 = answer.Title;
                        pollApiModel.Answer3Id = answer.AnswerId;
                    }
                    else if (answerIndex == 4)
                    {
                        pollApiModel.Answer4 = answer.Title;
                        pollApiModel.Answer4Id = answer.AnswerId;
                    }
                    answerIndex += 1;
                }
            return View(pollApiModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Perform([Bind("PollId,QuestionId,AnswerId")] PollPerform model)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var alreadyPolled = await _context.PollResponses.AnyAsync(x => x.UserId == currentUser.Id && x.PollId == model.PollId);
            if (!alreadyPolled)
            {
                _context.PollResponses.Add(new DataAccess.DomainModels.PollResponse
                {
                    UserId = currentUser.Id,
                    PollId = model.PollId,
                    AnswerId = model.AnswerId,
                    QuestionId = model.QuestionId
                });
                _ = await _context.SaveChangesAsync();
            }
            return Redirect("/");
        }
        }
}