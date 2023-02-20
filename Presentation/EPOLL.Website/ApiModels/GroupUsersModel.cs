using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPOLL.Website.ApiModels
{
    public class GroupUsersModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public List<GroupUser> Users { get; set; } = new List<GroupUser>();
    }

    public class GroupUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool HasAdded { get; set; }
    }
}
