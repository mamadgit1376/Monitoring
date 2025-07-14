namespace Monitoring_Support_Server.Models.ViewModels
{
    public class JwtToken
    {
        public JwtToken()
        {

        }
        public string? ApplicationToken { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
    }
}
