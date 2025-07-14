using System.ComponentModel.DataAnnotations;

namespace Monitoring.Support.Models.ViewModels
{
    public class CreateUserRegisterModel
    {
        [Required]
        [MinLength(11)]
        [MaxLength(11)]
        public string Username { get; set; }
        [Required]
        public string FullName { get; set; }
        public _UserRole UserRole { get; set; } = _UserRole.User;
        public int? CompanyId { get; set; }
        public int? TicketId { get; set; }
    }
    public enum _UserRole : short
    {
        Admin,
        User,
    }
}
