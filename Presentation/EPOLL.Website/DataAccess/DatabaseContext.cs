using EPOLL.Website.DataAccess.DomainModels;
using EPOLL.Website.DataAccess.IdentityCustomModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EPOLL.Website.DataAccess
{
    public class EPollContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {

        public EPollContext(DbContextOptions<EPollContext> options) : base(options)
        {
        }

        public DbSet<Poll> Polls { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupUser> GroupUsers { get; set; }
        public DbSet<PollResponse> PollResponses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole() { Id = 3, ConcurrencyStamp = "a64b35cb-5f26-4ba9-a147-1b3b55fc7c6b", Name = "User", NormalizedName = "USER" });

            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole() { Id = 2, ConcurrencyStamp = "88619bf2-0895-4a17-a362-caa5e6c619f0", Name = "OrganizationAdmin", NormalizedName = "ORGANIZATIONADMIN" });

            builder.Entity<ApplicationRole>().HasData(
            new ApplicationRole() { Id = 1, ConcurrencyStamp = "88619bf2-0895-4a17-a362-caa5e6c619f1", Name = "Admin", NormalizedName = "ADMIN" });

            builder.Entity<ApplicationUser>().HasData(
            new ApplicationUser() { Id = 1, FullName = "Admin", Email = "admin@easybridge.com", UserName = "admin@easybridge.com", NormalizedEmail = "ADMIN@EASYBRIDGE.COM", NormalizedUserName = "ADMIN@EASYBRIDGE.COM", PasswordHash = "AMSlikdei3LkPD6Zlp1pLUP96A5QyNtGn0wg7QT407GAVJrHTUdUqiYC4pYu0Ta4CQ==", SecurityStamp = "83cfdf30-7d77-4d63-a339-1c1ce0d7ee45", ConcurrencyStamp = "88619bf2-0895-4a17-a362-caa5e6c619f1" });

            builder.Entity<IdentityUserRole<int>>().HasData(new IdentityUserRole<int>
            {
                RoleId = 1,
                UserId = 1
            });

            builder.Entity<GroupUser>()
                .HasKey(bc => new { bc.UserId, bc.GroupId });
            builder.Entity<GroupUser>()
                .HasOne(bc => bc.User)
                .WithMany(b => b.GroupUsers)
                .HasForeignKey(bc => bc.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<GroupUser>()
                .HasOne(bc => bc.Group)
                .WithMany(c => c.GroupUsers)
                .HasForeignKey(bc => bc.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

         

        }
    }
}
