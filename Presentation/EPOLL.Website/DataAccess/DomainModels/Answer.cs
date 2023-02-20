using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EPOLL.Website.DataAccess.DomainModels
{
    public class Answer : BaseEntity
    {
        [Key]
        public int AnswerId { get; set; }
        public string Title { get; set; }

        public int QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public Question Question { get; set; }
    }
}
