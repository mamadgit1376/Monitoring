// app/companies/actions.ts
"use server";

import { apiFetch } from "@/lib/apiFetch";
import { revalidatePath } from "next/cache";

const API_URL = process.env.NEXT_PUBLIC_AUTH_API_URL;

/**
 * Server Action برای دریافت لیست شرکت‌ها
 */
export async function getCompaniesAction() {
  if (!API_URL) throw new Error("API URL is not configured.");
  return await apiFetch<any>(
    `${API_URL}/Monitor/GetListOfCompanies`
  );
}

/**
 * Server Action برای افزودن یا ویرایش شرکت
 */
export async function addOrEditCompanyAction(prevState: any, formData: FormData) {
  if (!API_URL) throw new Error("API URL is not configured.");

  const isEdit = formData.get("isEdit") === "true";
  const id = formData.get("id");

  const payload = {
    OldId: isEdit ? Number(id) : null,
    IsEdit: isEdit,
    IsDelete: false,
    CompanyName: formData.get("companyName"),
    BaseUrlAddress: formData.get("baseUrlAddress"),
    LocationAddress: formData.get("locationAddress"),
    NationalCode: formData.get("nationalCode"),
    ApiUser: formData.get("apiUser"),
    ApiPassword: formData.get("apiPassword"),
  };

  const result = await apiFetch(
    `${API_URL}/Management/AddOrEditCompany`,
    {
      method: "POST",
      body: JSON.stringify(payload),
      returnErrors: true,
    }
  );

  if (result.success) {
    revalidatePath("/companies");
  }

  return result;
}

/**
 * Server Action برای غیرفعال کردن (حذف) یک شرکت
 */
export async function deleteCompanyAction(companyId: number) {
   if (!API_URL) throw new Error("API URL is not configured.");

   const payload = {
     OldId: companyId,
     IsEdit: false,
     IsDelete: true,
   };

   const result = await apiFetch(
    `${API_URL}/Management/AddOrEditCompany`,
     {
       method: "POST",
       body: JSON.stringify(payload),
       returnErrors: true,
     }
   );

   if (result.success) {
     revalidatePath("/companies");
   }

   return result;
}
