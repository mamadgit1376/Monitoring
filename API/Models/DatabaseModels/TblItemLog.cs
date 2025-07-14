using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Monitoring.Support.Server.Models.DatabaseModels
{
    /// <summary>
    /// کلاس مدل برای جدول لاگ‌های اقلام (TblItemLog)
    /// این کلاس اطلاعات مربوط به لاگ‌های تغییرات اقلام و وضعیت آن‌ها را ذخیره می‌کند.
    /// </summary>
    public class TblItemLog
    {
        public TblItemLog()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// شناسه سازنده لاگ
        /// این فیلد به شناسه جدول کاربران (TblUsers) اشاره دارد.
        /// </summary>
        public int CreatorId { get; set; }

        /// <summary>
        /// شناسه قلم مرتبط با این لاگ
        /// این فیلد به شناسه جدول اقلام (TblItem) اشاره دارد.
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// شناسه شرکت مرتبط با این لاگ
        /// این فیلد به شناسه جدول شرکت‌ها (TblCompany) اشاره دارد.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// تاریخ ایجاد لاگ
        /// تاریخ و زمان ایجاد رکورد را ذخیره می‌کند.
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// تاریخ حذف لاگ (اختیاری)
        /// این فیلد تاریخ و زمان حذف رکورد را ذخیره می‌کند، در صورتی که حذف شده باشد.
        /// </summary>
        public DateTime? RemoveDate { get; set; }

        /// <summary>
        /// وضعیت پاسخ HTTP
        /// این فیلد نشان‌دهنده وضعیت کد HTTP مربوط به پاسخ است.
        /// </summary>
        public __HttpStatus ResponseHtmlStatus { get; set; }

        /// <summary>
        /// کد وضعیت
        /// این فیلد جواب کد وضعیت مربوط به لاگ را ذخیره می‌کند.
        /// </summary>
        public int? TblStatusCode { get; set; }

        /// <summary>
        /// آدرس URL کامل مرتبط با لاگ
        /// این فیلد حداکثر 500 کاراکتر طول می‌تواند داشته باشد.
        /// </summary>
        [StringLength(500)]
        public string FullUrl { get; set; }

        public bool IsNotified { get; set; } = false;
        // Navigation properties

        /// <summary>
        /// رابطه به قلم مربوطه (TblItem)
        /// این ویژگی نمایانگر رابطه بین لاگ و قلم مربوطه است.
        /// </summary>
        [ForeignKey("ItemId")]
        public TblItem TblItemNavigation { get; set; }

        /// <summary>
        /// رابطه به شرکت مربوطه (TblCompany)
        /// این ویژگی نمایانگر رابطه بین لاگ و شرکت مربوطه است.
        /// </summary>
        [ForeignKey("CompanyId")]
        public TblCompany TblCompanyNavigation { get; set; }

        /// <summary>
        /// رابطه به وضعیت مربوطه (TblStatus)
        /// این ویژگی نمایانگر رابطه بین لاگ و وضعیت مربوطه است.
        /// </summary>
        [ForeignKey("TblStatusCode")]
        public TblStatus TblStatusCodeNavigation { get; set; }


    }

    /// <summary>
    /// نوع وضعیت پاسخ HTTP
    /// این Enum انواع کدهای وضعیت HTTP را نمایش می‌دهد.
    /// </summary>
    public enum __HttpStatus : short
    {
        OK = 200,
        Created = 201,
        Accepted = 202,
        NoContent = 204,
        MovedPermanently = 301,
        Found = 302,
        NotModified = 304,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        NotAcceptable = 406,
        ProxyAuthenticationRequired = 407,
        Conflict = 409,
        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502,
        ServiceUnavailable = 503
    }

    public class ItemLogViewModel
    {
        public int ID { get; set; }
        public int CompanyId { get; set; }

        public int CreatorId { get; set; }

        public string ItemName { get; set; }

        public string StatusName { get; set; }
        public string StatusDescription { get; set; }
        public short StatusType { get; set; }
        public string CompanyName { get; set; }

        public string CreateDate { get; set; }
        public __HttpStatus HttpStatus { get; set; }
        public string ResponseHtmlStatus { get; set; }

        public string FullUrl { get; set; }
    }
}
