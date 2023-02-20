using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPOLL.Website.DataAccess.DomainModels
{ 
    public class Poll : BaseEntity
    {
        public Poll()
        {
            Questions = new HashSet<Question>();
        }
        
        [Key]
        public int PollId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int? OrganizationId { get; set; }
        [ForeignKey("OrganizationId")]
        public Organization Organization { get; set; }

        public int? GroupId { get; set; }
        [ForeignKey("GroupId")]
        public Group Group { get; set; }

        public ICollection<Question> Questions { get; set; }
    }
}
