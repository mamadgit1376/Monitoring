using Pack.Common.Connecting;
using Pack.Common.Convertor;
using Pack.Common.Model;
using Pack.Common.Model.StatusItemResult;
using Monitoring.Support.Server.Models.DatabaseModels;
using Monitoring.Support.Services;
using Monitoring_Support_Server.Models.ViewModels;
using Monitoring_Support_Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;

namespace Monitoring_Support_Server.Services
{
    public class MonitorService : IMonitorService
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly IRestJsonService _restJsonService;
        private readonly ILogger<MonitorService> _logger;
        private readonly MonitoringDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        public MonitorService(ILogger<MonitorService> logger, IMemoryCache memoryCache, IConfiguration configuration, MonitoringDbContext db, IRestJsonService restJsonService, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _configuration = configuration;
            _db = db;
            _restJsonService = restJsonService;
            _httpClientFactory = httpClientFactory;
        }

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
        public async Task<List<ItemLogViewModel>> GetItemLogList(int Page, int PageSize, string? FromDate = null, string? ToDate = null, __HttpStatus? HtmlStatus = null, string? CompanyCode = null, string? ItemCode = null, string? StatusItem = null)
        {
            // تعریف کوئری اولیه روی جدول لاگ‌ها
            var query = _db.TblItemLog
                .Include(i => i.TblCompanyNavigation)
                .Include(i => i.TblStatusCodeNavigation)
                .Include(i => i.TblItemNavigation)
                .Where(x => x.RemoveDate == null);

            // فیلتر بر اساس تاریخ شروع
            if (!string.IsNullOrEmpty(FromDate))
            {
                var fromDate = FromDate.ToDateTimeFromString().GetValueOrDefault().Date.AddDays(1);
                query = query.Where(x => x.CreateDate >= fromDate);
            }
            // فیلتر بر اساس تاریخ پایان
            if (!string.IsNullOrEmpty(ToDate))
            {
                var toDate = ToDate.ToDateTimeFromString().GetValueOrDefault().Date.AddDays(1).AddMicroseconds(-1);
                query = query.Where(x => x.CreateDate <= toDate);
            }

            // فیلتر بر اساس شناسه شرکت
            if (!string.IsNullOrEmpty(CompanyCode))
            {
                var companyId = CompanyCode.ToIntFromString();
                query = query.Where(x => x.CompanyId == companyId);
            }

            // فیلتر بر اساس شناسه آیتم
            if (!string.IsNullOrEmpty(ItemCode))
            {
                var itemId = ItemCode.ToIntFromString();
                query = query.Where(x => x.ItemId == itemId);
            }

            // فیلتر بر اساس شناسه وضعیت آیتم
            if (!string.IsNullOrEmpty(StatusItem))
            {
                var statusId = StatusItem.ToIntFromString();
                query = query.Where(x => x.TblStatusCode == statusId);
            }

            // فیلتر بر اساس کد وضعیت HTTP
            if (HtmlStatus != null)
            {
                query = query.Where(x => x.ResponseHtmlStatus == HtmlStatus);
            }

            // اجرای کوئری و نگاشت به مدل نمایشی
            var result = await query
                .OrderByDescending(x => x.CreateDate)
                .Skip((Page - 1) * PageSize).
                Take(PageSize)
                .Select(z => new ItemLogViewModel
                {
                    ID = z.ID,
                    CreatorId = z.CreatorId,
                    CompanyName = z.TblCompanyNavigation.CompanyName,
                    CompanyId = z.TblCompanyNavigation.ID,
                    StatusName = z.TblStatusCode == null ? "" : z.TblStatusCodeNavigation.PersianName,
                    StatusDescription = z.TblStatusCode == null ? "" : z.TblStatusCodeNavigation.Description, //add
                    StatusType = z.TblStatusCode == null ? (short)0 : (short)z.TblStatusCodeNavigation.StatusType,
                    ItemName = z.TblItemNavigation.ItemName,
                    FullUrl = z.FullUrl,
                    CreateDate = z.CreateDate.ToShamsiFromDateTime(true),
                    HttpStatus = z.ResponseHtmlStatus,
                })
                .ToListAsync();

            // جایگزینی مقدار کد HTTP با متن توضیحی معادل آن
            result.ForEach(z =>
                z.ResponseHtmlStatus = HttpStatusHelper.HttpStatusDictionary[z.HttpStatus]);

            return result;
        }

