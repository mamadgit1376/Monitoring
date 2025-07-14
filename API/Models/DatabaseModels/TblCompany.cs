using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Monitoring.Support.Server.Models.DatabaseModels
{
    /// <summary>
    /// کلاس مدل برای جدول شرکت‌ها (TblCompany)
    /// این کلاس اطلاعات مربوط به شرکت‌ها را ذخیره می‌کند.
    /// </summary>
    public class TblCompany
    {
        public TblCompany()
        {
            TblItems = new HashSet<TblItem>();
            TblItemLogs = new HashSet<TblItemLog>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// نام شرکت
        /// این فیلد ضروری است و طول آن حداکثر 100 کاراکتر می‌تواند باشد.
        /// </summary>
        [Required, StringLength(100)]
        public string CompanyName { get; set; }

        [Required]
        public string ApiUser {  get; set; }

        [Required]
        public string ApiPassword { get; set; }

        /// <summary>
        /// آدرس اصلی شرکت
        /// این فیلد حداکثر 255 کاراکتر طول می‌تواند داشته باشد.
        /// </summary>
        [Required, StringLength(255)]
        public string BaseUrlAddress { get; set; }

        /// <summary>
        /// آدرس شرکت
        /// </summary>
        [Required]
        public string LocationAddress { get; set; }

        /// <summary>
        /// شناسه ملی شرکت
        /// این فیلد حداکثر 20 کاراکتر طول می‌تواند داشته باشد.
        /// </summary>
        [Required, StringLength(20)]
        public string NationalCode { get; set; }

        /// <summary>
        /// تاریخ ایجاد شرکت
        /// تاریخ و زمان ایجاد رکورد را ذخیره می‌کند.
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// تاریخ ویرایش شرکت
        /// </summary>
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// تاریخ حذف شرکت (اختیاری)
        /// این فیلد تاریخ و زمان حذف رکورد را ذخیره می‌کند، در صورتی که حذف شده باشد.
        /// </summary>
        public DateTime? RemoveDate { get; set; }

        public ICollection<TblItem> TblItems { get; set; }
        public ICollection<TblItemLog> TblItemLogs { get; set; }
    }
}
