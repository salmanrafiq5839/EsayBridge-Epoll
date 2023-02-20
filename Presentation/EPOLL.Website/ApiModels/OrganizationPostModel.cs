using System;
using System.ComponentModel.DataAnnotations;

namespace EPOLL.Website.ApiModels
{
    public class OrganizationPostModel
    {
        public int OrganizationId { get; set; }
        [StringLength(200)]
        public string Name { get; set; }
        [StringLength(1000)]
        public string Detail { get; set; }
        public bool IsEnabled { get; set; }
        public string AdminEmail { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
