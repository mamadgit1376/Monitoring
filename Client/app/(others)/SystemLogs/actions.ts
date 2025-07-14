// app/item-logs/actions.ts
"use server";

import { apiFetch } from "@/lib/apiFetch";
import { CalendarDate } from "@internationalized/date";

// --- Model Interfaces ---
export interface ItemLogViewModel {
  id: number;
  creatorId: number;
  itemName: string;
  statusName: string;
  companyName: string;
  createDate: string;
  responseHtmlStatus: string;
  fullUrl: string;
  statusDescription: string;
  statusType: number;
}

// Updated to match actual API response structure
export interface ApiResponse {
  message: string;
  data: ItemLogViewModel[];
}

// Simple pagination data that we'll construct client-side
export interface PaginationInfo {
  page: number;
  pageSize: number;
}

export interface FilterOption {
  value: string;
  label: string;
}

const API_URL = process.env.NEXT_PUBLIC_AUTH_API_URL;

// --- Helper Functions ---
const formatDateForApi = (date: string) => {
  if (!date) return "";
  const d = new CalendarDate(
    parseInt(date.split("-")[0]),
    parseInt(date.split("-")[1]),
    parseInt(date.split("-")[2])
  );
  return `${d.year}-${String(d.month).padStart(2, "0")}-${String(
    d.day
  ).padStart(2, "0")}`;
};

// --- Server Actions ---

/**
 * Fetches all the necessary options for the filter dropdowns.
 */
export async function getFilterOptionsAction() {
  if (!API_URL) throw new Error("API URL is not configured.");

  const [companiesRes, itemsRes, statusRes] = await Promise.all([
    apiFetch<any>(`${API_URL}/Monitor/GetCompanyComboBox`),
    apiFetch<any>(`${API_URL}/Monitor/GetListOfItemsCombo`),
    apiFetch<any>(`${API_URL}/Monitor/GetTblStatusCode`),
  ]);

  return {
    companies: companiesRes.data?.data || [],
    items: itemsRes.data?.data || [],
    statuses: statusRes.data?.data || [],
  };
}

/**
 * Fetches the list of item logs based on filters.
 * Returns the actual API response structure.
 */
export async function getItemLogsAction(params: {
  page: number;
  pageSize: number;
  fromDate?: string;
  toDate?: string;
  htmlStatus?: string;
  companyCode?: string;
  itemCode?: string;
  statusItem?: string;
}) {
  if (!API_URL) throw new Error("API URL is not configured.");

  const queryParams = new URLSearchParams({
    Page: params.page.toString(),
    PageSize: params.pageSize.toString(),
  });

  if (params.fromDate) queryParams.set("FromDate", formatDateForApi(params.fromDate));
  if (params.toDate) queryParams.set("ToDate", formatDateForApi(params.toDate));
  if (params.htmlStatus) queryParams.set("HtmlStatus", params.htmlStatus);
  if (params.companyCode) queryParams.set("CompanyCode", params.companyCode);
  if (params.itemCode) queryParams.set("ItemCode", params.itemCode);
  if (params.statusItem) queryParams.set("StatusItem", params.statusItem);
 
  const response = await apiFetch<ApiResponse>(
    `${API_URL}/Monitor/GetListOfItemLogs?${queryParams}`
  );

  return {
    logs: response.data?.data || [],
    pagination: {
      page: params.page,
      pageSize: params.pageSize,
    }
  };
}