namespace Monitoring_Support_Server.Models.ViewModels
{
    public class CompanyModel
    {
        public int? OldId { get; set; }
        public bool IsEdit { get; set; } = false;
        public bool IsDelete { get; set; } = false;
        /// <summary>
        /// نام شرکت
        /// این فیلد ضروری است و طول آن حداکثر 100 کاراکتر می‌تواند باشد.
        /// </summary>
        public string? CompanyName { get; set; }

        /// <summary>
        /// آدرس اصلی شرکت
        /// این فیلد حداکثر 255 کاراکتر طول می‌تواند داشته باشد.
        /// </summary>
        public string? BaseUrlAddress { get; set; }

        /// <summary>
        /// آدرس شرکت
        /// </summary>
        public string? LocationAddress { get; set; }

        /// <summary>
        /// شناسه ملی شرکت
        /// این فیلد حداکثر 20 کاراکتر طول می‌تواند داشته باشد.
        /// </summary>
        public string? NationalCode { get; set; }

        /// <summary>
        /// نام کاربری در سامانه مشترکین
        /// </summary>
        public string? ApiUser { get; set; }

        /// <summary>
        /// پسورد در سامانه مشترکین
        /// </summary>
        public string? ApiPassword { get; set; }
    }
}
