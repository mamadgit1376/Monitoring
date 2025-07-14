using Monitoring.Support.Server.Models.DatabaseModels;
using Monitoring_Support_Server.Models.ViewModels;

namespace Monitoring_Support_Server.Services.Interfaces
{
    public interface ICategoryService
    {
        /// <summary>
        /// افزودن یا ویرایش دسته بندی جدید
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ResultModel> AddOrEditCategoryFields(CategoryModel model);

        /// <summary>
        /// دریافت لیستی از دسته بندی ها
        /// </summary>
        /// <returns></returns>
        Task<List<TblCategory>> GetCategoryList();

        /// <summary>
        /// غیرفعال سازی دسته بندی
        /// </summary>
        /// <param name="CategoryId">شناسه دسته بندی</param>
        /// <returns></returns>
        Task<ResultModel> DeactiveCategory(string CategoryId);

        /// <summary>
        /// فعال سازی دسته بندی
        /// </summary>
        /// <param name="CategoryId">شناسه دسته بندی</param>
        /// <returns></returns>
        Task<ResultModel> ActiveCategory(string CategoryId);
    }
}
