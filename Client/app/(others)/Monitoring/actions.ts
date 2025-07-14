// app/monitoring/actions.ts
"use server";

import { apiFetch } from "@/lib/apiFetch";

// --- Model Interface ---
export interface ShowMonitoringModel {
  companyId: number;
  companyName: string;
  successCount: number;
  errorCount: number;
  warningCount: number;
  percentSuccess: number;
}

const API_URL = process.env.NEXT_PUBLIC_AUTH_API_URL;

// --- Server Action ---

/**
 * Fetches the monitoring data for all companies.
 * This is designed to be called from a Server Component.
 */
export async function getMonitoringDataAction() {
  if (!API_URL) {
    throw new Error("API URL is not configured.");
  }

  // apiFetch will handle authentication and throw an error if the fetch fails.
  // Next.js will automatically catch this error and show the nearest error.js file.
  return await apiFetch<ShowMonitoringModel[]>(
    `${API_URL}/Monitor/GetComponiesMonitoring`
  );
}
