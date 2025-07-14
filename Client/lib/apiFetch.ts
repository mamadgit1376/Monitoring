"use server";

import { auth, signIn } from "@/auth";
import { redirect } from "next/navigation";
import crypto from "crypto"; // ماژول داخلی Node.js برای رمزنگاری

export interface ApiFetchOptions extends RequestInit {
  requireAuth?: boolean;
  returnErrors?: boolean;
  requireSiteSignature?: boolean;
}

export interface ApiFetchResult<T = any> {
  success: boolean;
  status: number;
  data?: T;
  error?: {
    message: string;
  };
}

function generateSiteSignature(siteKey: string): string {
  const now = new Date();
  const year = now.getUTCFullYear();
  const month = String(now.getUTCMonth() + 1).padStart(2, "0");
  const day = String(now.getUTCDate()).padStart(2, "0");

  // متغیر ساعت را حذف کرده و از payload بردارید
  const payload = `${year}${month}${day}`; // فقط شامل روز است

  const hmac = crypto.createHmac("sha256", siteKey);
  hmac.update(payload, "utf8");
  return hmac.digest("hex");
}

// Helper function to convert string to Unicode escape sequences (like .NET does)
function toUnicodeEscapeSequences(str: string): string {
  return str.replace(/[\u0080-\uFFFF]/g, function(match) {
    return '\\u' + ('0000' + match.charCodeAt(0).toString(16).toUpperCase()).substr(-4);
  });
}

// Helper function to recursively sort JSON objects and convert to Unicode escape format
function sortJsonObject(obj: any): any {
  if (obj === null || obj === undefined) {
    return obj;
  }
  
  if (Array.isArray(obj)) {
    // برای آرایه‌ها، هر عنصر را بررسی و مرتب می‌کنیم
    return obj.map(item => sortJsonObject(item));
  }
  
  if (typeof obj === 'object') {
    const sorted: any = {};
    // مرتب‌سازی کلیدها با استفاده از sort ساده برای تطابق دقیق با .NET
    const sortedKeys = Object.keys(obj).sort();
    
    for (const key of sortedKeys) {
      sorted[key] = sortJsonObject(obj[key]);
    }
    
    return sorted;
  }
  
  return obj;
}

// Helper function to perform the token refresh.
async function getNewTokensAndUpdateSession(
  session: import("next-auth").Session
): Promise<string | null> {
  try {
    const refreshUrl =
      process.env.NEXT_PUBLIC_AUTH_API_URL + "/User/RefreshToken";
    const response = await fetch(refreshUrl, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken: session.refreshToken }),
      cache: "no-store",
    });

    const data = await response.json();

    if (!response.ok || !data.data?.token) {
      console.error(
        "Failed to refresh token:",
        data.message || "Unknown error"
      );
      return null;
    }

    const newAccessToken = data.data.token;
    const newRefreshToken = data.data.refreshToken;

    // --- بخش کلیدی اصلاح شده ---
    // از `signIn` برای تریگر کردن آپدیت session استفاده می‌کنیم
    // بدون ارسال رمز عبور و با استفاده از حالتی که در authorize تعریف کردیم.
    await signIn("credentials", {
      redirect: false,
      // اطلاعات کاربر از session فعلی
      PhoneNumber: session.user.phoneNumber,
      role: session.user.role,
      // توکن‌های جدید برای آپدیت
      accessToken: newAccessToken,
      refreshToken: newRefreshToken,
    });
    return newAccessToken; // Return the new access token
  } catch (error) {
    console.error("An error occurred during token refresh:", error);
    return null;
  }
}

