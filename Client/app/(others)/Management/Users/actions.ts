// app/users/actions.ts
"use server";

import { apiFetch } from "@/lib/apiFetch";
import { revalidatePath } from "next/cache";

// --- Model Interface ---
export interface UserModel {
  id: number;
  userName: string;
  companies: string;
  fullName: string;
  role: string;
  removeDate?: string | null;
}

const API_URL = process.env.NEXT_PUBLIC_AUTH_API_URL;

// --- Server Actions ---

/**
 * Fetches the list of all users from the API.
 */
export async function getUsersAction() {
  if (!API_URL) throw new Error("API URL is not configured.");
  return await apiFetch<any>(`${API_URL}/Management/GetUsers`, {
    method: "GET", // The endpoint requires GET
    returnErrors: true,
    requireSiteSignature: true,
  });
}

/**
 * Adds a new user. This is a form action.
 */
export async function addUserAction(prevState: any, formData: FormData) {
  if (!API_URL) throw new Error("API URL is not configured.");

  const companyId = formData.get("companeyId");
  const payload = {
    Username: formData.get("username"),
    FullName: formData.get("fullName"),
    UserRole: Number(formData.get("userRole")),
    CompanyId: companyId ? Number(companyId) : null,
  };

  const result = await apiFetch(`${API_URL}/Management/AddNewUser`, {
    method: "POST",
    body: JSON.stringify(payload),
    returnErrors: true,
  });

  if (result.success) {
    revalidatePath("/users"); // Use the correct path for your users page
  }

  return result;
}

/**
 * Resets a user's password.
 */
export async function resetPasswordAction(userId: number) {
  if (!API_URL) throw new Error("API URL is not configured.");
  const result = await apiFetch(
    `${API_URL}/Management/ResetUserPassword?ThisUserId=${userId}`,
    {
      method: "POST",
      returnErrors: true,
    }
  );

  return result;
}

/**
 * Enables or disables a user.
 */
export async function toggleUserStatusAction(userId: number, enable: boolean) {
  if (!API_URL) throw new Error("API URL is not configured.");
  const payload = {
    ThisUserId: userId.toString(),
    IsForDisable: !enable,
    IsForEnable: enable,
  };

  const result = await apiFetch(`${API_URL}/Management/DisableUsers`, {
    method: "POST",
    body: JSON.stringify(payload),
    returnErrors: true,
  });

  if (result.success) {
    revalidatePath("/users");
  }

  return result;
}
