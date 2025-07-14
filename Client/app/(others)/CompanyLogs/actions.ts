// app/(others)/CompanyLogs/actions.ts
"use server";

import { apiFetch } from "@/lib/apiFetch";

// --- TYPE DEFINITIONS ---
// Moved from page.tsx to be shared across server and client

export interface LogsModel {
  companyId: number;
  addDate?: string | null;
  addTime?: string | null;
  finishTime?: string | null;
  nameOfUser?: string | null;
  functionDescription?: string | null;
  logType?: string | null;
  time?: number | null;
  ip?: string | null;
  message?: string | null;
  functionName?: string | null;
  className?: string | null;
  stackTrace?: string | null;
  extraInfo?: string | null;
  browserAddress?: string | null;
  agentName?: string | null;
}

export interface FilteredLogsModel {
  companyId: number | null;
  filterDate?: string;
  fromTime?: string;
  toTime?: string;
  filterIp?: string;
  filterText?: string;
  enteredUsedIds?: string;
  filteredUsedIds?: string;
  logTypeCodes?: string;
  functionIds?: string;
}

export interface ComboLogDto {
  id: number | null;
  name: string | null;
  type: string | null;
  description: string | null;
}

export interface ClientLogCombos {
  functionLogs: ComboLogDto[];
  typeLogs: ComboLogDto[];
}

export interface CompanyOption {
  value: string;
  label: string;
}


// --- SERVER ACTIONS ---

export async function fetchLogsAction(filters: FilteredLogsModel): Promise<{
  success: boolean;
  data?: LogsModel[];
  error?: string;
}> {
  if (!filters.companyId) {
    return {
      success: false,
      error: "لطفاً ابتدا شرکت را انتخاب کنید",
    };
  }

  try {
    const response = await apiFetch(
      `${process.env.NEXT_PUBLIC_AUTH_API_URL}/Monitor/GetListOfCompanyLogs`,
      {
        method: "POST",
        requireAuth: true,
        returnErrors: true,
        body: JSON.stringify(filters),
      }
    );

    if (!response.success) {
      return {
        success: false,
        error: response.error?.message || "خطا در دریافت داده‌ها",
      };
    }

    const responseData = response.data?.data || response.data || [];
    const logs = Array.isArray(responseData) ? responseData : [];

    return {
      success: true,
      data: logs,
    };
  } catch (error: any) {
    return {
      success: false,
      error: error?.message || "خطا در دریافت داده‌ها",
    };
  }
}

export async function fetchLogAndFunctionCombosAction(companyId: number): Promise<{
  success: boolean;
  data?: any;
  error?: string;
}> {
  try {
    const response = await apiFetch(
      `${process.env.NEXT_PUBLIC_AUTH_API_URL}/Monitor/BindComboOfComapnyLogs?CompanyId=${companyId}`,
      {
        method: "POST",
        requireAuth: true,
        returnErrors: true,
      }
    );

    if (!response.success) {
      return {
        success: false,
        error: response.error?.message || "خطا در دریافت داده‌ها",
      };
    }

    const comboData = response.data?.data || response.data;

    return {
      success: true,
      data: comboData,
    };
  } catch (error: any) {
    return {
      success: false,
      error: error?.message || "خطا در دریافت داده‌ها",
    };
  }
}