using Pack.Common.Convertor;
using Pack.Common.Public;
using Monitoring_Support_Server.Services.Interfaces;

public class BackGroundJobService : IHostedService
{
    private readonly ILogger<BackGroundJobService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public BackGroundJobService(IServiceScopeFactory serviceScopeFactory, ILogger<BackGroundJobService> logger)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Background job service is starting.");
        Task.Run(() => ExecuteAsync(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    protected async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background job service is running.");
        var methodInformation = new MethodInformation("JobService - اجرای دوره‌ای بررسی وضعیت آیتم‌ها برای شرکت‌های مرتبط");
        using var scope = _serviceScopeFactory.CreateScope(); // ایجاد یک اسکوپ جدید برای دسترسی به سرویس‌ها

        var _monitorService = scope.ServiceProvider.GetRequiredService<IMonitorService>(); // سرویس مانیتور
        var _itemService = scope.ServiceProvider.GetRequiredService<IItemService>(); // سرویس آیتم
        var _companyService = scope.ServiceProvider.GetRequiredService<ICompanyService>(); // سرویس شرکت‌ها
        var _BaleWebService = scope.ServiceProvider.GetRequiredService<IBaleWebService>(); // سرویس پیام رسانی
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // تاخیر 30 دقیقه‌ای بین هر بار اجرای سرویس
                await _semaphore.WaitAsync(stoppingToken); // قفل برای جلوگیری از تداخل در اجرای همزمان

                var itemList = await _itemService.GetItemList(false); // دریافت لیست آیتم‌ها
                if (itemList.Any())
                {
                    foreach (var item in itemList) // بررسی آیتم به جهت اینکه کدام شرکت ها براشون لاگ در سرور ثبت شده
                    {
                        var CheckTime = DateTime.Now.AddMinutes(-1 * item.RepeatTimeMinute); // زمان بررسی وضعیت آیتم
                        var ItemLogs = await _monitorService.GetItemLogForBGService(item.CompanyIds, item.ID, CheckTime); // دریافت لاگ‌های آیتم شرکت ها برای بررسی وضعیت
                        var loggedcodes = ItemLogs.Select(_ => _.CompanyId.ToString()).Distinct().ToList(); // استخراج آیدی شرکت‌هایی که لاگ دارند

                        var UncheckedCompanyCodes = item.CompanyIds.Except(loggedcodes).ToList(); // شرکت‌هایی که هنوز لاگ ندارند
                        if (UncheckedCompanyCodes.Any())
                        {
                            foreach (var code in UncheckedCompanyCodes) // به ازای هر شرکت یک درخواست به سرور آن شرکت ارسال میشود تا لاگ وضعیت ذخیره و مشخص شود
                            {
                                var companyId = code.ToIntFromString().GetValueOrDefault();
                                await _monitorService.CallStatus(methodInformation.ExtraInfo, companyId, item.ID, 1); // ذخیره وضعیت آیتم در دیتابیس
                                _logger.LogInformation($"وضعیت آیتم با شناسه {item.ID} برای شرکت {code} با موفقیت بررسی و ثبت شد.");
                            }
                        }
                    }
                }

                //await _BaleWebService.SendErrorItemsAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در اجرای job service.");
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Background job service is stopping.");
        return Task.CompletedTask;
    }
}
