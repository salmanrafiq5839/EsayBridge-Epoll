using EPOLL.Website.DataAccess.IdentityCustomModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EPOLL.Website.DataAccess.DomainModels
{
    public class PollResponse
    {
        [Key]
        public int Id { get; set; }
        public int PollId { get; set; }
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
        public int UserId { get; set; }
    }
}
