// app/Monitoring/[id]/actions.ts
"use server";

import { apiFetch } from "@/lib/apiFetch";

// --- Model Interfaces ---
export interface HttpStatus {
  statusCode: number;
  statusDescription: string;
}

export interface ItemLogViewModel {
  id: number;
  companyId: number;
  creatorId: number;
  itemName: string;
  statusName: string;
  statusDescription: string;
  statusType: number;
  companyName: string;
  createDate: string;
  httpStatus: HttpStatus;
  fullUrl: string;
}

export interface ShowSingleCompanyMonitoring {
  companyId: number;
  companyName: string;
  itemLog: ItemLogViewModel[];
}

const API_URL = process.env.NEXT_PUBLIC_AUTH_API_URL;

// --- Server Action ---

/**
 * Fetches the monitoring data for a single company.
 * Designed to be called from a Server Component.
 */
export async function getSingleMonitoringDataAction(companyId: string) {
  if (!API_URL) {
    throw new Error("API URL is not configured.");
  }

  // apiFetch will handle authentication and throw an error on failure.
  // Next.js will catch the error and display the nearest error.js file.
  return await apiFetch<ShowSingleCompanyMonitoring>(
    `${API_URL}/Monitor/GetSingleCompanyMonitoring?CompanyId=${companyId}`
  );
}
