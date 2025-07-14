// actions.ts
"use server";

import { apiFetch } from "@/lib/apiFetch"; // مسیر ابزار fetch خود را وارد کنید

// تعریف نوع داده‌ای که به API ارسال می‌شود
interface ChangeUserPasswordModel {
  oldPassword?: string;
  newPassword?: string;
  rNewPassword?: string;
}

// تعریف نوع پاسخی که از اکشن برمی‌گردد
interface ActionResult {
  success: boolean;
  message: string;
}

export async function changeUserPasswordAction(
  prevState: any,
  formData: FormData
): Promise<ActionResult> {
  const oldPassword = formData.get("oldPassword") as string;
  const newPassword = formData.get("newPassword") as string;
  const rNewPassword = formData.get("rNewPassword") as string;

  // اعتبارسنجی اولیه
  if (!oldPassword || !newPassword || !rNewPassword) {
    return { success: false, message: "لطفاً تمام فیلدها را پر کنید." };
  }

  if (newPassword !== rNewPassword) {
    return { success: false, message: "رمز عبور جدید و تکرار آن برابر نیستند." };
  }

  try {
    const response = await apiFetch(`${process.env.NEXT_PUBLIC_AUTH_API_URL}/User/ChangeUserPassword`, {
      method: "POST",
      requireAuth: true, // چون API شما [Authorize] دارد
      returnErrors: true,
      body: JSON.stringify({
        oldPassword,
        newPassword,
        rNewPassword,
      } as ChangeUserPasswordModel),
    });

    if (response.success) {
      // پیام موفقیت‌آمیز از API شما
      return { success: true, message: response.data?.message || "رمز عبور با موفقیت تغییر کرد." };
    } else {
      // پیام خطا از API شما
      return { success: false, message: response.data?.message || "خطایی در تغییر رمز عبور رخ داد." };
    }
  } catch (error) {
    console.error("Change Password Action Error:", error);
    return { success: false, message: "خطای غیرمنتظره در سرور. لطفاً دوباره تلاش کنید." };
  }
}