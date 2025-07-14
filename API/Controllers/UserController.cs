using Monitoring_Support_Server.MiddleWare;
using Monitoring_Support_Server.Models.DatabaseModels;
using Monitoring_Support_Server.Models.DatabaseModels.Users;
using Monitoring_Support_Server.Models.ViewModels;
using Monitoring_Support_Server.Services;
using Monitoring_Support_Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Monitoring.Support.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<TblUser> _userManager;
        private readonly SignInManager<TblUser> _signInManager;
        private readonly RoleManager<TblRole> _roleManager;
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly IUserService _userService;
        private readonly IBaleWebService _baleWebService;

        public UserController(UserManager<TblUser> userManager, SignInManager<TblUser> signInManager, RoleManager<TblRole> roleManager, ILogger<UserController> logger, IConfiguration configuration, IMemoryCache memoryCache, IUserService userService, IBaleWebService baleWebService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _configuration = configuration;
            _memoryCache = memoryCache;
            _userService = userService;
            _baleWebService = baleWebService;
        }



        //[Route("User/TestBale")]
        //[HttpPost]
        ////[Authorize(Roles = "HeadAdmin")]
        ////[SanitizeInput]
        //public async Task<IActionResult> TestBale(Model model)
        //{
        //    try
        //    {
        //        var result = await _baleWebService.SendMessageAsync(model.Message, model.Phones);
        //        return StatusCode(result.Execute ? 200 : 400, new ApiResponse { Message = result.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error Sending Bale Message");
        //        return StatusCode(500, new ApiResponse { Data = null, Message = "خطا در ارسال پیام." });
        //    }
        //}

        //public class Model
        //{
        //    public string Message { get; set; }
        //    public string[] Phones { get; set; }
        //}


        /// <summary>
        /// لاگین کاربران
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("User/Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            try
            {
                // بررسی تعداد لاگین ها
                string cacheKey = request.PhoneNumber.ToLower();
                if (!_memoryCache.TryGetValue(cacheKey, out int attempts))
                {
                    attempts = 0;
                }
                if (attempts > 100)
                    return BadRequest();
                // یافتن یوزر و اعتبار سنجی رمز 
                var user = await _userManager.FindByNameAsync(request.PhoneNumber);
                if (user == null || !await _userManager.CheckPasswordAsync(user, request.PassWord))
                {
                    attempts++;
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(10));
                    _memoryCache.Set(cacheKey, attempts, cacheEntryOptions);

                    _logger.LogWarning($"ورود ناموفق برای شماره: {request.PhoneNumber}");
                    return StatusCode(401, new ApiResponse { Message = "شماره یا رمز عبور اشتباه است." });
                }

                var currentIp = HttpContext.Connection.RemoteIpAddress?.ToString();


                if (!string.IsNullOrEmpty(user.LastKnownIp) && user.LastKnownIp != currentIp)
                {
                    return StatusCode(401, new ApiResponse { Message = "ورود از آی‌پی غیرمجاز" });
                }
                if (string.IsNullOrEmpty(user.LastKnownIp))
                {
                    user.LastKnownIp = currentIp;
                    await _userManager.UpdateAsync(user);
                }

                // تشخصی رول کاربر
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User";
                // ساخت jwt توکن
                var token = GenerateJwtToken(user, role);
                string refreshToken = GenerateRefreshToken();
                var refreshTokenEntity = new TblRefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    Expires = DateTime.Now.AddMinutes(30) //زمان انقضا
                };
                await _userService.SaveRefreshToken(refreshTokenEntity);

                return Ok(new ApiResponse { Data = new { token = token, refreshToken = refreshToken, role = role, phoneNumber = user.UserName, fullName = user.FullName }, Message = "ورود با موفقیت انجام شد." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطایی در ورود کاربر {request?.PhoneNumber} رخ داده است.");
                return StatusCode(500, new ApiResponse { Message = "خطا." });
            }
        }
        /// <summary>
        /// رفرش توکن
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("User/RefreshToken")]
        [HttpPost]
        [SanitizeInput]
        [ValidateSiteRequest]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.refreshToken))
                    return Unauthorized(new ApiResponse { Message = "Invalid or expired refresh token." });

                // اعتبار سنجی رفرش توکن
                var refreshToken = await _userService.GetRefreshToken(model.refreshToken);
                if (refreshToken == null || !refreshToken.IsActive || refreshToken.Expires <= DateTime.Now) // Added check for expiration
                    return Unauthorized(new ApiResponse { Message = "Invalid or expired refresh token." });
    
                // یافتن یوزر 
                var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
                if (user == null || user.RemoveDate != null) // Ensure the user associated with the refresh token is valid
                    return Unauthorized(new ApiResponse { Message = "User associated with refresh token is invalid." });

                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User";
                // ساخت jwt توکن
                var newAccessToken = GenerateJwtToken(user, role);

                string newRefreshToken = GenerateRefreshToken();

                // جایگزینی
                refreshToken.Revoked = DateTime.Now;
                refreshToken.ReplacedByToken = newRefreshToken;
                await _userService.UpdateRefreshToken(refreshToken);

                var newRefreshTokenEntity = new TblRefreshToken
                {
                    UserId = user.Id,
                    Token = newRefreshToken,
                    Expires = DateTime.Now.AddMinutes(30) //زمان انقضا
                };
                await _userService.SaveRefreshToken(newRefreshTokenEntity);

                return Ok(new ApiResponse
                {
                    Message = "توکن با موفقیت بروز رسانی شد.",
                    Data = new { token = newAccessToken, refreshToken = newRefreshToken }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh token.");
                return StatusCode(500, new ApiResponse { Message = $"خطایی در سرور رخ داد . بعدا تلاش کنید: {ex.Message}", Data = null });
            }
        }


        /// <summary>
        /// تغییر پسورد
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("User/ChangeUserPassword")]
        [SanitizeInput]
        [ValidateSiteRequest]
        public async Task<IActionResult> ChangeUserPassword([FromBody] ChangeUserPassword request)
        {
            string? userId = null;
            try
            {
                if (request.NewPassword != request.RNewPassword)
                {
                    return StatusCode(400, new ApiResponse() { Data = null, Message = "رمز های جدید وارد شده برابر نیست." });
                }
                userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                // یافتن یوزر کاربر
                var TblUser = await _userManager.FindByIdAsync(userId);
                if (TblUser == null)
                    return StatusCode(403,new ApiResponse { Data = null, Message = "اکانت شما معتبر نیست." });
                // تغییر رمز عبور
                var ChangePassword = await _userManager.ChangePasswordAsync(TblUser, request.OldPassword.Trim(), request.NewPassword.Trim());
                if(!ChangePassword.Succeeded && ChangePassword.Errors.Any(x => x.Code == "PasswordMismatch"))
                {
                    return StatusCode(400, new ApiResponse { Data = null , Message = "رمز عبور فعلی اشتباه است." });
                }
                return StatusCode(ChangePassword.Succeeded ? 200 : 400, new ApiResponse { Data = null, Message = ChangePassword.Succeeded ? "تغییر پسورد با موفقیت انجام شد." : "تغییر پسورد ناموفق بود." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطایی در تغییر رمز عبور کاربر {userId} رخ داده است.");
                return StatusCode(500, new ApiResponse { Message = "خطا." });
            }
        }

        private string GenerateJwtToken(TblUser user, string Role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                     new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                     new Claim(ClaimTypes.Name, user.UserName),
                     new Claim(ClaimTypes.Role, Role)
                }),
                Expires = DateTime.Now.AddMinutes(5),
                Issuer = _configuration["JwtSettings:Issuer"] ?? "defaultIssuer",
                Audience = _configuration["JwtSettings:Audience"] ?? "defaultAudience",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

    }
}
