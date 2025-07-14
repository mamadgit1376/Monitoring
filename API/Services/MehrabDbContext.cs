using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Monitoring.Support.Server.Models.DatabaseModels;
using Monitoring_Support_Server.Models.DatabaseModels.Users;
using Microsoft.EntityFrameworkCore.Design;
using Monitoring_Support_Server.Models.DatabaseModels;
namespace Monitoring.Support.Services
{
    public class MonitoringDbContext : IdentityDbContext<TblUser, TblRole, int>
    {
        public MonitoringDbContext(DbContextOptions<MonitoringDbContext> options)
        : base(options)
        {
        }
        public class MonitoringDbContextFactory : IDesignTimeDbContextFactory<MonitoringDbContext>
        {
            public MonitoringDbContext CreateDbContext(string[] args)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var optionsBuilder = new DbContextOptionsBuilder<MonitoringDbContext>();
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

                return new MonitoringDbContext(optionsBuilder.Options);
            }
        }
        public DbSet<TblCategory> TblCategory { get; set; }
        public DbSet<TblCompany> TblCompany { get; set; }
        public DbSet<TblItem> TblItem { get; set; }
        public DbSet<TblItemLog> TblItemLog { get; set; }
        public DbSet<TblStatus> TblStatus { get; set; }
        public DbSet<TblRefreshToken> TblRefreshToken { get; set; }
    }
}