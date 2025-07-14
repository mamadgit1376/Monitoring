using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monitoring_Support_Server.Models.DatabaseModels.Users
{
    [Table("TblUserTokens", Schema = "usr")]

    public class TblUserToken : IdentityUserToken<int>
    {
        public TblUserToken()
        {
        }
        public string SmsCode { get; set; }


    }
}