        /// <summary>
        /// سرویس دریافت توکن از مهراب Api
        /// </summary>
        /// <param name="extraInfo"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        private async Task<string?> GetTokenFromWebService(StringBuilder extraInfo, TblCompany company, bool fromMemory = true)
        {
            // چک میکنیم که آیا از قبل توکنی برای شرکت موردنظر در مموری ذخیره شده است یا خیر
            string cacheKey = $"abfa_token_{company.ID}";
            if (fromMemory && _memoryCache.TryGetValue(cacheKey, out string cachedToken))
            {
                extraInfo.Append($"-Token read from memory cache for company {company.ID}");
                return cachedToken;
            }
            // حالتی که توکن در مموری منقضی شده است
            string? tokenResult = null;
            try
            {
                string checkUrl = $"{company.BaseUrlAddress}/login";
                extraInfo.Append($"-Calling URL: {checkUrl.ToStringForLog()}");
                var model = new ApiLoginModel
                {
                    username = company.ApiUser,
                    password = company.ApiPassword,
                };
                var jsonToken = await _restJsonService.PostForSite(extraInfo, Url: checkUrl, Obj: model, ApiAddressKey: "MainService");
                JwtToken? result = JsonSerializer.Deserialize<JwtToken>(jsonToken);

                tokenResult = result?.access_token;
                if (!string.IsNullOrEmpty(tokenResult))
                {
                    // ذخیره توکن در مموری به مدت ده دقیقه
                    _memoryCache.Set(cacheKey, tokenResult, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    });
                }
            }
            catch (Exception ex)
            {
                extraInfo.Append($"-Error in GetTokenFromAbfa: {ex.Message}");
                _logger.LogError(ex, "Error Exception");
            }

