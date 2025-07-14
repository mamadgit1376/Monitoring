using Monitoring.Support.Services;
using Monitoring_Support_Server.Models.DatabaseModels;
using Monitoring_Support_Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Monitoring_Support_Server.Services
{
    public class UserService : IUserService
    {
        private readonly MonitoringDbContext _db;

        public UserService(MonitoringDbContext db)
        {
            _db = db;
        }

        public async Task SaveRefreshToken(TblRefreshToken refreshToken)
        {
            _db.TblRefreshToken.Add(refreshToken);
            await _db.SaveChangesAsync();
        }

        public async Task<TblRefreshToken?> GetRefreshToken(string token)
        {
            var old = await _db.TblRefreshToken.Where(rt => rt.Token == token).FirstOrDefaultAsync();
            return old;
        }

        public async Task UpdateRefreshToken(TblRefreshToken refreshToken)
        {
             _db.TblRefreshToken.Update(refreshToken);
             await _db.SaveChangesAsync();
        }


    }
}
