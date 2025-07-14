namespace Monitoring_Support_Server.Models.ViewModels
{
    public class LoginModel
    {
        public string PhoneNumber { get; set; }
        public string PassWord { get; set; }
    }

    public class ApiLoginModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string grant_type { get; set; } = "password";
    }
    public class ChangeUserPassword
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string RNewPassword { get; set; }
    }
}
