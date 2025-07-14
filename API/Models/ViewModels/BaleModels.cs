using System.Text.Json.Serialization;

namespace Monitoring_Support_Server.Models.ViewModels
{
    public class BaleApiResponse<T>
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("result")]
        public T Result { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    // این مدل برای نگهداری اطلاعات کاربر یا ربات است که توسط getMe بازگردانده می‌شود
    public class BaleUser
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("is_bot")]
        public bool IsBot { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }
    }
}
