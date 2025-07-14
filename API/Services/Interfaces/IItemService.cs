using Monitoring_Support_Server.Models.ViewModels;

namespace Monitoring_Support_Server.Services.Interfaces
{
    public interface IItemService
    {
        /// <summary>
        /// افزودن یا ویرایش آیتم جدید
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ResultModel> AddOrEditItemFields(ItemModel model);

        /// <summary>
        /// نسبت دادن آیتم به شرکت ها
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="companyIds"></param>
        /// <returns></returns>
        Task<ResultModel> AssignCompaniesToItem(int itemId, List<int> companyIds);

        /// <summary>
        /// دریافت لیستی از آیتم ها
        /// </summary>
        /// <returns></returns>
        Task<List<ComboItem>> GetTblStatusCombo();

        /// <summary>
        /// غیر فعال کردن یک آیتم
        /// </summary>
        /// <param name="ItemId">شناسه آیتم</param>
        /// <returns></returns>
        Task<ResultModel> DeactiveItem(string ItemId);

        /// <summary>
        /// فعال کردن یک آیتم
        /// </summary>
        /// <param name="ItemId">شناسه آیتم</param>
        /// <returns></returns>
        Task<ResultModel> ActiveItem(string ItemId);
        /// <summary>
        /// دریافت لیست آیتم ها به صورت مدل
        /// </summary>
        /// <returns></returns>
        Task<List<ItemViewModel>> GetItemList(bool WithRemoved = false);
        Task<List<ComboItem>> GetItemListCombo();
    }
}
