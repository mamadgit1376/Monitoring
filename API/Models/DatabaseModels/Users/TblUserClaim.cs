using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monitoring_Support_Server.Models.DatabaseModels.Users
{
    [Table("TblUserClaims", Schema = "usr")]
    public class TblUserClaim : IdentityUserClaim<int>
    {
        public TblUserClaim()
        {
        }
        
    }
}   