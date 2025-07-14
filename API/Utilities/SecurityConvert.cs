using System.Security.Cryptography;
using System.Text;

namespace Monitoring_Support_Server.Utilities
{
    public static class SecurityConvert
    {
        private static readonly byte[] DefaultEncryptionKey = Encoding.UTF8.GetBytes("gNQpf9ELdESp9ieDJnXUOelnfUsT4dGg"); // 32 characters for AES-256
        private static readonly byte[] DefaultInitializationVector = Encoding.UTF8.GetBytes("XpA2lgkznWx1ofGg"); // 16 characters for AES (128-bit block size)

            /// <summary>
            /// یک رشته را با استفاده از AES و کلید مشخص شده و IV اختیاری رمزنگاری می‌کند.
            /// اگر پارامترهای کلید یا IV خالی یا null باشند، مقادیر پیش‌فرض استفاده می‌شوند.
            /// هشدار: این متد برای نمایش از مقادیر پیش‌فرض هاردکد شده استفاده می‌کند که برای محیط تولید ناامن است.
            /// به ویژه استفاده از IV ثابت یک نقص امنیتی بحرانی است.
            /// </summary>
            /// <param name="plainText">رشته‌ای که باید رمزنگاری شود.</param>
            /// <param name="key">اختیاری. کلید رمزنگاری (۱۶، ۲۴ یا ۳۲ کاراکتر برای AES). اگر null یا خالی باشد مقدار پیش‌فرض استفاده می‌شود.</param>
            /// <param name="iv">اختیاری. بردار مقداردهی اولیه (۱۶ کاراکتر برای AES). اگر null یا خالی باشد مقدار پیش‌فرض استفاده می‌شود. هشدار: این ناامن است.</param>
            /// <returns>رشته رمزنگاری شده به صورت Base64 (با IV اضافه شده در ابتدای آن)، یا null اگر ورودی نامعتبر باشد یا طول کلید/IV (در صورت ارائه) نادرست باشد.</returns>
            public static string? EncryptFromString(this string plainText, string? key = null, string? iv = null)
            {
                if (string.IsNullOrEmpty(plainText))
                    return null;

                byte[] finalKeyBytes;
                if (string.IsNullOrEmpty(key))
                {
                    finalKeyBytes = DefaultEncryptionKey;
                }
                else
                {
                    finalKeyBytes = Encoding.UTF8.GetBytes(key);
                }

                // بررسی طول کلید برای AES
                if (finalKeyBytes.Length != 16 && finalKeyBytes.Length != 24 && finalKeyBytes.Length != 32)
                    return null; // طول کلید نامعتبر

                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = finalKeyBytes;
                    aesAlg.Mode = CipherMode.CBC; // حالت زنجیره‌ای بلوک (نیازمند IV)
                    aesAlg.Padding = PaddingMode.PKCS7;

                    byte[] finalIvBytes;
                    // بررسی اینکه آیا IV ارائه شده و طول آن صحیح است (۱۶ بایت برای AES)
                    if (!string.IsNullOrEmpty(iv) && Encoding.UTF8.GetBytes(iv).Length == aesAlg.BlockSize / 8)
                    {
                        // استفاده از IV ارائه شده
                        finalIvBytes = Encoding.UTF8.GetBytes(iv);
                    }
                    else
                    {
                        // هشدار: استفاده از IV ثابت بسیار ناامن است.
                        // برای امنیت، IV باید همیشه تصادفی و منحصر به فرد باشد.
                        finalIvBytes = DefaultInitializationVector;
                    }
                    aesAlg.IV = finalIvBytes;

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        // IV را به ابتدای متن رمزنگاری شده اضافه می‌کنیم تا هنگام رمزگشایی قابل بازیابی باشد.
                        // این برای حالت CBC ضروری است.
                        msEncrypt.Write(finalIvBytes, 0, finalIvBytes.Length);

                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                            csEncrypt.FlushFinalBlock();
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }

