using EPOLL.Website.DataAccess.IdentityCustomModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPOLL.Website.DataAccess.DomainModels
{ 
    public class Group  : BaseEntity
    {
        public Group()
        {
            GroupUsers = new HashSet<GroupUser>();
            Polls = new HashSet<Poll>();
        }

        [Key]
        public int GroupId { get; set; }
        [StringLength(200)]
        public string Name { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }

        public int OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public ICollection<GroupUser> GroupUsers { get; set; }
        public ICollection<Poll> Polls { get; set; }
    }
}
