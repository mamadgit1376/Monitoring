// app/categories/actions.ts
"use server"; // این دستورالعمل مشخص می‌کند که کل این فایل فقط روی سرور اجرا می‌شود

import { apiFetch } from "@/lib/apiFetch";
import { revalidatePath } from "next/cache";


const API_URL = process.env.NEXT_PUBLIC_AUTH_API_URL;

/**
 * Server Action برای دریافت لیست دسته‌بندی‌ها
 */
export async function getCategoriesAction() {
  if (!API_URL) {
    throw new Error("API URL is not configured.");
  }
  // این تابع روی سرور اجرا شده و نتیجه را برمی‌گرداند
  return await apiFetch<any>(
    `${API_URL}/Monitor/GetListOfCategories`,
    {
      method: "GET",
      returnErrors: true, // همیشه نتیجه ساختاریافته را برگردان
    }
  );
}

/**
 * Server Action برای افزودن یک دسته‌بندی جدید
 */
export async function addCategoryAction(prevState: any, formData: FormData) {
  const categoryName = formData.get("categoryName") as string;

  if (!API_URL) {
    throw new Error("API URL is not configured.");
  }

  if (!categoryName) {
    return {
      success: false,
      status: 400,
      error: { message: "نام دسته‌بندی الزامی است." },
    };
  }

  const result = await apiFetch(`${API_URL}/Management/AddOrEditCategory`, {
    method: "POST",
    body: JSON.stringify({ categoryName }),
    returnErrors: true,
  });

  // اگر موفقیت‌آمیز بود، کش این صفحه را پاک می‌کنیم تا لیست به‌روز شود
  if (result.success) {
    revalidatePath("/categories"); // مسیر صفحه خود را وارد کنید
  }

  return result;
}