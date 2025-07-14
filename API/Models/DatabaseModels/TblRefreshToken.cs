using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monitoring_Support_Server.Models.DatabaseModels
{
    public class TblRefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; } // Link refresh token to user
        public string Token { get; set; } // The refresh token string
        public DateTime Expires { get; set; }
        public DateTime? Revoked { get; set; } // When the token was revoked
        public string? ReplacedByToken { get; set; } // The new token that replaced this one

        public bool IsActive => Revoked == null && Expires >= DateTime.Now;

    }
    public class RefreshTokenRequest
    {
        /// <summary>
        /// The refresh token provided by the client.
        /// </summary>
        public string refreshToken { get; set; } = string.Empty;
    }
}
