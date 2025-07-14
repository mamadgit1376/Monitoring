using Monitoring_Support_Server.Models.ViewModels;

namespace Monitoring_Support_Server.Services.Interfaces
{
    public interface IBaleWebService
    {
        Task<ResultModel> SendMessageAsync(string message, string[] phoneNumber);
        Task<ResultModel> SendOtpAsync(string phoneNumber, int otp);

        Task<ResultModel> SendErrorItemsAsync();
    }
}
