using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monitoring_Support_Server.Models.DatabaseModels.Users
{
    [Table("TblRole", Schema = "usr")]
    public class TblRoleClaim : IdentityRoleClaim<int>
    {
        public TblRoleClaim()
        {
        }




    }
}   