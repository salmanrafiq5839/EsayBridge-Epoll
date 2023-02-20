using EPOLL.Website.DataAccess.IdentityCustomModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPOLL.Website.DataAccess.DomainModels
{
    public class GroupUser
    {
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
    }
}
