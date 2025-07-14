using AbfaCS.Common.Bale;
using Monitoring.Support.Server.Models.DatabaseModels;
using Monitoring.Support.Services;
using Monitoring_Support_Server.Models.ViewModels;
using Monitoring_Support_Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Monitoring_Support_Server.Services
{
    public class BaleWebService : IBaleWebService
    {
        private readonly IBaleService _baleService;
        private readonly MessagingToken _messagingToken;
        private readonly MonitoringDbContext _db;

        public BaleWebService(IBaleService baleService, IOptions<MessagingToken> messagingTokenOptions, MonitoringDbContext db)
        {
            _baleService = baleService;
            _messagingToken = messagingTokenOptions.Value;
            _db = db;
        }


        public async Task<ResultModel> SendMessageAsync(string message, string[] phoneNumber)
        {
            try
            {
                var Token = await _baleService.AuthenticateAsync(_messagingToken.UserName, _messagingToken.Password);

                SendMessageRequest sendMessageRequest = new SendMessageRequest
                {
                    Text = message,
                    Phones = phoneNumber
                };
                var result = await _baleService.SendBulkTextMessageAsync(sendMessageRequest, Token.AccessToken);
                return new ResultModel(true, "");
            }
            catch (Exception ex)
            {
                return new ResultModel(false, $"Error sending message: {ex.Message}");
            }
        }

        public async Task<ResultModel> SendOtpAsync(string phoneNumber, int otp)
        {
            try
            {
                var Token = await _baleService.AuthenticateAsync(_messagingToken.UserName, _messagingToken.Password);
                SendOtpRequest sendOtpRequest = new SendOtpRequest
                {
                    Phone = phoneNumber,
                    Otp = otp
                };
                var res = await _baleService.SendOtpAsync(sendOtpRequest, Token.AccessToken);
                return new ResultModel(true, "");
            }
            catch (Exception ex)
            {
                return new ResultModel(false, $"Error sending OTP: {ex.Message}");
            }
        }

        public async Task<ResultModel> SendErrorItemsAsync()
        {
            try
            {
                var now = DateTime.Now.AddDays(-1);
                var Token = await _baleService.AuthenticateAsync(_messagingToken.UserName, _messagingToken.Password);

                var ItemsForNotif = await _db.TblItemLog.Include(z => z.TblStatusCodeNavigation).Include(z => z.TblCompanyNavigation).Include(z=>z.TblItemNavigation).Where(x => !x.IsNotified && x.RemoveDate == null && x.CreateDate > now).ToListAsync();

                var Users = await _db.Users.Where(z => z.RemoveDate == null).ToListAsync();

                foreach (var item in ItemsForNotif)
                {
                    var message = $"item.TblItemNavigation.ImportanceLevel: {item.TblItemNavigation.ImportanceLevel.ToString()} -item.TblItemNavigation.ItemName: {item.TblItemNavigation.ItemName} - TblStatusCodeNavigation.Name: {item.TblStatusCodeNavigation.Name} - TblStatusCodeNavigation.Description: {item.TblStatusCodeNavigation.Description} - item.TblStatusCodeNavigation.PersianName: {item.TblStatusCodeNavigation.PersianName} - item.TblCompanyNavigation.CompanyName : {item.TblCompanyNavigation.CompanyName}";

                    var phoneNumbers = Users.Where(z => z.CompanyId == null || z.CompanyId == item.CompanyId).Select(z => z.PhoneNumber).ToList();
                    var ValidPhoneNumbers = phoneNumbers
                        .Where(p => !string.IsNullOrEmpty(p) && p.Length == 11 && p.StartsWith("0"))
                        .Select(p => "98" + p.Substring(1)) // تبدیل 0915... به 98915...
                        .ToList();

                    SendMessageRequest sendMessageRequest = new SendMessageRequest
                    {
                        Text = message,
                        Phones = ValidPhoneNumbers
                    };

                    await _baleService.SendBulkTextMessageAsync(sendMessageRequest, Token.AccessToken);
                    item.IsNotified = true;


                }
                _db.TblItemLog.UpdateRange(ItemsForNotif);
                await _db.SaveChangesAsync();

                return new ResultModel(true, "");
            }
            catch (Exception ex)
            {
                return new ResultModel(false, $"Error sending error items: {ex.Message}");
            }
        }

    }
}