            return tokenResult;
        }

        /// <summary>
        /// متدی برای بررسی وضعیت یک آیتم از طریق سرور شرکت و ذخیره وضعیت در دیتابیس.
        /// </summary>
        /// <param name="extraInfo"></param>
        /// <param name="CompanyId">شناسه شرکت</param>
        /// <param name="ItemId">شناسه آیتم</param>
        /// <param name="UserId">شناسه کاربر (سازنده لاگ)</param>
        /// <returns>وضعیت نهایی آیتم</returns>
        public async Task<TblStatus> CallStatus(StringBuilder extraInfo, int CompanyId, int ItemId, int UserId)
        {
            // ایجاد آبجکت اولیه وضعیت با تاریخ ایجاد
            var result = new TblStatus
            {
                CreateDate = DateTime.Now,
            };

            try
            {
                // بررسی موجود بودن شرکت و آیتم در دیتابیس
                var company = await _db.TblCompany
                    .FirstOrDefaultAsync(x => x.ID == CompanyId && x.RemoveDate == null);
                var item = await _db.TblItem
                    .FirstOrDefaultAsync(x => x.ID == ItemId && x.RemoveDate == null);

                if (company == null || item == null)
                {
                    result.Name = "Invalid";
                    result.PersianName = "شرکت یا آیتم نامعتبر است";
                    result.StatusType = __StatusType.Error;
                    result.Description = $"CompanyId {CompanyId} یا ItemId {ItemId} معتبر نیست.";
                }
                else
                {
                    string baseUrl = company.BaseUrlAddress.TrimEnd('/');
                    string additionalUrl = item.AdditionalUrlAddress?.TrimStart('/') ?? "";
                    string callUrl = string.IsNullOrEmpty(additionalUrl) ? baseUrl : $"{baseUrl}/{additionalUrl}";

                    var httpClient = _httpClientFactory.CreateClient("MainService");
                    var request = new HttpRequestMessage(HttpMethod.Get, callUrl);
                    var Token = await GetTokenFromWebService(extraInfo, company, true);
                    if (!string.IsNullOrEmpty(Token))
                    {
                        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                    }

                    HttpResponseMessage httpResponse;
                    try
                    {
                        httpResponse = await httpClient.SendAsync(request);
                        if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            Token = await GetTokenFromWebService(extraInfo, company, false);
                            if (!string.IsNullOrEmpty(Token))
                            {
                                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                                httpResponse = await httpClient.SendAsync(request);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        extraInfo.Append($"-HttpClient Error: {ex.Message}");
                        throw;
                    }

                    var responseContent = await httpResponse.Content.ReadAsStringAsync();

                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var ExternalStatusResult = JsonSerializer.Deserialize<StatusItemResult>(responseContent);
                        int? statusId = null;

                        // پردازش هر آیتم وضعیت به صورت جداگانه
                        foreach (var ExternalStatus in ExternalStatusResult.StatusItems)
                        {
                            // بررسی وجود وضعیت مشابه در دیتابیس
                            var ExistingStatus = await _db.TblStatus
                                .Where(_ => _.Name == ExternalStatus.Name &&
                                            _.PersianName == ExternalStatus.PersianName &&
                                            _.Description == ExternalStatus.Description)
                                .FirstOrDefaultAsync();

                            if (ExistingStatus != null)
                            {
                                // اگر وضعیت موجود است، از همان استفاده می‌کنیم
                                statusId = ExistingStatus.ID;
                            }
                            else
                            {
                                // ایجاد یک آبجکت جدید برای هر وضعیت
                                var newStatus = new TblStatus
                                {
                                    CreateDate = DateTime.Now,
                                    Name = ExternalStatus.Name,
                                    PersianName = ExternalStatus.PersianName,
                                    StatusType = (__StatusType)ExternalStatus.StatusType,
                                    Description = ExternalStatus.Description
                                };

                                _db.TblStatus.Add(newStatus);
                                await _db.SaveChangesAsync();

                                statusId = newStatus.ID;
                                result = newStatus;
                            }

                            // ثبت لاگ آیتم برای هر وضعیت
                            var itemLog = new TblItemLog
                            {
                                CreateDate = DateTime.Now,
                                FullUrl = callUrl,
                                TblStatusCode = statusId,
                                CompanyId = CompanyId,
                                ItemId = ItemId,
                                CreatorId = UserId,
                                ResponseHtmlStatus = (__HttpStatus)httpResponse.StatusCode,
                            };

                            _db.TblItemLog.Add(itemLog);
                            await _db.SaveChangesAsync();
                        }

                        // ثبت موفقیت‌آمیز بودن دریافت و ذخیره وضعیت‌ها
                        result.Name = "Success";
                        result.PersianName = "وضعیت ها با موفقیت دریافت و ذخیره شدند";
                        result.StatusType = __StatusType.Success;
                        result.Description = $"تعداد {ExternalStatusResult.StatusItems.Count} وضعیت از سرویس دریافت و ذخیره شد.";

                    }
                    else
                    {
                        // در صورت عدم موفقیت درخواست HTTP
                        result.Name = "HttpError";
                        result.PersianName = "خطای HTTP";
                        result.StatusType = __StatusType.Error;
                        result.Description = $"درخواست HTTP با کد {(int)httpResponse.StatusCode} ({httpResponse.StatusCode}) ناموفق بود.";

                        // ثبت لاگ برای خطای HTTP
                        var itemLog = new TblItemLog
                        {
                            CreateDate = DateTime.Now,
                            FullUrl = callUrl,
                            TblStatusCode = null,
                            CompanyId = CompanyId,
                            ItemId = ItemId,
                            CreatorId = UserId,
                            ResponseHtmlStatus = (__HttpStatus)httpResponse.StatusCode,
                        };

                        _db.TblItemLog.Add(itemLog);
                        await _db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                result.Name = "Error";
                result.PersianName = "خطا";
                result.StatusType = __StatusType.Error;
                result.Description = $"خطا در دریافت وضعیت از سرویس: {ex.Message}";
                _logger.LogError(ex, "Error Exception");
            }

            return result;
        }

        /// <summary>
        /// دریافت لیستی از آیتم  لاگ ها برای سرویس پس زمینه
        /// </summary>
        /// <param name="CompanyCodes">لیستی از کد های شرکت ها</param>
        /// <param name="itemId">کد آیتم</param>
        /// <param name="CheckTime">زمان ست شده توسط کاربر</param>
        /// <returns></returns>
        public async Task<List<TblItemLog>> GetItemLogForBGService(List<string> CompanyCodes, int itemId, DateTime CheckTime)
        {
            var logs = await _db.TblItemLog.Where(z => z.ResponseHtmlStatus == __HttpStatus.OK).Include(_ => _.TblCompanyNavigation)
                .Where(x =>
                    x.CreateDate >= CheckTime &&
                    x.ItemId == itemId &&
                    CompanyCodes.Contains(x.CompanyId.ToString()))
                .ToListAsync();

            return logs;
        }

        /// <summary>
        /// دریافت کمبوهای پنل رویدادها
        /// </summary>
        /// <param name="extraInfo"></param>
        /// <param name="CompanyId">کد شرکت</param>
        /// <returns></returns>
        public async Task<ClientLogCombos> BindComboOfComapnyLogs(StringBuilder extraInfo, int CompanyId)
        {
            ClientLogCombos result = new ClientLogCombos();

            try 
            {
                var company = await _db.TblCompany.FirstOrDefaultAsync(x => x.ID == CompanyId && x.RemoveDate == null);
                string callUrl = $"{company.BaseUrlAddress}/Monitoring/BindComboOfComapnyLogs";

            var httpClient = _httpClientFactory.CreateClient("MainService");
            var request = new HttpRequestMessage(HttpMethod.Get, callUrl);
            var Token = await GetTokenFromWebService(extraInfo, company,true);
            if (!string.IsNullOrEmpty(Token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            }
            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await httpClient.SendAsync(request);
                if( httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Token = await GetTokenFromWebService(extraInfo, company, false);
                    if (!string.IsNullOrEmpty(Token))
                    {
                        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                        httpResponse = await httpClient.SendAsync(request);
                    }
                }
            }
            catch (Exception ex)
            {
                extraInfo.Append($"-HttpClient Error: {ex.Message}");
                throw;
            }

                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseContent))
                {
                    var comboResult = JsonSerializer.Deserialize<ComboLogResult>(responseContent);
                    if (comboResult != null && comboResult.Logs != null && comboResult.Logs.Any())
                    {
                        foreach (var log in comboResult.Logs)
                        {
                            var comboLogModel = new ComboLogModel
                            {
                                Id = log.Id,
                                Name = log.Name,
                                Type = log.Type,
                                Description = log.Description
                            };

                            if (log.Type == "Function")
                            {
                                result.FunctionLogs.Add(comboLogModel);
                            }
                            if (log.Type == "Type")
                            {
                                result.TypeLogs.Add(comboLogModel);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                extraInfo.Append($"-Error in BindComboOfComapnyLogs: {ex.Message}");
                _logger.LogError(ex, "Error Exception");
            }

            return result;
        }

        /// <summary>
        /// دریاف لیستی از رویدادهای سامانه
        /// </summary>
        /// <param name="extraInfo"></param>
        /// <param name="model"></param>
        public async Task<List<LogsModel>> GetCompanyLogsList(StringBuilder extraInfo, FilteredLogsModel model)
        {
            List<LogsModel> result = new List<LogsModel>();

            try
            {
                var company = await _db.TblCompany.FirstOrDefaultAsync(x => x.ID == model.CompanyId && x.RemoveDate == null);
                string callUrl = $"{company.BaseUrlAddress}/Monitoring/GetLogsApi?" +
                                    $"ShamsiFilterDate={model.FilterDate}&" +
                                    $"FromTime={model.FromTime}&" +
                                    $"ToTime={model.ToTime}&" +
                                    $"LogTypeCodes={model.LogTypeCodes}&" +
                                    $"FilteredUsedIds={model.FilteredUsedIds}&" +
                                    $"EnteredUsedIds={model.EnteredUsedIds}&" +
                                    $"FunctionIds={model.FunctionIds}&" +
                                    $"FilterText={model.FilterText}&" +
                                    $"FilterIp={model.FilterIp}";

                extraInfo.Append($"-Calling URL: {callUrl.ToStringForLog()}");

                var httpClient = _httpClientFactory.CreateClient("MainService");
                var request = new HttpRequestMessage(HttpMethod.Get, callUrl);
                var Token = await GetTokenFromWebService(extraInfo, company, true);
                if (!string.IsNullOrEmpty(Token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                }
                HttpResponseMessage httpResponse;
                try
                {
                    httpResponse = await httpClient.SendAsync(request);
                    if(httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Token = await GetTokenFromWebService(extraInfo, company, false);
                        if (!string.IsNullOrEmpty(Token))
                        {
                            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                            httpResponse = await httpClient.SendAsync(request);
                        }
                    }
                }
                catch (Exception ex)
                {
                    extraInfo.Append($"-HttpClient Error: {ex.Message}");
                    throw;
                }

                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseContent))
                {
                    var logResult = JsonSerializer.Deserialize<LogsResult>(responseContent);

                    // اگر LogResult نال نبود و لیست Logs هم خالی نبود
                    if (logResult != null && logResult.Logs != null && logResult.Logs.Any())
                    {
                        foreach (var log in logResult.Logs)
                        {
                            var logModel = new LogsModel()
                            {
                                CompanyId = model.CompanyId, // این مقدار رو همون مقدار ورودی میگذاریم
                                AddDate = log.AddDate,
                                AddTime = log.AddTime.Length > 12 ? log.AddTime.Substring(0, 12) : log.AddTime,
                                FinishTime = log.FinishTime.Length > 12 ? log.FinishTime.Substring(0, 12) : log.FinishTime,
                                AgentName = log.AgentName,
                                BrowserAddress = log.BrowserAddress,
                                ClassName = log.ClassName,
                                ExtraInfo = log.ExtraInfo,
                                FunctionDescription = log.FunctionDescription,
                                FunctionName = log.FunctionName,
                                IP = log.IP,
                                LogType = log.LogType,
                                Message = log.Message,
                                NameOfUser = log.NameOfUser,
                                StackTrace = log.StackTrace,
                                Time = log.Time,
                            };
                            result.Add(logModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                extraInfo.Append($"-Error in GetCompanyLogsList: {ex.Message}");
                _logger.LogError(ex, "Error Exception");
            }

            return result;
        }


        public async Task<List<ShowMonitoringModel>> GetCompaniesMonitoring()
        {
            List<ShowMonitoringModel> monitoringModels = new List<ShowMonitoringModel>();

            // دریافت لیست شرکت‌ها که در حالت مانیتورینگ هستند
            var companies = await _db.TblCompany
                .Where(c => c.RemoveDate == null)
                .ToListAsync();
            if (companies == null || companies.Count == 0)
            {
                return monitoringModels; // اگر هیچ شرکتی وجود ندارد، لیست خالی برمی‌گردانیم
            }
            var TotalItems = await _db.TblItem
                   .Where(i => i.RemoveDate == null).Include(i => i.TblCompanies)
                   .ToListAsync();
            foreach (var company in companies)
            {
                int SuccessCount = 0;
                int errorCount = 0;
                int WarningCount = 0;
                var Items = TotalItems.Where(i => i.TblCompanies.Contains(company)).ToList();
                if (Items.Count == 0)
                {
                    continue; // اگر هیچ آیتمی برای شرکت وجود ندارد، ادامه می‌دهیم
                }
                foreach (var item in Items)
                {
                    var itemLog = await _db.TblItemLog
                        .Where(x => x.ItemId == item.ID && x.CompanyId == company.ID && x.RemoveDate == null)
                        .Include(i => i.TblStatusCodeNavigation)
                        .OrderByDescending(z => z.CreateDate)
                        .FirstOrDefaultAsync();
                    if (itemLog != null && itemLog.ResponseHtmlStatus == __HttpStatus.OK)
                    {
                        if (itemLog.TblStatusCode == null)
                            errorCount += 1;
                        else if (itemLog.TblStatusCodeNavigation.StatusType == __StatusType.Success)
                            SuccessCount += 1;
                        else if (itemLog.TblStatusCodeNavigation.StatusType == __StatusType.Warning)
                            WarningCount += 1;
                        else if (itemLog.TblStatusCodeNavigation.StatusType == __StatusType.Error)
                            errorCount += 1;
                    }
                    else
                    {
                        errorCount += 1;
                    }
                }
                ShowMonitoringModel showMonitoringModel = new ShowMonitoringModel
                {
                    CompanyId = company.ID,
                    CompanyName = company.CompanyName,
                    SuccessCount = SuccessCount,
                    ErrorCount = errorCount,
                    WarningCount = WarningCount,
                    PercentSuccess = TotalItems.Count > 0 ? (SuccessCount * 100) / Items.Count : 0,
                };
                monitoringModels.Add(showMonitoringModel);
            }
            return monitoringModels;
        }


        public async Task<ShowSingleCompanyMonitoring> GetSingleCompanyMonitoring(int CompanyId)
        {
            var Company = await _db.TblCompany.FirstOrDefaultAsync(x => x.ID == CompanyId && x.RemoveDate == null);
            var ItemLogs = await _db.TblItemLog.Where(z => z.CompanyId == CompanyId && z.RemoveDate == null).Include(z => z.TblItemNavigation).Include(z => z.TblStatusCodeNavigation).OrderByDescending(z => z.CreateDate).Take(100).ToListAsync();

            if (Company == null)
            {
                return new ShowSingleCompanyMonitoring();
            }

            var showSingleCompanyMonitoring = new ShowSingleCompanyMonitoring
            {
                CompanyId = Company.ID,
                CompanyName = Company.CompanyName,
                ItemLog = ItemLogs.Select(z => new ItemLogViewModel
                {
                    ID = z.ID,
                    CreatorId = z.CreatorId,
                    CompanyName = Company.CompanyName,
                    CompanyId = Company.ID,
                    StatusName = z.TblStatusCode == null ? "" : z.TblStatusCodeNavigation.PersianName,
                    StatusDescription = z.TblStatusCode == null ? "" : z.TblStatusCodeNavigation.Description, //add
                    StatusType = z.TblStatusCode == null ? (short)0 : (short)z.TblStatusCodeNavigation.StatusType,
                    ItemName = z.TblItemNavigation.ItemName,
                    FullUrl = z.FullUrl,
                    CreateDate = z.CreateDate.ToShamsiFromDateTime(true),
                    HttpStatus = z.ResponseHtmlStatus,
                    ResponseHtmlStatus = ""
                }).ToList()
            };
            showSingleCompanyMonitoring.ItemLog.ForEach(z =>
             z.ResponseHtmlStatus = HttpStatusHelper.HttpStatusDictionary[z.HttpStatus]);

            return showSingleCompanyMonitoring;
        }
    }
}