using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Linq;

namespace Monitoring_Support_Server.MiddleWare
{
    public class BodyHashValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public BodyHashValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;

            // فقط روی متدهای POST, PUT, PATCH و با Content-Type مناسب اجرا شود
            if ((request.Method == HttpMethods.Post) && request.ContentType?.StartsWith("application/json") == true && (!request.Path.StartsWithSegments("/User/RefreshToken") && !request.Path.StartsWithSegments("/User/Login") && !request.Path.StartsWithSegments("/User/TestBale")))
            {
                // این دستور به ما اجازه می‌دهد استریم بدنه درخواست را چند بار بخوانیم
                request.EnableBuffering();

                // خواندن بدنه درخواست به صورت رشته با پشتیبانی از UTF-8 برای رشته‌های فارسی
                string bodyAsString;
                using (var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
                {
                    bodyAsString = await reader.ReadToEndAsync();
                }

                // برگرداندن استریم به ابتدای آن تا بقیه pipeline بتوانند آن را بخوانند
                request.Body.Position = 0;

                if (string.IsNullOrWhiteSpace(bodyAsString))
                {
                    // اگر بدنه خالی است، کاری انجام نده و به مرحله بعد برو
                    await _next(context);
                    return;
                }

                try
                {
                    // استفاده از JsonNode برای کار با JSON به صورت داینامیک
                    var jsonNode = JsonNode.Parse(bodyAsString);
                    if (jsonNode is not JsonObject jsonObject)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync("Invalid JSON object.");
                        return;
                    }

                    // استخراج و حذف فیلد hashbody
                    if (!jsonObject.TryGetPropertyValue("hashbody", out var hashNode) || hashNode is null)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync("Missing 'hashbody' field.");
                        return;
                    }

                    var receivedHash = hashNode.GetValue<string>();
                    jsonObject.Remove("hashbody");

                    // مرتب‌سازی فیلدهای JSON قبل از هش کردن
                    var sortedJson = SortJsonObject(jsonObject);

                    // تبدیل آبجکت مرتب‌شده به رشته JSON با استفاده از Unicode escape sequences
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default // استفاده از Default encoder برای تولید Unicode escape sequences
                    };
                    var payloadToHash = sortedJson.ToJsonString(options);

                    // ساخت هش از بدنه مرتب‌شده
                    var calculatedHash = ComputeSha256Hash(payloadToHash);

                    // مقایسه هش‌ها (به صورت غیر حساس به بزرگی و کوچکی حروف)
                    if (!string.Equals(receivedHash, calculatedHash, System.StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync("Invalid body hash.");
                        return;
                    }
                }
                catch (JsonException)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid JSON format in request body.");
                    return;
                }
            }

            // اگر همه چیز درست بود یا درخواست از نوع POST/JSON نبود، به مرحله بعد برو
            await _next(context);
        }

        private static JsonObject SortJsonObject(JsonObject jsonObject)
        {
            var sortedObject = new JsonObject();

            // مرتب‌سازی کلیدها با استفاده از Ordinal comparison برای ثبات در زبان‌های مختلف
            var sortedKeys = jsonObject.Select(kvp => kvp.Key).OrderBy(k => k, StringComparer.Ordinal);

            foreach (var key in sortedKeys)
            {
                var value = jsonObject[key];

                // اگر مقدار یک JsonObject است، آن را نیز به صورت بازگشتی مرتب کنیم
                if (value is JsonObject nestedObject)
                {
                    sortedObject[key] = SortJsonObject(nestedObject);
                }
                // اگر مقدار یک JsonArray است، عناصر داخل آن را بررسی کنیم
                else if (value is JsonArray jsonArray)
                {
                    var sortedArray = new JsonArray();
                    foreach (var item in jsonArray)
                    {
                        if (item is JsonObject nestedArrayObject)
                        {
                            sortedArray.Add(SortJsonObject(nestedArrayObject));
                        }
                        else
                        {
                            sortedArray.Add(item?.DeepClone());
                        }
                    }
                    sortedObject[key] = sortedArray;
                }
                else
                {
                    sortedObject[key] = value?.DeepClone();
                }
            }

            return sortedObject;
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using (var sha256Hash = SHA256.Create())
            {
                // استفاده از UTF-8 encoding برای پشتیبانی صحیح از کاراکترهای فارسی
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // تبدیل آرایه بایت به رشته هگزادسیمال
                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}