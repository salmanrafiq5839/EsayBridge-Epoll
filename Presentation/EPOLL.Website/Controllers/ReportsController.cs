using EPOLL.Website.ApiModels;
using EPOLL.Website.DataAccess;
using EPOLL.Website.DataAccess.IdentityCustomModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EPOLL.Website.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly EPollContext _context;
        private UserManager<ApplicationUser> _userManager;

        public ReportsController(EPollContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var pollReports = await PollReport();
            return View(pollReports);
        }

        private async Task<List<PollApiModel>> PollReport()
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var query = _context.PollResponses.AsQueryable();

            if (currentUserRole == "OrganizationAdmin")
            {
                var userName = User.FindFirstValue(ClaimTypes.Name);
                var user = await _userManager.FindByNameAsync(userName);
                var organizationPolls = await _context.Polls.Where(x => x.OrganizationId == user.OrganizationId).Select(x => x.PollId).ToListAsync();
                query = query.Where(x => organizationPolls.Contains(x.PollId));
            }
            var pollApiModels = new List<PollApiModel>();
            var pollResponses = await query.ToListAsync();
            var distinctPollIds = pollResponses.Select(x => x.PollId).Distinct().ToList();
            foreach (var pollId in distinctPollIds)
            {
                var poll = await GetPoll(pollId);
                var resultModel = new PollApiModel();
                resultModel.Name = poll.Name;
                resultModel.PollId = poll.PollId;
                var submittedAnswers = pollResponses.Where(x => x.PollId == pollId).ToList();
                resultModel.Answer1Count = submittedAnswers.Count(x => x.AnswerId == poll.Answer1Id);
                resultModel.Answer2Count = submittedAnswers.Count(x => x.AnswerId == poll.Answer2Id);
                resultModel.Answer3Count = submittedAnswers.Count(x => x.AnswerId == poll.Answer3Id);
                resultModel.Answer4Count = submittedAnswers.Count(x => x.AnswerId == poll.Answer4Id);

                pollApiModels.Add(resultModel);
            }
            return pollApiModels;
        }

        private async Task<PollApiModel> GetPoll(int id)
        {
                var poll = await _context.Polls.Include(x => x.Questions).ThenInclude(x => x.Answers).FirstOrDefaultAsync(x => x.PollId == id);
            if (poll == null) return null;

                PollApiModel model = new PollApiModel { PollId = poll.PollId, Name = poll.Name };
                var question = poll.Questions.FirstOrDefault();
                if (question != null)
                {
                    model.Question = question.Title;
                    model.QuestionId = question.QuestionId;
                    if (question.Answers.Count > 0)
                    {
                        var answer1 = question.Answers.ElementAt(0);
                        model.Answer1Id = answer1.AnswerId;
                        model.Answer1 = answer1.Title;

                        if (question.Answers.Count > 1)
                        {
                            var answer2 = question.Answers.ElementAt(1);
                            model.Answer2Id = answer2.AnswerId;
                            model.Answer2 = answer2.Title;

                            if (question.Answers.Count > 2)
                            {
                                var answer3 = question.Answers.ElementAt(2);
                                model.Answer3Id = answer3.AnswerId;
                                model.Answer3 = answer3.Title;

                                if (question.Answers.Count > 3)
                                {
                                    var answer4 = question.Answers.ElementAt(3);
                                    model.Answer4Id = answer4.AnswerId;
                                    model.Answer4 = answer4.Title;
                                }
                            }
                        }
                    }

                }
                return model;
            
        }
    }
}
