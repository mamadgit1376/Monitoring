using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monitoring_Support_Server.Models.DatabaseModels.Users
{
    [Table("TblUserLogins", Schema = "usr")]

    public class TblUserLogin : IdentityUserLogin<int>
    {
        public TblUserLogin()
        {
        }
        public string SmsCode { get; set; }


    }
}