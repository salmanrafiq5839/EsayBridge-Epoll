using EPOLL.Website.DataAccess.IdentityCustomModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPOLL.Website.DataAccess.DomainModels
{
    public class Organization : BaseEntity
    {
        public Organization()
        {
            Polls = new HashSet<Poll>();
            Groups = new HashSet<Group>();
        }

        [Key]
        public int OrganizationId { get; set; }
        [StringLength(200)]
        public string Name { get; set; }
        [StringLength(1000)]
        public string Detail { get; set; }

        #region navigation properties

        public ICollection<ApplicationUser> OrganizationUsers { get; set; }

        public ICollection<Poll> Polls { get; set; }
        public ICollection<Group> Groups { get; set; }
        #endregion
    }
}
