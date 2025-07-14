using Monitoring.Support.Models.ViewModels;
using Monitoring_Support_Server.MiddleWare;
using Monitoring_Support_Server.Models.DatabaseModels.Users;
using Monitoring_Support_Server.Models.ViewModels;
using Monitoring_Support_Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace Monitoring_Support_Server.Controllers
{
    [ApiController]
    [ValidateSiteRequest]
    public class ManagementController : ControllerBase
    {
        private readonly UserManager<TblUser> _userManager;
        private readonly RoleManager<TblRole> _roleManager;
        private readonly ILogger<ManagementController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private const string upperChars = "ABCDEFGHJKMNPQRSTUVWXYZ";
        private const string lowerChars = "abcdefghkmnpqrstuvwxyz";
        private const string numberChars = "23456789";
        private const string specialChars = "!@#$%^&*";
        private const string easyChars = upperChars + lowerChars + numberChars + specialChars;
        private static readonly Random random = new Random();
        private readonly ICompanyService _companyService;
        private readonly ICategoryService _categoryService;
        private readonly IItemService _itemService;
        private readonly IUserService _userService;
        public ManagementController(UserManager<TblUser> userManager, RoleManager<TblRole> roleManager, ILogger<ManagementController> logger,
                     IConfiguration configuration, IMemoryCache memoryCache, ICompanyService companyService, ICategoryService categoryService, IItemService itemService, IUserService userService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _configuration = configuration;
            _memoryCache = memoryCache;
            _companyService = companyService;
            _categoryService = categoryService;
            _itemService = itemService;
            _userService = userService;
        }

        #region DastNazanParsa
        //[AllowAnonymous]
        //[HttpGet]
        //[Route("Management/CreateHead")]
        //public async Task<IActionResult> CreateHead()
        //{
        //    try
        //    {

        //        var user = new TblUser
        //        {
        //            UserName = "09010000000",
        //            FullName = "مهراب ادمین اصلی",
        //            PhoneNumber = "09010000000",
        //            PhoneNumberConfirmed = true,
        //            Email = "A@gmail.com",
        //            EmailConfirmed = true,
        //            TwoFactorEnabled = false,
        //            LastKnownIp = HttpContext.Connection.RemoteIpAddress?.ToString(),

        //        };
        //        var Password = "09010000000@Admin";
        //        var result = await _userManager.CreateAsync(user, Password);
        //        if (!result.Succeeded)
        //        {
        //            return StatusCode(400, new ApiResponse { Message = "ثبت‌ نام ناموفق بود." });
        //        }
        //        var RoleName = "HeadAdmin";
        //        var CreateRole = await _roleManager.RoleExistsAsync(RoleName);
        //        if (!CreateRole)
        //        {
        //            TblRole TblRole = new TblRole()
        //            {
        //                Name = RoleName,
        //            };
        //            var CreateroleResult = await _roleManager.CreateAsync(TblRole);
        //            if (!CreateroleResult.Succeeded)
        //            {
        //                return BadRequest(new ApiResponse { Message = "ثبت‌ نام موفق بود اما ایجاد نقش ناموفق بود." });
        //            }
        //        }
        //        var roleResult = await _userManager.AddToRoleAsync(user, RoleName);
        //        if (!roleResult.Succeeded)
        //        {
        //            return BadRequest(new ApiResponse { Message = "ثبت‌ نام موفق بود اما افزودن نقش ناموفق بود." });
        //        }
        //        return Ok(new ApiResponse { Message = "ثبت ‌نام کاربر با موفقیت انجام شد." });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"خطایی در ورود کاربر رخ داده است.");
        //        return StatusCode(500, new ApiResponse { Message = "خطا." });
        //    }
        //}
        #endregion

        /// <summary>
        /// افزودن کاربر جدید
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = "HeadAdmin,Admin")]
        [HttpPost]
        [Route("Management/AddNewUser")]
        [SanitizeInput]
        public async Task<IActionResult> AddNewUser([FromBody] CreateUserRegisterModel model)
        {
            try
            {
                // تایید نقش کاربر
                var existingUser = await _userManager.FindByNameAsync(model.Username);
                if (existingUser != null)
                {
                    return StatusCode(400, new ApiResponse { Data = null, Message = "کاربر با این شماره تلفن قبلا ثبت ‌نام کرده است." });
                }
                var UserInRole = await _userManager.GetUsersInRoleAsync("Admin");
                if (model.UserRole == _UserRole.User)
                {// اگر کاربر از نوع User باشد باید کد شرکت را هم بفرستد
                    if (model.CompanyId == null)
                    {// اگر کد شرکت ارسال نشود
                        return StatusCode(400, new ApiResponse { Data = null, Message = "یوزر استانی بدون کد استان معتبر نیست ." });
                    }
                    // اگر کد شرکت ارسال شود باید معتبر باشد
                    var ValidCompany = await _companyService.IsValidCompany((int)model.CompanyId);
                    if (!ValidCompany)
                    {
                        return StatusCode(400, new ApiResponse { Data = null, Message = "شرکت معتبر نیست." });
                    }
                }
                // اگر کاربر از نوع Admin باشد باید کد شرکت را هم بفرستد
                var user = new TblUser
                {
                    UserName = model.Username,
                    FullName = model.FullName,
                    PhoneNumber = null,
                    PhoneNumberConfirmed = false,
                    Email = null,
                    EmailConfirmed = false,
                    TwoFactorEnabled = true,
                    LastKnownIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    CompanyId = model.CompanyId,
                };
                // اگر کاربر از نوع Admin باشد باید کد شرکت را هم بفرستد
                var Password = GenerateEasyPassword(8);
                var result = await _userManager.CreateAsync(user, Password);
                if (!result.Succeeded)
                {
                    return StatusCode(400, new ApiResponse { Message = "ثبت‌ نام ناموفق بود." });
                }
                var RoleName = model.UserRole == _UserRole.Admin ? "Admin" : "User";
                var CreateRole = await _roleManager.RoleExistsAsync(RoleName);
                if (!CreateRole)
                {// اگر نقش وجود نداشته باشد آن را ایجاد میکند
                    TblRole TblRole = new TblRole()
                    {
                        Name = RoleName,
                    };
                    var CreateroleResult = await _roleManager.CreateAsync(TblRole);
                    if (!CreateroleResult.Succeeded)
                    {
                        return StatusCode(400, new ApiResponse { Message = "ثبت‌ نام موفق بود اما ایجاد نقش ناموفق بود." });
                    }
                }
                // اگر نقش وجود داشته باشد آن را به کاربر اضافه میکند
                var roleResult = await _userManager.AddToRoleAsync(user, RoleName);
                if (!roleResult.Succeeded)
                    return StatusCode(400, new ApiResponse { Message = "ثبت‌ نام موفق بود اما افزودن نقش ناموفق بود." });
                else
                    return Ok(new ApiResponse { Message = $"ثبت ‌نام کاربر با موفقیت انجام شد. - رمز عبور - {Password}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطا در هنگام ثبت ‌نام کاربر {model.Username}");
                return StatusCode(500, new ApiResponse { Message = "خطا." });
            }
        }


        /// <summary>
        /// دریافت لیست کاربران
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "HeadAdmin,Admin")]
        [HttpGet]
        [Route("Management/GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                // دریافت لیست کاربران
                string[] Roles = new[] { "Admin", "User", "HeadAdmin" };
                List<ShowUserModels> ShowUsers = new List<ShowUserModels>();
                foreach (var RoleName in Roles)
                { // برای هر نقش لیست کاربران را دریافت میکند
                    var Users = await _userManager.GetUsersInRoleAsync(RoleName);
                    ShowUsers.AddRange(Users.Select(z => new ShowUserModels
                    {
                        Id = z.Id,
                        Companies = z.CompanyId.ToString(),
                        Role = RoleName,
                        UserName = z.UserName,
                        FullName = z.FullName,
                        removeDate = z.RemoveDate
                    }).ToList());
                }
                return Ok(new ApiResponse { Message = "ثبت ‌نام کاربر با موفقیت انجام شد.", Data = ShowUsers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطا در هنگام دریافت لیست کاربران");
                return StatusCode(500, new ApiResponse { Message = "خطا." });
            }
        }

        /// <summary>
        /// بازنشانی رمز عبور برای کاربر مشخص شده و تولید رمز عبور جدید.
        /// </summary>
        /// <remarks>
        /// این متد فقط برای کاربران با نقش "HeadAdmin" یا "Admin" مجاز است.
        /// رمز عبور جدید به صورت خودکار تولید شده و در پیام پاسخ بازگردانده می‌شود.
        /// اطمینان حاصل کنید که فراخواننده مجوز لازم را دارد.
        /// </remarks>
        /// <param name="ThisUserId">شناسه یکتای کاربری که رمز عبور او باید بازنشانی شود.</param>
        /// <returns>
        /// یک <see cref="IActionResult"/> که نتیجه عملیات را نشان می‌دهد.
        /// اگر بازنشانی موفق باشد، کد وضعیت 200 به همراه رمز جدید بازگردانده می‌شود.
        /// اگر عملیات ناموفق باشد یا فراخواننده مجوز کافی نداشته باشد، کد وضعیت 400 بازگردانده می‌شود.
        /// اگر کاربر شناسایی نشود، کد وضعیت 401 و اگر کاربر یافت نشود، کد وضعیت 404 بازگردانده می‌شود.
        /// </returns>
        [Authorize(Roles = "HeadAdmin,Admin")]
        [HttpPost]
        [Route("Management/ResetUserPassword")]
        [SanitizeInput]
        public async Task<IActionResult> ResetUserPassword(string ThisUserId)
        {
            try
            {

                var ThisTblUser = await _userManager.FindByIdAsync(ThisUserId);
                if (ThisTblUser == null)
                    return StatusCode(400, new ApiResponse { Data = null, Message = "اکانت شما معتبر نیست." });
                var NewPassWord = GenerateEasyPassword(8);
                var Token = await _userManager.GeneratePasswordResetTokenAsync(ThisTblUser);

                var Reset = await _userManager.ResetPasswordAsync(ThisTblUser, Token, NewPassWord);

                return StatusCode(Reset.Succeeded ? 200 : 400, new ApiResponse { Message = Reset.Succeeded ? $"تغییر پسورد با موفقیت انجام گردید . رمز جدید :‌ {NewPassWord}" : "مشکلی در باز نشانی پسورد پیش آمد.", Data = null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطا در هنگام دریافت ریست پسورد کاربران");
                return StatusCode(500, new ApiResponse { Message = "خطا." });
            }
        }

        /// <summary>
        /// غیر فعال سازی یک کاربر
        /// </summary>
        /// <param name="disableUserModel"></param>
        /// <returns></returns>
        [Authorize(Roles = "HeadAdmin,Admin")]
        [HttpPost]
        [Route("Management/DisableUsers")]
        [SanitizeInput]
        public async Task<IActionResult> DisableUsers(DisableUserModel disableUserModel)
        {
            try
            {
                var ThisTblUser = await _userManager.FindByIdAsync(disableUserModel.ThisUserId);
                if (ThisTblUser == null)
                    return StatusCode(403, new ApiResponse { Data = null, Message = "اکانت شما معتبر نیست." });
                if (disableUserModel.IsForEnable && disableUserModel.IsForDisable)
                    return StatusCode(400, new ApiResponse { Message = "شما مجاز به این کار نیستید." });
                if (disableUserModel.IsForDisable)
                {
                    if (ThisTblUser.RemoveDate != null)
                        return StatusCode(400, new ApiResponse { Message = "کاربر غیرفعال است." });
                    ThisTblUser.RemoveDate = DateTime.Now;

                }
                else if (disableUserModel.IsForEnable)
                {
                    if (ThisTblUser.RemoveDate == null)
                        return StatusCode(400, new ApiResponse { Message = "کاربر فعال است." });
                    ThisTblUser.RemoveDate = null;
                }
                else
                    return StatusCode(400, new ApiResponse { Message = "شما مجاز به این کار نیستید." });
                var Saved = await _userManager.UpdateAsync(ThisTblUser);
                return StatusCode(Saved.Succeeded ? 200 : 400, new ApiResponse { Message = Saved.Succeeded ? $"تغییر کاربر با موفقیت انجام گردید." : "مشکلی در تغییر کاربر پیش آمد.", Data = null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطا در هنگام تغییر کاربر");
                return StatusCode(500, new ApiResponse { Message = "خطا." });
            }
        }

        /// <summary>
        /// افزودن یا ویرایش شرکت جدید
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "HeadAdmin")]
        [Route("Management/AddOrEditCompany")]
        public async Task<IActionResult> AddOrEditCompany([FromBody] CompanyModel model)
        {
            try
            {
                /// افزودن یا ویرایش شرکت جدید
                var result = await _companyService.AddOrEditCompanyFields(model);
                if (result.Execute)
                    return Ok(new ApiResponse { Message = result.Message });
                else
                    return BadRequest(new ApiResponse { Message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AddOrEditCompanyFields");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا." });
            }
        }

        /// <summary>
        /// افزودن یا ویرایش دسته بندی جدید
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "HeadAdmin")]
        [Route("Management/AddOrEditCategory")]
        [SanitizeInput]
        public async Task<IActionResult> AddOrEditCategory([FromBody] CategoryModel model)
        {
            try
            {
                // افزودن یا ویرایش دسته بندی جدید
                var result = await _categoryService.AddOrEditCategoryFields(model);
                if (result.Execute)
                    return StatusCode(200, new ApiResponse { Message = result.Message });
                else
                    return StatusCode(400, new ApiResponse { Message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AddOrEditCategoryFields");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا." });
            }
        }

        /// <summary>
        /// افزودن یا ویرایش آیتم جدید
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "HeadAdmin")]
        [Route("Management/AddOrEditItem")]
        public async Task<IActionResult> AddOrEditItem([FromBody] ItemModel model)
        {
            try
            {
                // افزودن یا ویرایش آیتم جدید
                var result = await _itemService.AddOrEditItemFields(model);
                return StatusCode(result.Execute ? 200 : 400, new ApiResponse { Message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AddOrEditItemFields");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا." });
            }
        }

        /// <summary>
        /// نسبت دادن آیتم به شرکت ها
        /// </summary>
        /// <param name="itemId">آیتم آیدی</param>
        /// <param name="companyIds">آیدی های شرکت ها</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("Management/AssignCompaniesToItem")]
        public async Task<IActionResult> AssignCompaniesToItem(int itemId, List<int> companyIds)
        {
            try
            {
                // نسبت دادن آیتم به شرکت ها
                var result = await _itemService.AssignCompaniesToItem(itemId, companyIds);
                return StatusCode(result.Execute ? 200 : 400, new ApiResponse { Message = result.Message });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AssignCompaniesToItem");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا." });
            }
        }

        /// <summary>
        /// غیرفعال سازی شرکت
        /// </summary>
        /// <param name="CompanyId">شناسه شرکت</param>
        /// <returns></returns>
        [Route("Management/DeactiveCompany")]
        [HttpPost]
        [Authorize(Roles = "HeadAdmin")]
        [SanitizeInput]
        public async Task<IActionResult> DeactiveCompany(string CompanyId)
        {
            try
            {
                // غیرفعال سازی شرکت
                var result = await _companyService.DeactiveCompany(CompanyId);
                return StatusCode(result.Execute ? 200 : 400, new ApiResponse { Message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Deactiving Company");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا." });
            }
        }

        /// <summary>
        /// فعال سازی شرکت
        /// </summary>
        /// <param name="CompanyId">شناسه شرکت</param>
        /// <returns></returns>
        [Route("Management/ActiveCompany")]
        [HttpPost]
        [Authorize(Roles = "HeadAdmin")]
        [SanitizeInput]
        public async Task<IActionResult> ActiveCompany(string CompanyId)
        {
            try
            {
                var result = await _companyService.ActiveCompany(CompanyId);
                return StatusCode(result.Execute ? 200 : 400, new ApiResponse { Message = result.Message });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Activating Company");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا." });
            }
        }

        /// <summary>
        /// غیرفعال سازی دسته بندی
        /// </summary>
        /// <param name="CategoryId">شناسه دسته بندی</param>
        /// <returns></returns>
        [Route("Management/DeactiveCategory")]
        [HttpGet]
        [Authorize(Roles = "HeadAdmin")]
        [SanitizeInput]
        public async Task<IActionResult> DeactiveCategory(string CategoryId)
        {
            try
            {
                var result = await _categoryService.DeactiveCategory(CategoryId);
                return StatusCode(result.Execute ? 200 : 400, new ApiResponse { Message = result.Message });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Deactiving Category");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا." });
            }
        }

        /// <summary>
        /// فعال سازی دسته بندی
        /// </summary>
        /// <param name="CategoryId">شناسه دسته بندی</param>
        /// <returns></returns>
        [Route("Management/ActiveCategory")]
        [HttpPost]
        [Authorize(Roles = "HeadAdmin")]
        [SanitizeInput]
        public async Task<IActionResult> ActiveCategory(string CategoryId)
        {
            try
            {
                var result = await _categoryService.ActiveCategory(CategoryId);
                return StatusCode(result.Execute ? 200 : 400, new ApiResponse { Message = result.Message });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Activating Category");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا." });
            }
        }


        /// <summary>
        /// غیر فعال کردن یک آیتم
        /// </summary>
        /// <param name="ItemId">شناسه آیتم</param>
        /// <returns></returns>
        [Route("Management/DeactiveItem")]
        [HttpGet]
        [Authorize(Roles = "HeadAdmin")]
        [SanitizeInput]
        public async Task<IActionResult> DeactiveItem(string ItemId)
        {
            try
            {
                var result = await _itemService.DeactiveItem(ItemId);
                return StatusCode(result.Execute ? 200 : 400, new ApiResponse { Message = result.Message });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Deactivating Item");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا در غیرفعال‌سازی آیتم." });
            }
        }

        /// <summary>
        /// فعال کردن یک آیتم
        /// </summary>
        /// <param name="ItemId">شناسه آیتم</param>
        /// <returns></returns>
        [Route("Management/ActiveItem")]
        [HttpPost]
        [Authorize(Roles = "HeadAdmin")]
        [SanitizeInput]
        public async Task<IActionResult> ActiveItem(string ItemId)
        {
            try
            {
                var result = await _itemService.ActiveItem(ItemId);
                return StatusCode(result.Execute ? 200 : 400, new ApiResponse { Message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Activating Item");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا در غیرفعال‌سازی آیتم." });
            }
        }
        /// <summary>
        /// Generates a random password with at least one uppercase letter, one lowercase letter, one number, and one special character.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static string GenerateEasyPassword(int length = 8)
        {
            if (length < 3)
                throw new ArgumentException("Password length must be at least 3 characters.");

            var password = new List<char>();

            // Ensure at least one of each
            password.Add(upperChars[random.Next(upperChars.Length)]);
            password.Add(lowerChars[random.Next(lowerChars.Length)]);
            password.Add(numberChars[random.Next(numberChars.Length)]);
            password.Add(specialChars[random.Next(specialChars.Length)]);
            // Fill the rest randomly
            for (int i = password.Count; i < length; i++)
            {
                password.Add(easyChars[random.Next(easyChars.Length)]);
            }

            // Shuffle the password to avoid predictable pattern
            return new string(password.OrderBy(x => random.Next()).ToArray());
        }
    }
}
