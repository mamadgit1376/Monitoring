using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Monitoring.Support.Server.Models.DatabaseModels
{
    /// <summary>
    /// کلاس مدل برای جدول آیتم ها (TblItem)
    /// این کلاس اطلاعات مربوط به آیتم ها را ذخیره می‌کند.
    /// </summary>
    public class TblItem
    {
        public TblItem()
        {
            TblCompanies = new HashSet<TblCompany>();
            TblItemLogs = new HashSet<TblItemLog>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        /// <summary>
        /// عنوان آیتم
        /// این فیلد ضروری است و طول آن حداکثر 200 کاراکتر می‌تواند باشد.
        /// </summary>
        [Required(ErrorMessage = "عنوان اجباری است"), StringLength(200)]
        public string ItemName { get; set; }
        /// <summary>
        /// زمان تکرار به دقیقه
        /// این فیلد نشان‌دهنده مدت زمان تکرار برای هر آیتم است و به طور پیش‌فرض برابر 10 دقیقه است.
        /// </summary>
        public int RepeatTimeMinute { get; set; } = 10;
        /// <summary>
        /// آدرس اضافی آیتم
        /// این فیلد حداکثر 255 کاراکتر طول می‌تواند داشته باشد.
        /// </summary>
        [StringLength(255)]
        public string AdditionalUrlAddress { get; set; }
        /// <summary>
        /// سطح اهمیت آیتم
        /// </summary>
        [Required]
        public __ImportanceLevel ImportanceLevel { get; set; }
        /// <summary>
        /// تاریخ ایجاد آیتم
        /// تاریخ و زمان ایجاد رکورد را ذخیره می‌کند.
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// تاریخ ویرایش آیتم
        /// </summary>
        public DateTime? ModifyDate { get; set; }
        /// <summary>
        /// تاریخ حذف آیتم (اختیاری)
        /// این فیلد تاریخ و زمان حذف رکورد را ذخیره می‌کند، در صورتی که حذف شده باشد.
        /// </summary>
        public DateTime? RemoveDate { get; set; }
        /// <summary>
        /// شناسه دسته‌بندی مرتبط با آیتم
        /// این فیلد به شناسه جدول دسته‌بندی (TblCategory) اشاره دارد.
        /// </summary>
        [Required(ErrorMessage = "انتخاب دسته‌بندی اجباری است")]
        public int TblCategoryId { get; set; }
        /// <summary>
        /// رابطه به دسته‌بندی مربوطه (TblCategory)
        /// این ویژگی نمایانگر رابطه بین جدول آیتم و جدول دسته‌بندی است.
        /// </summary>
        [ForeignKey("TblCategoryId")]
        public TblCategory TblCategoryNavigation { get; set; }

        public ICollection<TblCompany> TblCompanies { get; set; }
        public ICollection<TblItemLog> TblItemLogs { get; set; }
    }

    public enum __ImportanceLevel
    {
        /// <summary>
        /// سطح اهمیت کم
        /// </summary>
        Low = 0,
        /// <summary>
        /// سطح اهمیت متوسط
        /// </summary>
        Medium = 1,
        /// <summary>
        /// سطح اهمیت زیاد
        /// </summary>
        High = 2
    }
}