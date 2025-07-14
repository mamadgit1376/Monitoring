using AbfaCS.Common.Encryptor;
using Monitoring.Support.Server.Models.DatabaseModels;
using Monitoring.Support.Services;
using Monitoring_Support_Server.Models.ViewModels;
using Monitoring_Support_Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Monitoring_Support_Server.Services
{
    public class ItemService : IItemService
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ItemService> _logger;
        private readonly MonitoringDbContext _db;

        public ItemService(ILogger<ItemService> logger, IMemoryCache memoryCache, IConfiguration configuration, MonitoringDbContext db)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _configuration = configuration;
            _db = db;
        }

        public async Task<ResultModel> AddOrEditItemFields(ItemModel model)
        {
            try
            {
                var Now = DateTime.Now;
                // بررسی معتبر بودن CategoryId ارسالی


                // بررسی وجود آیتم بر اساس عنوان (می‌تونه شرط دیگری هم باشه مثلاً ID)
                TblItem? existingItem = null;
                if (model.OldId != null)
                {
                    existingItem = await _db.TblItem.Include(i => i.TblCompanies).FirstOrDefaultAsync(i => i.ID == model.OldId && i.RemoveDate == null);
                    if (existingItem != null)
                    {
                        if (model.IsDelete)
                        {
                            existingItem.RemoveDate = Now;
                            existingItem.ModifyDate = Now;
                            _db.TblItem.Update(existingItem);
                            await _db.SaveChangesAsync();
                            return new ResultModel(true, "آیتم با موفقیت حذف شد.");
                        }
                        else
                        {
                            var categoryExists = await _db.TblCategory
               .AnyAsync(c => c.ID == model.TblCategoryId && c.RemoveDate == null);

                            if (!categoryExists)
                                return new ResultModel(false, "دسته‌بندی انتخاب‌شده وجود ندارد یا حذف شده است.");
                            // ویرایش آیتم موجود
                            existingItem.ItemName = model.ItemName;
                            existingItem.RepeatTimeMinute = model.RepeatTimeMinute ?? 10;
                            existingItem.AdditionalUrlAddress = model.AdditionalUrlAddress.Trim();
                            existingItem.ImportanceLevel = model.ImportanceLevel ?? __ImportanceLevel.Medium;
                            existingItem.TblCategoryId = model.TblCategoryId ?? 1;
                            existingItem.ModifyDate = Now;


                            // به‌روزرسانی شرکت‌ها (در صورت وجود)
                            if (model.CompanyIds.Any())
                            {
                                var companies = await _db.TblCompany
                                    .Where(c => model.CompanyIds.Contains(c.ID.ToString()) && c.RemoveDate == null)
                                    .ToListAsync();

                                existingItem.TblCompanies.Clear();
                                foreach (var company in companies)
                                {
                                    existingItem.TblCompanies.Add(company);
                                }
                            }

                            _db.TblItem.Update(existingItem);
                            await _db.SaveChangesAsync();
                            return new ResultModel(true, "آیتم با موفقیت ویرایش شد.");
                        }
                    }
                    else
                        return new ResultModel(false, "آیتم یافت نرفت.");
                }
                else
                {
                    var categoryExists = await _db.TblCategory
                    .AnyAsync(c => c.ID == model.TblCategoryId && c.RemoveDate == null);

                    if (!categoryExists)
                        return new ResultModel(false, "دسته‌بندی انتخاب‌شده وجود ندارد یا حذف شده است.");
                    // ایجاد آیتم جدید
                    var newItem = new TblItem
                    {
                        ItemName = model.ItemName,
                        RepeatTimeMinute = model.RepeatTimeMinute ?? 10,
                        AdditionalUrlAddress = model.AdditionalUrlAddress.Trim(),
                        ImportanceLevel = model.ImportanceLevel ?? __ImportanceLevel.Medium,
                        TblCategoryId = model.TblCategoryId ?? 1,
                        CreateDate = Now,
                        ModifyDate = Now,
                        RemoveDate = null,
                        TblCompanies = new List<TblCompany>()
                    };

                    if (model.CompanyIds.Any())
                    {
                        var companies = await _db.TblCompany
                            .Where(c => model.CompanyIds.Contains(c.ID.ToString()) && c.RemoveDate == null)
                            .ToListAsync();

                        foreach (var company in companies)
                        {
                            newItem.TblCompanies.Add(company);
                        }
                    }

                    await _db.TblItem.AddAsync(newItem);
                    await _db.SaveChangesAsync();
                    return new ResultModel(true, "آیتم با موفقیت ایجاد شد.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد یا ویرایش آیتم.");
                return new ResultModel(false, "خطایی در ایجاد یا ویرایش آیتم رخ داد.");
            }
        }

        public async Task<List<ItemViewModel>> GetItemList(bool WithRemoved = false)
        {
            var result = await _db.TblItem
                .Include(i => i.TblCategoryNavigation)
                .OrderByDescending(x => x.CreateDate).Select(item => new ItemViewModel
                {
                    ID = item.ID,
                    ItemName = item.ItemName,
                    RepeatTimeMinute = item.RepeatTimeMinute,
                    AdditionalUrlAddress = item.AdditionalUrlAddress,
                    ImportanceLevel = item.ImportanceLevel,
                    CreateDate = item.CreateDate,
                    ModifyDate = item.ModifyDate,
                    TblCategoryId = item.TblCategoryId,
                    CategoryTitle = item.TblCategoryNavigation.CategoryName, // assuming Title exists
                    CompanyIds = item.TblCompanies.Select(z => z.ID.ToString()).ToList() ?? new List<string>(),
                    companies = item.TblCompanies.Select(z=> z.CompanyName).ToList()?? new List<string>(),
                    Removed = item.RemoveDate != null ? true : false    
                })
                .ToListAsync();

            if (!WithRemoved)
            {
                return result.Where(Z => Z.Removed == false).ToList() ;
            }

            return result;
        }
        public async Task<List<ComboItem>> GetItemListCombo()
        {
            var result = await _db.TblItem.Where(z=>z.RemoveDate == null)
                .Select(item => new ComboItem
                {
                    value = item.ID.ToString(),
                    label = item.ItemName,
                })
                .ToListAsync();

            return result;
        }
        public async Task<List<ComboItem>> GetTblStatusCombo()
        {
            var Componies = await _db.TblStatus.Where(z => z.RemoveDate == null).Select(z => new ComboItem { label = z.Name, value = z.ID.ToString() }).ToListAsync();
            return Componies;
        }
        public async Task<ResultModel> DeactiveItem(string ItemId)
        {
            var item = await _db.TblItem
                .FirstOrDefaultAsync(x => x.ID == ItemId.DecryptToInt() && x.RemoveDate == null);

            if (item == null)
                return new ResultModel(false, "آیتم مورد نظر یافت نشد.");

            item.RemoveDate = DateTime.Now;

            _db.TblItem.Update(item);
            await _db.SaveChangesAsync();

            return new ResultModel(true, "آیتم با موفقیت غیرفعال شد.");
        }
        public async Task<ResultModel> ActiveItem(string ItemId)
        {
            var item = await _db.TblItem
                .FirstOrDefaultAsync(x => x.ID == ItemId.DecryptToInt() && x.RemoveDate != null);

            if (item == null)
                return new ResultModel(false, "آیتم مورد نظر یافت نشد.");

            item.RemoveDate = null;

            _db.TblItem.Update(item);
            await _db.SaveChangesAsync();

            return new ResultModel(true, "آیتم با موفقیت فعال شد.");
        }
        /// <summary>
        /// نسبت دادن آیتم به شرکت ها
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="companyIds"></param>
        /// <returns></returns>
        public async Task<ResultModel> AssignCompaniesToItem(int itemId, List<int> companyIds)
        {
            try
            {
                var item = await _db.TblItem.Include(x => x.TblCompanies)
                    .FirstOrDefaultAsync(x => x.ID == itemId && x.RemoveDate == null);
                if (item == null)
                    return new ResultModel(false, "آیتم یافت نشد.");

                var companies = await _db.TblCompany
                    .Where(c => companyIds.Contains(c.ID) && c.RemoveDate == null)
                    .ToListAsync();

                foreach (var old in item.TblCompanies.ToList())
                {
                    if (!companyIds.Contains(old.ID))
                    {
                        item.TblCompanies.Remove(old);
                    }
                }
                foreach (var company in companies)
                {
                    if (!item.TblCompanies.Any(c => c.ID == company.ID))
                    {
                        item.TblCompanies.Add(company);
                    }
                }

                _db.TblItem.Update(item);
                await _db.SaveChangesAsync();

                return new ResultModel(true, "ارتباط شرکت‌ها با آیتم به‌روز شد.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در به‌روزرسانی شرکت‌ها برای آیتم.");
                return new ResultModel(false, "خطایی در به‌روزرسانی ارتباط شرکت‌ها رخ داد.");
            }
        }
    }
}
