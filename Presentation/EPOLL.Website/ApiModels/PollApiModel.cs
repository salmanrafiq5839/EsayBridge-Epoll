using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPOLL.Website.ApiModels
{
    public class PollApiModel
    {
        public int PollId { get; set; }
        public int OrganizationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Question { get; set; }
        public string Answer1 { get; set; }
        public string Answer2 { get; set; }
        public string Answer3 { get; set; }
        public string Answer4 { get; set; }

        public int Answer1Id { get; set; }
        public int Answer2Id { get; set; }
        public int Answer3Id { get; set; }
        public int Answer4Id { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        public int? GroupId { get; set; }
        public int? QuestionId { get; internal set; }

        public int Answer1Count { get; set; }
        public int Answer2Count { get; set; }
        public int Answer3Count { get; set; }
        public int Answer4Count { get; set; }

        public string QrCodeLink { get; set; }
    }
}
