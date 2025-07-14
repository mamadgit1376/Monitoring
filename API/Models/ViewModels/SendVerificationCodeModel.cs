using System.ComponentModel.DataAnnotations;

namespace Monitoring_Support_Server.Models.ViewModels
{
    public class SendVerificationCodeModel
    {
        [Required(ErrorMessage = "شماره تلفن الزامی است")]
        [Phone(ErrorMessage = "شماره تلفن معتبر نیست")]
        public string PhoneNumber { get; set; }
    }
}
