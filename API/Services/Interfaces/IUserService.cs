using Monitoring_Support_Server.Models.DatabaseModels;

namespace Monitoring_Support_Server.Services.Interfaces
{
    public interface IUserService
    {
        Task SaveRefreshToken(TblRefreshToken refreshToken);

        Task<TblRefreshToken?> GetRefreshToken(string token);

        Task UpdateRefreshToken(TblRefreshToken refreshToken);
    }
}
