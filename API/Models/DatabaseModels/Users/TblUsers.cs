using Monitoring.Support.Server.Models.DatabaseModels;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monitoring_Support_Server.Models.DatabaseModels.Users
{
    [Table("TblUsers", Schema = "usr")]
    public partial class TblUser : IdentityUser<int> 
    {
        public override string? Email { get; set; }

        /// <summary>
        /// شماره شماره تلفن 
        /// </summary>
        public override string? PhoneNumber { get; set; }
        /// <summary>
        /// آخرین آی پی کاربر
        /// </summary>
        public string? LastKnownIp { get; set; }

        /// <summary>
        /// نام کامل کاربر
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// کد پیالمک
        /// </summary>
        public long? SmsCode { get; set; }

        public int? CompanyId { get; set; } = null;

        [ForeignKey("CompanyId")]
        public TblCompany? TblCompany { get; set; }
        public DateTime? RemoveDate { get; set; } = null;
    }
}
