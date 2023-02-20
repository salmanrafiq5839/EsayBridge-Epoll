using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPOLL.Website.ApiModels
{
    public class OrganizationAdminStats
    {
        public int UsersCount { get; set; }
        public int PollsCount { get; set; }
        public int GroupsCount { get; set; }
    }
}
