using System;

namespace EPOLL.Website.DataAccess.DomainModels
{
    public class BaseEntity
    {
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        public bool IsEnabled { get; set; } = true;
    }
}
