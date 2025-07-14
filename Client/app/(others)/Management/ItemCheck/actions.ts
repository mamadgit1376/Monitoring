// app/items/actions.ts
"use server";

import { apiFetch } from "@/lib/apiFetch";
import { revalidatePath } from "next/cache";


export interface CompanyOption {
  value: string;
  label: string;
}

const API_URL = process.env.NEXT_PUBLIC_AUTH_API_URL;

// --- Server Actions ---

/**
 * Fetches the list of all items.
 */
export async function getItemsAction() {
  if (!API_URL) throw new Error("API URL is not configured.");
  return await apiFetch<any>(`${API_URL}/Monitor/GetListOfItems`);
}

/**
 * Fetches the list of companies for the dropdown.
 */
export async function getCompanyOptionsAction() {
  if (!API_URL) throw new Error("API URL is not configured.");
  return await apiFetch<any>(`${API_URL}/Monitor/GetCompanyComboBox`);
}

/**
 * Adds or edits an item.
 */
export async function addOrEditItemAction(prevState: any, formData: FormData) {
  if (!API_URL) throw new Error("API URL is not configured.");

  const isEdit = !!formData.get("oldId");
  const payload = {
    oldId: isEdit ? Number(formData.get("oldId")) : null,
    isDelete: false,
    itemName: formData.get("itemName"),
    repeatTimeMinute: Number(formData.get("repeatTimeMinute")),
    additionalUrlAddress: formData.get("additionalUrlAddress"),
    importanceLevel: Number(formData.get("importanceLevel")),
    tblCategoryId: Number(formData.get("tblCategoryId")),
    companyIds: null, // Company assignment is handled separately
  };

  const result = await apiFetch(`${API_URL}/Management/AddOrEditItem`, {
    method: "POST",
    body: JSON.stringify(payload),
    returnErrors: true,
  });

  if (result.success) {
    revalidatePath("/items"); // Use the correct path for your items page
  }

  return result;
}

/**
 * Deletes (disables) an item.
 */
export async function deleteItemAction(itemId: number) {
  if (!API_URL) throw new Error("API URL is not configured.");
  const payload = { oldId: itemId, isDelete: true };

  const result = await apiFetch(`${API_URL}/Management/AddOrEditItem`, {
    method: "POST",
    body: JSON.stringify(payload),
    returnErrors: true,
  });

  if (result.success) {
    revalidatePath("/items");
  }

  return result;
}

/**
 * Assigns selected companies to a specific item.
 */
export async function assignCompaniesToItemAction(itemId: number, companyIds: string[]) {
  if (!API_URL) throw new Error("API URL is not configured.");

  const result = await apiFetch(
    `${API_URL}/Management/AssignCompaniesToItem?itemId=${itemId}`,
    {
      method: "POST",
      body: JSON.stringify(companyIds),
      returnErrors: true,
    }
  );

  if (result.success) {
    revalidatePath("/items");
  }

  return result;
}