export async function apiFetch<T = any>(
  input: string,
  options: ApiFetchOptions = {}
): Promise<ApiFetchResult<T>> {
  const {
    requireAuth = true,
    returnErrors = false,
    requireSiteSignature = true,
    ...fetchOptions
  } = options;

  let session = requireAuth ? await auth() : null;
  if (requireAuth && !session) {
    return redirect("/"); // بهتر است به صفحه لاگین هدایت شود
  }

  if (requireAuth && session?.accessToken) {
    const headers = (fetchOptions.headers as Record<string, string>) || {};
    headers["Authorization"] = `Bearer ${session.accessToken}`;
    fetchOptions.headers = headers;
  }

  if (!fetchOptions.headers) {
    fetchOptions.headers = {};
  }

  (fetchOptions.headers as Record<string, string>)["Content-Type"] =
    "application/json";
  
  // افزودن امضای سایت در صورت نیاز
  if (requireSiteSignature) {
    const siteKey = process.env.API_SITE_KEY;
    if (!siteKey) {
      console.error("متغیر محیطی API_SITE_KEY تنظیم نشده است.");
      throw new Error("کلید امضای سایت در پیکربندی سرور یافت نشد.");
    }
    const signature = generateSiteSignature(siteKey);
    fetchOptions.headers["X-Site-Signature"] = signature;
  }
  
  // 🆕 منطق جدید برای مرتب‌سازی، هش کردن body و افزودن آن
  if (fetchOptions.body && typeof fetchOptions.body === "string") {
    try {
      // 🆕 Step 1: Parse original body (string) to object
      const originalBodyObject = JSON.parse(fetchOptions.body);

      // 🆕 Step 2: Clone and remove `hashbody` (if it existed accidentally)
      const bodyToHash = { ...originalBodyObject };
      delete bodyToHash.hashbody;

      // 🆕 Step 3: Sort the object properties recursively
      const sortedBody = sortJsonObject(bodyToHash);

      // 🆕 Step 4: Serialize using the same rules as .NET (compact JSON with Unicode escapes)
      const canonicalJson = JSON.stringify(sortedBody);
      
      // 🆕 Step 5: Convert to Unicode escape sequences to match .NET format
      const canonicalJsonWithUnicodeEscapes = toUnicodeEscapeSequences(canonicalJson);

      // 🆕 Step 6: Hash the serialized string with Unicode escapes
      const bodyHash = crypto
        .createHash("sha256")
        .update(canonicalJsonWithUnicodeEscapes, "utf8")
        .digest("hex");

      // 🆕 Step 7: Add `hashbody` to the original object and re-serialize
      originalBodyObject.hashbody = bodyHash;
      fetchOptions.body = JSON.stringify(originalBodyObject);
      
      // اضافه کردن لاگ برای دیباگ (می‌توانید در production حذف کنید)
      console.log("Canonical JSON for hashing:", canonicalJsonWithUnicodeEscapes);
      console.log("Generated hash:", bodyHash);
      
    } catch (e) {
      console.error(
        "پردازش و هش کردن body با خطا مواجه شد. ممکن است body یک JSON معتبر نباشد.",
        e
      );
      // بهتر است در این حالت یک خطا پرتاب شود تا توسعه‌دهنده متوجه مشکل شود
      throw new Error("Body ارسال شده برای هش کردن قابل پردازش نبود.");
    }
  }
  
  // --- 1. FIRST ATTEMPT ---
  let res = await fetch(input, { ...fetchOptions, cache: "no-store" });

  // --- 2. HANDLE 401: REFRESH AND RETRY ---
  if (res.status === 401 && requireAuth && session) {
    if (session.refreshToken) {
      // session کامل را به تابع پاس می‌دهیم
      const newAccessToken = await getNewTokensAndUpdateSession(session);

      if (newAccessToken) {
        (fetchOptions.headers as Record<string, string>)[
          "Authorization"
        ] = `Bearer ${newAccessToken}`;

        res = await fetch(input, { ...fetchOptions, cache: "no-store" });
      } else {
        return redirect("/");
      }
    } else {
      return redirect("/");
    }
  }

  // --- 3. PROCESS FINAL RESPONSE ---
  try {
    if (res.status === 401 || res.status === 403) {
      return redirect("/");
    }

    if (res.ok) {
      if (
        res.status === 204 ||
        !res.headers.get("content-type")?.includes("application/json")
      ) {
        return { success: true, status: res.status, data: undefined };
      }
      const data = await res.json();
      return { success: true, status: res.status, data };
    }

    const errorData = await res.json().catch(() => ({}));
    const errorMessage =
      errorData.message || `HTTP Error: ${res.status} ${res.statusText}`;

    if (returnErrors) {
      return {
        success: false,
        status: res.status,
        error: { message: errorMessage },
      };
    }

    throw new Error(errorMessage);
  } catch (error) {
    if (
      error instanceof Error &&
      (error as any).digest?.startsWith("NEXT_REDIRECT")
    ) {
      throw error;
    }

    console.error(
      "An unexpected error occurred in apiFetch processing:",
      error
    );

    if (returnErrors) {
      return {
        success: false,
        status: 500,
        error: {
          message:
            error instanceof Error
              ? error.message
              : "A server connection error occurred.",
        },
      };
    }

    throw error;
  }
}