using Monitoring.Support.Server.Models.DatabaseModels;
using Monitoring_Support_Server.Models.ViewModels;
using System.Text;

namespace Monitoring_Support_Server.Services.Interfaces
{
    public interface IMonitorService
    {

        /// <summary>
        /// متدی برای بررسی وضعیت یک آیتم از طریق سرور شرکت و ذخیره وضعیت در دیتابیس.
        /// </summary>
        /// <param name="extraInfo"></param>
        /// <param name="CompanyId">شناسه شرکت</param>
        /// <param name="ItemId">شناسه آیتم</param>
        /// <param name="UserId">شناسه کاربر (سازنده لاگ)</param>
        /// <returns>وضعیت نهایی آیتم</returns>
        Task<TblStatus> CallStatus(StringBuilder extraInfo, int CompanyId, int ItemId, int UserId);

        /// <summary>
        /// دریافت لیستی از آیتم لاگ ها بر اساس فیلترهای انتخابی (تاریخ، کد شرکت، وضعیت و ...)
        /// </summary>
        /// <param name="FromDate">تاریخ شروع (اجباری) به فرمت yyyy/MM/dd</param>
        /// <param name="ToDate">تاریخ پایان (اختیاری) به فرمت yyyy/MM/dd</param>
        /// <param name="HtmlStatus">کد وضعیت HTTP به‌صورت عددی (مثلاً 200، 404 و ...)</param>
        /// <param name="CompanyCode">کد عددی شرکت جهت فیلتر لاگ‌ها</param>
        /// <param name="ItemCode">کد عددی آیتم جهت فیلتر لاگ‌ها</param>
        /// <param name="StatusItem">کد عددی وضعیت جهت فیلتر لاگ‌ها</param>
        /// <returns>آبجکتی شامل لیست فیلترشده از آیتم لاگ ها به همراه پیغام وضعیت</returns>
        Task<List<ItemLogViewModel>> GetItemLogList(int Page, int PageSize, string? FromDate = null, string? ToDate = null, __HttpStatus? HtmlStatus = null, string? CompanyCode = null, string? ItemCode = null, string? StatusItem = null);

        /// <summary>
        /// دریافت لیستی از آیتم  لاگ ها برای سرویس پس زمینه
        /// </summary>
        /// <param name="CompanyCodes">لیستی از کد های شرکت ها</param>
        /// <param name="itemId">کد آیتم</param>
        /// <param name="CheckTime">زمان ست شده توسط کاربر</param>
        /// <returns></returns>
        Task<List<TblItemLog>> GetItemLogForBGService(List<string> CompanyCodes, int itemId, DateTime CheckTime);

        /// <summary>
        /// دریاف لیستی از رویدادهای سامانه
        /// </summary>
        /// <param name="extraInfo"></param>
        /// <param name="model"></param>
        Task<List<LogsModel>> GetCompanyLogsList(StringBuilder extraInfo, FilteredLogsModel model);
        /// <summary>
        /// دریافت لیستی از شرکت‌ها و وضعیت‌های نظارتی آن‌ها
        /// </summary>
        /// <returns></returns>
        Task<List<ShowMonitoringModel>> GetCompaniesMonitoring();

        /// <summary>
        /// دریافت وضعیت نظارتی یک شرکت خاص
        /// </summary>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        Task<ShowSingleCompanyMonitoring> GetSingleCompanyMonitoring(int CompanyId);

        /// <summary>
        /// دریافت کمبوهای پنل رویدادها
        /// </summary>
        /// <param name="extraInfo"></param>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        Task<ClientLogCombos> BindComboOfComapnyLogs(StringBuilder extraInfo, int CompanyId);
    }
}
