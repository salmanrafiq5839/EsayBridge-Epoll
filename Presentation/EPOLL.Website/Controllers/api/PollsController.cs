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
    public class PollsController : ControllerBase
    {
        private readonly EPollContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PollsController(EPollContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Polls
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PollApiModel>>> GetPolls()
        {
            var a = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = await _context.Users.Where(x=>x.UserName == a).FirstOrDefaultAsync();
            var userGroupsIds = await _context.GroupUsers.Where(x => x.UserId == currentUser.Id).Select(x => x.GroupId).ToListAsync();

            var polls =  await _context.Polls.Where(x=> x.OrganizationId == currentUser.OrganizationId).ToListAsync();
            var actualPolls = polls.Where(x => x.GroupId == null || userGroupsIds.Contains(x.GroupId.Value)).ToList();
            var qrcodeUrl = $"{ this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}/qrcodes/qrcode"+"{0}.png";
            return actualPolls.Select(x => new PollApiModel { PollId = x.PollId, Name = x.Name, Description = x.Description,
                QrCodeLink = string.Format(qrcodeUrl, x.PollId)
            }).OrderByDescending(x=>x.PollId).ToList();
        }

        // GET: api/Polls/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PollApiModel>> GetPoll(int id)
        {
            var poll = await _context.Polls.Include(x=>x.Questions).ThenInclude(x=>x.Answers).FirstOrDefaultAsync(x=>x.PollId == id);

            if (poll == null)
            {
                return NotFound();
            }

            PollApiModel model = new PollApiModel { PollId = poll.PollId, Name = poll.Name };
            var question = poll.Questions.FirstOrDefault();
            if(question != null)
            {
                model.Question = question.Title;
                model.QuestionId = question.QuestionId;
                if(question.Answers.Count > 0)
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

        // PUT: api/Polls/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPoll(int id, Poll poll)
        {
            if (id != poll.PollId)
            {
                return BadRequest();
            }

            _context.Entry(poll).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PollExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Polls
        [HttpPost]
        public async Task<ActionResult<Poll>> PostPoll(Poll poll)
        {
            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPoll", new { id = poll.PollId }, poll);
        }

        // DELETE: api/Polls/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Poll>> DeletePoll(int id)
        {
            var poll = await _context.Polls.FindAsync(id);
            if (poll == null)
            {
                return NotFound();
            }

            _context.Polls.Remove(poll);
            await _context.SaveChangesAsync();

            return poll;
        }

        [HttpGet("{id}/{questionId}/{answerId}")]
        public async Task<ActionResult<BaseModel>> SubmitAnswer(int id, int questionId, int answerId)
        {
            var poll = await _context.Polls.FindAsync(id);
            if (poll == null)
            {
                return NotFound();
            }

            var claims = HttpContext.User.Claims.ToList();
            var userIdClaim = claims.Find(x => x.Type == "sid").Value;
            var userId = int.Parse(userIdClaim);
            var alreadySubmitted = await _context.PollResponses.Where(x => x.UserId == userId && x.PollId == id).AnyAsync();
            if (!alreadySubmitted)
            {
                await _context.PollResponses.AddAsync(new PollResponse { AnswerId = answerId, PollId = id, QuestionId = questionId, UserId = userId });
                await _context.SaveChangesAsync();
            }
            else
            {
                return new BaseModel { Message = "Already Submitted" };
            }
            return new BaseModel { Success = true };
        }

        [HttpGet("GetReport/{id}")]
        public async Task<ActionResult<PollApiModel>> PollReport(int id)
        {
            var pollResponses = await _context.PollResponses.Where(x => x.PollId == id).ToListAsync();
            if (pollResponses.Count == 0)
            {
                return NotFound();
            }

            var pollActionResult = await GetPoll(id);
            var poll = pollActionResult.Value;

            var claims = HttpContext.User.Claims.ToList();
            var userIdClaim = claims.Find(x => x.Type == "sid").Value;
            var userId = int.Parse(userIdClaim);
            var submittedAnswers = await _context.PollResponses.Where(x => x.PollId == id).ToListAsync();


            poll.Answer1Count = submittedAnswers.Count(x => x.AnswerId == poll.Answer1Id); 
            poll.Answer2Count = submittedAnswers.Count(x => x.AnswerId == poll.Answer2Id);
            poll.Answer3Count = submittedAnswers.Count(x => x.AnswerId == poll.Answer3Id);
            poll.Answer4Count = submittedAnswers.Count(x => x.AnswerId == poll.Answer4Id);

            return poll;
        }

        private bool PollExists(int id)
        {
            return _context.Polls.Any(e => e.PollId == id);
        }
    }
}
