using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monitoring_Support_Server.Models.DatabaseModels.Users
{
    [Table("TblRoles", Schema = "usr")]
    public partial class TblRole : IdentityRole<int>
    {
        public TblRole() { }

    }
}