            /// <summary>
            /// یک رشته را با استفاده از AES و کلید مشخص شده و IV اختیاری رمزگشایی می‌کند.
            /// اگر پارامترهای کلید یا IV خالی یا null باشند، مقادیر پیش‌فرض استفاده می‌شوند.
            /// هشدار: این متد برای نمایش از مقادیر پیش‌فرض هاردکد شده استفاده می‌کند که برای محیط تولید ناامن است.
            /// به ویژه استفاده از IV ثابت یک نقص امنیتی بحرانی است.
            /// </summary>
            /// <param name="cipherText">رشته رمزنگاری شده به صورت Base64 (با IV اضافه شده در ابتدای آن اگر رمزنگاری اولیه از آن استفاده کرده باشد).</param>
            /// <param name="key">اختیاری. کلید رمزنگاری (۱۶، ۲۴ یا ۳۲ کاراکتر برای AES). اگر null یا خالی باشد مقدار پیش‌فرض استفاده می‌شود.</param>
            /// <param name="iv">اختیاری. بردار مقداردهی اولیه (۱۶ کاراکتر برای AES). اگر null یا خالی باشد مقدار پیش‌فرض استفاده می‌شود. هشدار: این ناامن است.</param>
            /// <returns>رشته رمزگشایی شده، یا null اگر ورودی نامعتبر باشد، طول کلید/IV (در صورت ارائه) نادرست باشد یا رمزگشایی با شکست مواجه شود.</returns>
            public static string? DecryptFromString(this string cipherText, string? key = null, string? iv = null)
            {
                if (string.IsNullOrEmpty(cipherText))
                    return null;

                byte[] finalKeyBytes;
                if (string.IsNullOrEmpty(key))
                {
                    finalKeyBytes = DefaultEncryptionKey;
                }
                else
                {
                    finalKeyBytes = Encoding.UTF8.GetBytes(key);
                }

                // بررسی طول کلید برای AES
                if (finalKeyBytes.Length != 16 && finalKeyBytes.Length != 24 && finalKeyBytes.Length != 32)
                    return null; // طول کلید نامعتبر

                byte[] cipherBytesWithIv = Convert.FromBase64String(cipherText);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = finalKeyBytes;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;

                    byte[] finalIvBytes;
                    int actualCipherTextOffset;
                    int ivLength = aesAlg.BlockSize / 8; // طول IV برای AES همیشه ۱۶ بایت (۱۲۸ بیت) است

                    // بررسی اینکه آیا IV ارائه شده و طول آن صحیح است
                    if (!string.IsNullOrEmpty(iv) && Encoding.UTF8.GetBytes(iv).Length == ivLength)
                    {
                        // استفاده از IV ارائه شده
                        finalIvBytes = Encoding.UTF8.GetBytes(iv);
                        actualCipherTextOffset = 0; // اگر IV جداگانه ارائه شده باشد، متن رمزنگاری شده آن را در ابتدای خود ندارد
                    }
                    else
                    {
                        // هشدار: استفاده از IV ثابت بسیار ناامن است.
                        // این رمزگشایی فقط زمانی موفق خواهد بود که رمزنگاری اولیه نیز از همین IV ثابت استفاده کرده باشد.
                        // اگر رمزنگاری با IV تصادفی و اضافه شده به ابتدای متن انجام شده باشد، این حالت شکست می‌خورد.
                        finalIvBytes = DefaultInitializationVector;
                        actualCipherTextOffset = ivLength; // فرض می‌کنیم IV به ابتدای متن اضافه شده است (مطابق با Encrypt)
                    }
                    aesAlg.IV = finalIvBytes;

                    // اگر IV از طریق پارامتر ارائه نشده باشد اما رمزنگاری با IV تصادفی و اضافه شده به ابتدای متن انجام شده باشد،
                    // این منطق تلاش می‌کند آن را از بخش مربوطه در متن رمزنگاری شده استخراج کند.
                    // این تعامل پیچیده است و می‌تواند منجر به شکست رمزگشایی شود.
                    if (actualCipherTextOffset > 0 && cipherBytesWithIv.Length < actualCipherTextOffset)
                    {
                        return null; // داده کافی برای IV وجود ندارد اگر انتظار می‌رفت به ابتدای متن اضافه شده باشد
                    }

                    // متن رمزنگاری شده واقعی بعد از offset شروع می‌شود.
                    int actualCipherTextLength = cipherBytesWithIv.Length - actualCipherTextOffset;
                    if (actualCipherTextLength <= 0)
                    {
                        return null; // هیچ متن رمزنگاری شده‌ای برای رمزگشایی وجود ندارد
                    }
                    byte[] actualCipherBytes = new byte[actualCipherTextLength];
                    Buffer.BlockCopy(cipherBytesWithIv, actualCipherTextOffset, actualCipherBytes, 0, actualCipherTextLength);

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(actualCipherBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
    }
}
