using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Monitoring.Support.Server.Models.DatabaseModels
{
    /// <summary>
    /// کلاس مدل برای جدول دسته‌بندی‌ها (TblCategory)
    /// این کلاس اطلاعات دسته‌بندی‌ها را ذخیره می‌کند.
    /// </summary>
    public class TblCategory
    {
        public TblCategory()
        {
            TblItems = new HashSet<TblItem>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// نام دسته‌بندی
        /// این فیلد ضروری است و طول آن حداکثر 100 کاراکتر می‌تواند باشد.
        /// </summary>
        [Required, StringLength(100)]
        public string CategoryName { get; set; }

        /// <summary>
        /// تاریخ ایجاد دسته‌بندی
        /// تاریخ و زمان ایجاد رکورد را ذخیره می‌کند.
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// تاریخ ویرایش شرکت
        /// </summary>
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// تاریخ حذف دسته‌بندی (اختیاری)
        /// این فیلد تاریخ و زمان حذف رکورد را ذخیره می‌کند، در صورت حذف.
        /// </summary>
        public DateTime? RemoveDate { get; set; }

        [InverseProperty("TblCategoryNavigation")]
        public virtual ICollection<TblItem> TblItems { get; set; }
    }
}
