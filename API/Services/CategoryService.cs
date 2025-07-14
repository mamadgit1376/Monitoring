using Pack.Common.Encryptor;
using Monitoring.Support.Server.Models.DatabaseModels;
using Monitoring.Support.Services;
using Monitoring_Support_Server.Models.ViewModels;
using Monitoring_Support_Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Monitoring_Support_Server.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CategoryService> _logger;
        private readonly MonitoringDbContext _db;

        public CategoryService(ILogger<CategoryService> logger, IMemoryCache memoryCache, IConfiguration configuration, MonitoringDbContext db)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _configuration = configuration;
            _db = db;
        }

        public async Task<ResultModel> AddOrEditCategoryFields(CategoryModel model)
        {
            try
            {
                var existingCategory = await _db.TblCategory
                    .FirstOrDefaultAsync(c => c.CategoryName == model.CategoryName && c.RemoveDate == null);

                if (existingCategory != null)
                {
                    existingCategory.CategoryName = model.CategoryName;
                    existingCategory.ModifyDate = DateTime.Now;

                    _db.TblCategory.Update(existingCategory);
                    await _db.SaveChangesAsync();

                    return new ResultModel(true, "ویرایش دسته‌بندی با موفقیت انجام شد");
                }
                else
                {
                    var nameExists = await _db.TblCategory.AnyAsync(c => c.CategoryName == model.CategoryName && c.RemoveDate == null);
                    if (nameExists)
                        return new ResultModel(false, "دسته‌بندی با این نام قبلاً ثبت شده است.");
                    else
                    {
                        var newCategory = new TblCategory
                        {
                            CategoryName = model.CategoryName,
                            CreateDate = DateTime.Now,
                            ModifyDate = null,
                            RemoveDate = null
                        };

                        await _db.TblCategory.AddAsync(newCategory);
                        await _db.SaveChangesAsync();

                        return new ResultModel(true, "دسته‌بندی با موفقیت ثبت شد.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت اطلاعات دسته‌بندی رخ داد");
                return new ResultModel(false, "خطایی در ثبت اطلاعات دسته‌بندی رخ داد.");
            }
        }

        public async Task<List<TblCategory>> GetCategoryList()
        {
            var result = await _db.TblCategory
                .Where(x => x.RemoveDate == null)
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync();

            return result;
        }

        public async Task<ResultModel> DeactiveCategory(string CategoryId)
        {
            var category = await _db.TblCategory
                .FirstOrDefaultAsync(x => x.ID == CategoryId.DecryptToInt() && x.RemoveDate == null);

            if (category == null)
                return new ResultModel(false, "دسته‌بندی مورد نظر یافت نشد.");

            category.RemoveDate = DateTime.Now;

            _db.TblCategory.Update(category);
            await _db.SaveChangesAsync();

            return new ResultModel(true, "دسته‌بندی با موفقیت غیرفعال شد.");
        }

        public async Task<ResultModel> ActiveCategory(string CategoryId)
        {
            var category = await _db.TblCategory
                .FirstOrDefaultAsync(x => x.ID == CategoryId.DecryptToInt() && x.RemoveDate != null);

            if (category == null)
                return new ResultModel(false, "دسته‌بندی مورد نظر یافت نشد.");

            category.RemoveDate = null;

            _db.TblCategory.Update(category);
            await _db.SaveChangesAsync();

            return new ResultModel(true, "دسته‌بندی با موفقیت فعال شد.");
        }
    }
}
