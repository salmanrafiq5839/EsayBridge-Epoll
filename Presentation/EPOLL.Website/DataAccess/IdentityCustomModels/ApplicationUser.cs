using EPOLL.Website.DataAccess.DomainModels;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace EPOLL.Website.DataAccess.IdentityCustomModels
{
    public class ApplicationUser : IdentityUser<int>
    {
        public ApplicationUser()
        {
            GroupUsers = new HashSet<GroupUser>();
        }

        public string FullName { get; set; }

        public bool IsOrganizationAdmin { get; set; }

        public int? OrganizationId { get; set; }
        public Organization Organization { get; set; }

        
        public ICollection<GroupUser> GroupUsers { get; set; }
    }
}
