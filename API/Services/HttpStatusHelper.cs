using Monitoring.Support.Server.Models.DatabaseModels;

namespace Monitoring_Support_Server.Services
{
    public static class HttpStatusHelper
    {
        public static readonly Dictionary<__HttpStatus, string> HttpStatusDictionary = new Dictionary<__HttpStatus, string>
        {
            { __HttpStatus.OK, "200: موفق" },
            { __HttpStatus.Created, "201: ایجاد شد" },
            { __HttpStatus.Accepted, "202: پذیرفته شد" },
            { __HttpStatus.NoContent, "204: بدون محتوا" },
            { __HttpStatus.MovedPermanently, "301: منتقل شده دائمی" },
            { __HttpStatus.Found, "302: یافت شد" },
            { __HttpStatus.NotModified, "304: تغییری نکرده" },
            { __HttpStatus.BadRequest, "400: درخواست نامعتبر" },
            { __HttpStatus.Unauthorized, "401: غیرمجاز" },
            { __HttpStatus.Forbidden, "403: ممنوع" },
            { __HttpStatus.NotFound, "404: یافت نشد" },
            { __HttpStatus.MethodNotAllowed, "405: روش مجاز نیست" },
            {__HttpStatus.NotAcceptable , "406 : مجاز نیست" },
            {__HttpStatus.ProxyAuthenticationRequired , "407 : آیپی تراست نیست" },
            { __HttpStatus.Conflict, "409: تداخل" },
            { __HttpStatus.InternalServerError, "500: خطای داخلی سرور" },
            { __HttpStatus.NotImplemented, "501: پیاده‌سازی نشده" },
            { __HttpStatus.BadGateway, "502: گیت‌وی نامعتبر" },
            { __HttpStatus.ServiceUnavailable, "503: سرویس در دسترس نیست" }
        };
    }
}