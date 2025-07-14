using AbfaCS.Common.Encryptor;
using Monitoring.Support.Server.Models.DatabaseModels;
using Monitoring.Support.Services;
using Monitoring_Support_Server.Models.ViewModels;
using Monitoring_Support_Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Monitoring_Support_Server.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CompanyService> _logger;
        private readonly MonitoringDbContext _db;

        public CompanyService(ILogger<CompanyService> logger, IMemoryCache memoryCache, IConfiguration configuration, MonitoringDbContext db)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _configuration = configuration;
            _db = db;
        }

        public async Task<ResultModel> AddOrEditCompanyFields(CompanyModel model)
        {
            try
            {
                var Now = DateTime.Now;
                if (model.IsEdit || model.IsDelete)
                {
                    if (model.OldId == null)
                    {
                        return new ResultModel(false, " آیدی شرکت خالی است");
                    }
                    var existingCompany = await _db.TblCompany.FirstOrDefaultAsync(c => c.ID == model.OldId);
                    if (existingCompany != null)
                    {
                        if (model.IsEdit)
                        {
                            existingCompany.CompanyName = model.CompanyName;
                            existingCompany.BaseUrlAddress = model.BaseUrlAddress.Trim();
                            existingCompany.LocationAddress = model.LocationAddress;
                            existingCompany.NationalCode = model.NationalCode;
                            existingCompany.ApiUser = model.ApiUser.Trim();
                            existingCompany.ApiPassword = model.ApiPassword.Trim();
                        }
                        else
                        {
                            existingCompany.RemoveDate = DateTime.Now;
                        }
                        existingCompany.ModifyDate = Now;
                        _db.TblCompany.Update(existingCompany);
                        await _db.SaveChangesAsync();
                        return new ResultModel(true, " تغییرات با موفقیت انجام شد");
                    }
                    else
                    {
                        return new ResultModel(false, "شرکت مورد نظر یافت نشد");
                    }
                }
                else
                {
                    if (model.CompanyName == null || model.NationalCode == null || model.BaseUrlAddress == null || model.LocationAddress == null || model.ApiUser == null || model.ApiPassword == null)
                    {
                        return new ResultModel(false, "لطفا تمام فیلدها را پر کنید");
                    }
                    var newCompany = new TblCompany
                    {
                        CompanyName = model.CompanyName,
                        NationalCode = model.NationalCode,
                        CreateDate = Now,
                        BaseUrlAddress = model.BaseUrlAddress.Trim(),
                        LocationAddress = model.LocationAddress.Trim(),
                        ApiUser = model.ApiUser.Trim(),
                        ApiPassword = model.ApiPassword.Trim(),
                        ModifyDate = Now,
                        RemoveDate = null,
                    };

                    await _db.TblCompany.AddAsync(newCompany);
                    await _db.SaveChangesAsync();

                    return new ResultModel(true, "شرکت با موفقیت ثبت شد");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت اطلاعات شرکت رخ داد");
                return new ResultModel(false, "خطایی در ثبت اطلاعات شرکت رخ داد");
            }
        }

        public async Task<List<TblCompany>> GetCompanyList()
        {
            var result = await _db.TblCompany
               .Where(x => x.RemoveDate == null)
               .OrderByDescending(x => x.CreateDate)
               .ToListAsync();
            return result;
        }
        public async Task<bool> IsValidCompany(int CompanyId)
        {
            return await _db.TblCompany.Where(z => z.RemoveDate == null && z.ID == CompanyId).AnyAsync();
        }

        public async Task<ResultModel> DeactiveCompany(string CompanyId)
        {
            var company = await _db.TblCompany
                .FirstOrDefaultAsync(x => x.ID == CompanyId.DecryptToInt() && x.RemoveDate == null);

            if (company == null)
            {
                return new ResultModel(false, "شرکت مورد نظر یافت نشد");
            }

            company.RemoveDate = DateTime.Now;

            _db.TblCompany.Update(company);
            await _db.SaveChangesAsync();

            return new ResultModel(true, "شرکت با موفقیت غیر فعال شد");
        }

        public async Task<ResultModel> ActiveCompany(string CompanyId)
        {
            var company = await _db.TblCompany
                .FirstOrDefaultAsync(x => x.ID == CompanyId.DecryptToInt() && x.RemoveDate != null);

            if (company == null)
            {
                return new ResultModel(false, "شرکت مورد نظر یافت نشد");
            }

            company.RemoveDate = null;

            _db.TblCompany.Update(company);
            await _db.SaveChangesAsync();

            return new ResultModel(true, "شرکت با موفقیت فعال شد");
        }

        public async Task<List<ComboItem>> GetCompanyComboBox()
        {
            var Componies = await _db.TblCompany.Where(z => z.RemoveDate == null)
                .Select(z => new ComboItem 
                { 
                    label = z.CompanyName, 
                    value = z.ID.ToString(), 
                    BaseUrl = z.BaseUrlAddress 
                }).ToListAsync();

            return Componies;
        }
    }
}
