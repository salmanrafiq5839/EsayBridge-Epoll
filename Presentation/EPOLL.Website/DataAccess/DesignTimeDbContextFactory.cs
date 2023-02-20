using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace EPOLL.Website.DataAccess
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EPollContext>
    {
        public EPollContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
            var connectionString = configuration.GetConnectionString("myconn");

            var builder = new DbContextOptionsBuilder<EPollContext>();
            var context = new EPollContext(
            builder.UseSqlite(connectionString).
            Options);
            return context;
        }
    }
    
}
