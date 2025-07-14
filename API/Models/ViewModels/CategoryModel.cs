using System.ComponentModel.DataAnnotations;

namespace Monitoring_Support_Server.Models.ViewModels
{
    public class CategoryModel
    {
        /// <summary>
        /// نام دسته‌بندی
        /// این فیلد ضروری است و طول آن حداکثر 100 کاراکتر می‌تواند باشد.
        /// </summary>
        [Required, StringLength(100)]
        public string CategoryName { get; set; }
    }
}
