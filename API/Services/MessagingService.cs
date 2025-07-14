using Monitoring_Support_Server.Models.ViewModels;
using Monitoring_Support_Server.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
namespace Monitoring_Support_Server.Services
{
    public class MessagingService : IMessagingService
    {
        private readonly MessagingToken _messagingToken;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baleApiBaseUrl;

        public MessagingService(IOptions<MessagingToken> messagingTokenOptions, IHttpClientFactory httpClientFactory)
        {
            _messagingToken = messagingTokenOptions.Value;
            _httpClientFactory = httpClientFactory;

            if (string.IsNullOrEmpty(_messagingToken.BaleToken))
            {
                throw new InvalidOperationException("The 'BaleToken' was not found in configuration.");
            }

            _baleApiBaseUrl = $"https://tapi.bale.ai/bot{_messagingToken.BaleToken}";
        }




        #region BaleArm

        /// <summary>
        /// اطلاعات پایه ربات را دریافت کرده و توکن را اعتبارسنجی می‌کند.
        /// </summary>
        public async Task<BaleUser> GetBotInfoAsync()
        {
            // متد getMe نیازی به پارامتر ورودی ندارد
            string url = $"{_baleApiBaseUrl}/getMe";
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    // پاسخ را به مدل‌هایی که ساختیم تبدیل می‌کنیم
                    var apiResponse = JsonSerializer.Deserialize<BaleApiResponse<BaleUser>>(jsonResponse);

                    if (apiResponse != null && apiResponse.Ok)
                    {
                        // اگر همه چیز موفق بود، اطلاعات ربات را برمی‌گردانیم
                        return apiResponse.Result;
                    }
                    else
                    {
                        Console.WriteLine($"API returned an error: {apiResponse?.Description}");
                        return null;
                    }
                }
                else
                {
                    // اگر وضعیت پاسخ موفقیت‌آمیز نبود (مثلا 401 Unauthorized)
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Token validation failed. Status: {response.StatusCode}, Content: {errorContent}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"An HTTP error occurred during token validation: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> SendMessageAsync(string message, string recipient)
        {
            string url = $"{_baleApiBaseUrl}/sendMessage";
            var httpClient = _httpClientFactory.CreateClient();

            var payload = new
            {
                chat_id = recipient,
                text = message
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(url, content);

                // اگر درخواست موفقیت آمیز نبود
                if (!response.IsSuccessStatusCode)
                {
                    // محتوای خطا را بخوان و چاپ کن
                    string errorContent = await response.Content.ReadAsStringAsync();
                    // به جای کنسول از یک لاگر واقعی استفاده کنید
                    Console.WriteLine($"Error sending message. Status: {response.StatusCode}, Content: {errorContent}");
                    return false;
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"An HTTP error occurred: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// بررسی می‌کند که آیا کاربری در بله وجود دارد یا خیر.
        /// </summary>
        public async Task<bool> CheckUserExistsAsync(string userId)
        {
            // از متد getChat برای بررسی وجود کاربر استفاده می‌کنیم
            string url = $"{_baleApiBaseUrl}/getChat";
            var httpClient = _httpClientFactory.CreateClient();

            // بدنه درخواست فقط شامل شناسه کاربر است
            var payload = new { chat_id = userId };
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(url, content);
                // اگر کاربر وجود داشته باشد، درخواست موفقیت‌آمیز خواهد بود
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        #endregion BaleArm

    }
}
