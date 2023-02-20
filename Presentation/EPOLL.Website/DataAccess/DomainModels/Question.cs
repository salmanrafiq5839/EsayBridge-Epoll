using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EPOLL.Website.DataAccess.DomainModels
{
    public class Question : BaseEntity
    {
        public Question()
        {
            Answers = new HashSet<Answer>();
        }
        [Key]
        public int QuestionId { get; set; }
        public string Title { get; set; }

        public int PollId { get; set; }
        [ForeignKey("PollId")]
        public Poll Poll { get; set; }

        public ICollection<Answer> Answers { get; set; }
        
    }
}
