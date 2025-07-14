using Pack.Common.Convertor;
using Pack.Common.Public;
using Monitoring.Support.Server.Models.DatabaseModels;
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
    public class MonitorController : ControllerBase
    {
        private readonly UserManager<TblUser> _userManager;
        private readonly ILogger<MonitorController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly IMonitorService _monitorService;
        private readonly ICompanyService _companyService;
        private readonly ICategoryService _categoryService;
        private readonly IItemService _itemService;
        private readonly IMessagingService _messagingService;
        public MonitorController(UserManager<TblUser> userManager, ILogger<MonitorController> logger, IConfiguration configuration, IMemoryCache memoryCache, IMonitorService monitorService, ICompanyService companyService, ICategoryService categoryService, IItemService itemService, IMessagingService messagingService)
        {
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
            _memoryCache = memoryCache;
            _monitorService = monitorService;
            _companyService = companyService;
            _categoryService = categoryService;
            _itemService = itemService;
            _messagingService = messagingService;
        }

        #region Company

        /// <summary>
        /// دریافت لیستی از شرکت ها
        /// </summary>
        /// <returns></returns>
        [Route("Monitor/GetListOfCompanies")]
        [HttpGet]
        [Authorize(Roles = "HeadAdmin,Admin")]
        public async Task<IActionResult> GetListOfCompanies()
        {
            try
            {
                // دریافت لیست شرکت‌ها از سرویس
                var CompanyList = await _companyService.GetCompanyList();
                CompanyList.ForEach(z => z.ApiPassword = "");
                return StatusCode(200, new ApiResponse { Message = "لیست با موفقیت دریافت شد.", Data = CompanyList });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Getting GetCompanyList");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا." });
            }
        }

        //[Route("Monitor/SentTest")]
        //[HttpGet]
        //public async Task<IActionResult> SentTest(string message, string recipient)
        //{
        //    try
        //    {
        //        var botInfo = await _messagingService.GetBotInfoAsync();
        //        if (botInfo == null)
        //        {
        //            return BadRequest(new ApiResponse { Message = "توکن ربات معتبر نیست." });
        //        }
        //        bool userExists = await _messagingService.CheckUserExistsAsync(recipient);
        //        if (!userExists)
        //        {
        //            return BadRequest(new ApiResponse { Message = "کاربر موجود نیست." });
        //        }   
        //        await _messagingService.SendMessageAsync(message, recipient);
        //        return Ok(new ApiResponse { Message = "پیام با موفقیت ارسال شد." });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error Sending Test Message");
        //        return StatusCode(500, new ApiResponse { Data = null, Message = "خطا در ارسال پیام." });
        //    }
        //}


        /// <summary>
        /// دریافت کمبو باکس لیست استان ها
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "HeadAdmin,Admin")]
        [Route("Monitor/GetCompanyComboBox")]
        public async Task<IActionResult> GetCompanyComboBox()
        {
            try
            {
                // دریافت لیست استان ها
                var ComboBox = await _companyService.GetCompanyComboBox();
                return Ok(new ApiResponse { Message = "لیست با موفقیت دریافت شد.", Data = ComboBox ?? new List<ComboItem>() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating GetCompanyComboBox");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا." });
            }
        }


        #endregion

        #region Category

        /// <summary>
        /// دریافت لیستی از دسته بندی ها
        /// </summary>
        /// <returns></returns>
        [Route("Monitor/GetListOfCategories")]
        [HttpGet]
        [Authorize(Roles = "HeadAdmin,Admin")]
        public async Task<IActionResult> GetListOfCategories()
        {
            try
            {
                var categoryList = await _categoryService.GetCategoryList();
                return Ok(new ApiResponse { Message = "لیست دسته‌بندی‌ها با موفقیت دریافت شد.", Data = categoryList });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Getting Category List");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا در دریافت دسته‌بندی‌ها." });
            }
        }

        #endregion

        #region Item
        /// <summary>
        /// دریافت لیستی از آیتم ها به صورت مدل
        /// </summary>
        /// <returns></returns>
        [Route("Monitor/GetListOfItems")]
        [HttpGet]
        [Authorize(Roles = "HeadAdmin,Admin")]
        public async Task<IActionResult> GetListOfItems()
        {
            try
            {
                var itemList = await _itemService.GetItemList(true);
                return Ok(new ApiResponse { Message = "لیست آیتم‌ها با موفقیت دریافت شد.", Data = itemList });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Getting GetListOfItems ");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا در دریافت آیتم‌ها." });
            }
        }
        /// <summary>
        /// دریافت کمبو باکس آیتم ها جهت انتخاب
        /// </summary>
        /// <returns></returns>
        [Route("Monitor/GetListOfItemsCombo")]
        [HttpGet]
        [Authorize(Roles = "HeadAdmin,Admin")]
        public async Task<IActionResult> GetListOfItemsCombo()
        {
            try
            {
                // دریافت کمبو باکس آیتم ها جهت انتخاب
                var itemComboList = await _itemService.GetItemListCombo();
                return Ok(new ApiResponse { Message = "لیست آیتم‌ها با موفقیت دریافت شد.", Data = itemComboList });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Getting GetListOfItemsCombo ");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا در دریافت آیتم‌ها." });
            }
        }


        /// <summary>
        /// دریافت statuc codes کمبو آیتم
        /// </summary>
        /// <returns></returns>
        [Route("Monitor/GetTblStatusCode")]
        [HttpGet]
        [Authorize(Roles = "HeadAdmin,Admin")]
        public async Task<IActionResult> GetTblStatusCode()
        {
            try
            {
                // دریافت لیست شرکت‌ها از سرویس
                var TblStatusCombo = await _itemService.GetTblStatusCombo();
                return StatusCode(200, new ApiResponse { Message = "لیست با موفقیت دریافت شد.", Data = TblStatusCombo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Getting GetTblStatusCode");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا." });
            }
        }
        #endregion

        #region Status
        /// <summary>
        /// ذخیره نتایج وضعیت  هر شرکت و بروز رسانی ایتم لاگ های مربوط به آن
        /// </summary>
        /// <param name="CompanyId">شناسه شرکت</param>
        /// <param name="ItemId">شناسه آیتم</param>
        /// <returns></returns>
        [Route("Monitor/CallStatus")]
        [HttpPost]
        [Authorize(Roles = "HeadAdmin,Admin")]
        public async Task<IActionResult> CallStatus(int CompanyId, int ItemId)
        {
            MethodInformation methodInformation = new("ذخیره نتایج وضعیت  هر شرکت و بروز رسانی ایتم لاگ های مربوط به آن");
            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int userIdInt = userId.ToIntFromString() ?? 0;
                var result = await _monitorService.CallStatus(methodInformation.ExtraInfo, CompanyId, ItemId, userIdInt);
                return Ok(new ApiResponse { Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CallStatus");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا در انجام عملیات." });
            }
        }
        #endregion

        #region ItemLog
        /// <summary>
        /// دریافت لیستی از آیتم لاگ ها بر اساس فیلترهای انتخابی (تاریخ، کد شرکت، وضعیت و ...)
        /// </summary>
        /// <param name="FromDate">تاریخ شروع (اجباری) به فرمت yyyy/MM/dd</param>
        /// <param name="ToDate">تاریخ پایان (اختیاری) به فرمت yyyy/MM/dd</param>
        /// <param name="HtmlStatus">کد وضعیت HTTP به‌صورت عددی (مثلاً 200، 404 و ...)</param>
        /// <param name="CompanyCode">کد عددی شرکت جهت فیلتر لاگ‌ها</param>
        /// <param name="ItemCode">کد عددی آیتم جهت فیلتر لاگ‌ها</param>
        /// <param name="StatusItem">کد عددی وضعیت جهت فیلتر لاگ‌ها</param>
        /// <param name="Page">شماره صفحه (اختیاری، پیش‌فرض 1)</param>
        /// <param name="PageSize">تعداد آیتم‌ها در هر صفحه (اختیاری، پیش‌فرض 100)</param>
        /// <returns>آبجکتی شامل لیست فیلترشده از آیتم لاگ ها به همراه پیغام وضعیت</returns>
        [Route("Monitor/GetListOfItemLogs")]
        [HttpGet]
        [Authorize(Roles = "HeadAdmin,Admin")]
        public async Task<IActionResult> GetListOfItemLogs(int? Page = 1, int? PageSize = 100, string? FromDate = null, string? ToDate = null, __HttpStatus? HtmlStatus = null, string? CompanyCode = null, string? ItemCode = null, string? StatusItem = null)
        {
            try
            {
                Page ??= 1;
                PageSize ??= 100;
                var ItemLogList = await _monitorService.GetItemLogList(Page.Value, PageSize.Value, FromDate, ToDate, HtmlStatus, CompanyCode, ItemCode, StatusItem);
                return Ok(new ApiResponse { Message = "لیست آیتم لاگ ‌ها با موفقیت دریافت شد.", Data = ItemLogList });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Getting ItemLog List");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا در دریافت آیتم لاگ‌ ها." });
            }
        }
        #endregion
        
        /// <summary>
        /// دریافت کمبو باکس لاگ استان ها
        /// </summary>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        [Route("Monitor/BindComboOfComapnyLogs")]
        [HttpPost]
        [Authorize(Roles = "HeadAdmin,Admin")]
        public async Task<IActionResult> BindComboOfComapnyLogs(int CompanyId)
        {
            MethodInformation methodInformation = new("دریافت رویدادهای سامانه خدمات مشترکین");
            try
            {
                var ComboOfComapnyLogs = await _monitorService.BindComboOfComapnyLogs(methodInformation.ExtraInfo, CompanyId);
                return Ok(new ApiResponse { Data = ComboOfComapnyLogs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Getting CompanyLogs List");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا در دریافت آیتم لاگ‌ ها." });
            }
        }

        /// <summary>
        /// دریافت لیست باکس لاگ استان ها
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("Monitor/GetListOfCompanyLogs")]
        [HttpPost]
        [Authorize(Roles = "HeadAdmin,Admin")]
        public async Task<IActionResult> GetListOfCompanyLogs([FromBody] FilteredLogsModel model)
        {
            MethodInformation methodInformation = new("دریافت رویدادهای سامانه خدمات مشترکین");
            try
            {
                var CompanyLogsList = await _monitorService.GetCompanyLogsList(methodInformation.ExtraInfo, model);
                return Ok(new ApiResponse { Message = "لیست لاگ های شرکت ها با موفقیت دریافت شد.", Data = CompanyLogsList });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Getting CompanyLogs List");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا در دریافت آیتم لاگ‌ ها." });
            }
        }


        /// <summary>
        /// دریافت لیست کلی مانیتورینگ شرکت‌ها
        /// </summary>
        /// <returns></returns>
        [Route("Monitor/GetComponiesMonitoring")]
        [HttpGet]
        [Authorize(Roles = "HeadAdmin,Admin")]
        public async Task<IActionResult> GetComponiesMonitoring()
        {
            MethodInformation methodInformation = new MethodInformation("دریافت لیست شرکت های در حال مانیتورینگ");
            try
            {
                var ComponiesMonitoring = await _monitorService.GetCompaniesMonitoring();
                return Ok(new ApiResponse { Message = "لیست شرکت‌ها و وضعیت‌های نظارتی آن‌ها با موفقیت دریافت شد.", Data = ComponiesMonitoring });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Getting Componies Monitoring List");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا در دریافت لیست شرکت‌ها." });
            }
        }

        /// <summary>
        /// دریافت مانیتورینگ تک شرکت
        /// </summary>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        [Route("Monitor/GetSingleCompanyMonitoring")]
        [HttpGet]
        [Authorize(Roles = "HeadAdmin,Admin")]
        public async Task<IActionResult> GetSingleCompanyMonitoring(int CompanyId)
        {
            MethodInformation methodInformation = new MethodInformation("دریافت مانیتورینگ یک شرکت");
            try
            {
                var singleCompanyMonitoring = await _monitorService.GetSingleCompanyMonitoring(CompanyId);
                return Ok(new ApiResponse { Message = "دریافت مانیتورینگ یک شرکت با موفقیت انجام شد.", Data = singleCompanyMonitoring });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Getting Single Company Monitoring");
                return StatusCode(500, new ApiResponse { Data = null, Message = "خطا در دریافت مانیتورینگ یک شرکت." });
            }
        }


    }
}
