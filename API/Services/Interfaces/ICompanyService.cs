using Monitoring.Support.Server.Models.DatabaseModels;
using Monitoring_Support_Server.Models.ViewModels;

namespace Monitoring_Support_Server.Services.Interfaces
{
    public interface ICompanyService
    {
        /// <summary>
        /// افزودن یا ویرایش شرکت جدید
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ResultModel> AddOrEditCompanyFields(CompanyModel model);
        /// <summary>
        /// دریافت لیست شرکت ها
        /// </summary>
        /// <returns></returns>
        Task<List<ComboItem>> GetCompanyComboBox();

        /// <summary>
        /// بررسی وجود شرکت
        /// </summary>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        Task<bool> IsValidCompany(int CompanyId);

        /// <summary>
        /// دریافت لیستی از شرکت ها
        /// </summary>
        /// <returns></returns>
        Task<List<TblCompany>> GetCompanyList();

        /// <summary>
        /// غیرفعال سازی شرکت
        /// </summary>
        /// <param name="CompanyId">شناسه شرکت</param>
        /// <returns></returns>
        Task<ResultModel> DeactiveCompany(string CompanyId);

        /// <summary>
        /// فعال سازی شرکت
        /// </summary>
        /// <param name="CompanyId">شناسه شرکت</param>
        /// <returns></returns>
        Task<ResultModel> ActiveCompany(string CompanyId);
    }
}
