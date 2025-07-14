using Monitoring.Support.Server.Models.DatabaseModels;

namespace Monitoring_Support_Server.Models.ViewModels
{
    public class ItemModel
    {
        public int? OldId { get; set; }
        public bool IsDelete { get; set; } = false;
        /// <summary>
        /// عنوان آیتم
        /// این فیلد ضروری است و طول آن حداکثر 200 کاراکتر می‌تواند باشد.
        /// </summary>
        public string? ItemName { get; set; }

        /// <summary>
        /// زمان تکرار به دقیقه
        /// این فیلد نشان‌دهنده مدت زمان تکرار برای هر آیتم است و به طور پیش‌فرض برابر 10 دقیقه است.
        /// </summary>
        public int? RepeatTimeMinute { get; set; }

        /// <summary>
        /// آدرس اضافی آیتم
        /// این فیلد حداکثر 255 کاراکتر طول می‌تواند داشته باشد.
        /// </summary>
        public string? AdditionalUrlAddress { get; set; }

        /// <summary>
        /// سطح اهمیت آیتم
        /// </summary>
        public __ImportanceLevel? ImportanceLevel { get; set; }

        /// <summary>
        /// شناسه دسته‌بندی مرتبط با آیتم
        /// این فیلد به شناسه جدول دسته‌بندی (TblCategory) اشاره دارد.
        /// </summary>
        public int? TblCategoryId { get; set; }

        /// <summary>
        /// لیستی از شرکت ها
        /// </summary>
        public List<string>? CompanyIds { get; set; } = new List<string>();
    }


    public class ItemViewModel
    {
        public int ID { get; set; }

        public string ItemName { get; set; }

        public int RepeatTimeMinute { get; set; }

        public string AdditionalUrlAddress { get; set; }

        public __ImportanceLevel ImportanceLevel { get; set; }

        public string ImportanceLevelText => ImportanceLevel.ToString();

        public DateTime CreateDate { get; set; }

        public DateTime? ModifyDate { get; set; }

        public int TblCategoryId { get; set; }

        public string CategoryTitle { get; set; }

        public List<string> CompanyIds { get; set; } = new List<string>();
        public List<string> companies { get; set; } = new List<string>();
        public bool Removed { get; set; } 
    }
}
