namespace EPOLL.Website.ApiModels
{
    public class UserModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
    }
}
