using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Monitoring.Support.Server.Models.DatabaseModels
{
    /// <summary>
    /// کلاس مدل برای جدول وضعیت‌ها (TblStatus)
    /// این کلاس اطلاعات مربوط به وضعیت‌های مختلف را ذخیره می‌کند.
    /// </summary>
    public class TblStatus: StatusItem
    {
        public TblStatus()
        {
            
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        

        /// <summary>
        /// تاریخ ایجاد وضعیت
        /// تاریخ و زمان ایجاد رکورد را ذخیره می‌کند.
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// تاریخ حذف وضعیت (اختیاری)
        /// این فیلد تاریخ و زمان حذف رکورد را ذخیره می‌کند، در صورتی که حذف شده باشد.
        /// </summary>
        public DateTime? RemoveDate { get; set; }

        /// <summary>
        /// مجموعه‌ای از لاگ‌های وضعیت (TblItemLog)
        /// این رابطه تمام لاگ‌هایی که به این وضعیت مرتبط هستند را ذخیره می‌کند.
        /// </summary>
        public ICollection<TblItemLog> TblItemLogs { get; set; }
    }

    /// <summary>
    /// نوع وضعیت
    /// این Enum وضعیت‌های ممکن برای یک وضعیت را نمایش می‌دهد.
    /// </summary>
    public enum __StatusType : short
    {
        Success = 1, // وضعیت موفقیت
        Error = 0,    // وضعیت خطا
        Warning = 2    // وضعیت هشدار
    }
    public class StatusItem
    {
        /// <summary>
        /// نام وضعیت
        /// این فیلد ضروری است و طول آن حداکثر 100 کاراکتر می‌تواند باشد.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// نام فارسی وضعیت
        /// این فیلد ضروری است و طول آن حداکثر 100 کاراکتر می‌تواند باشد.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string PersianName { get; set; }

        /// <summary>
        /// نوع وضعیت
        /// این فیلد نشان‌دهنده نوع وضعیت است که می‌تواند موفقیت یا خطا باشد.
        /// 0: موفقیت، 1: خطا
        /// </summary>
        public __StatusType StatusType { get; set; }

        /// <summary>
        /// توضیحات وضعیت
        /// این فیلد توضیحات بیشتری در مورد وضعیت را ذخیره می‌کند و حداکثر طول آن 500 کاراکتر است.
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }
    }
}
