import { Suspense } from "react";
import { apiFetch } from "@/lib/apiFetch";
// V-- THIS LINE MUST MATCH YOUR FILENAME EXACTLY --V
import CompanyLogsClient from "./company-logs-client"; 
import { CompanyOption } from "./actions";

async function getInitialData() {
  try {
    // Fetch companies
    const companiesResponse = await apiFetch<any>(
      `${process.env.NEXT_PUBLIC_AUTH_API_URL}/Monitor/GetCompanyComboBox`,
      {
        method: "GET",
        requireAuth: true,
        returnErrors: true,
      }
    );

    if (!companiesResponse.success) {
      throw new Error(companiesResponse.error?.message || "Failed to fetch companies");
    }

    const companies = companiesResponse.data?.data || companiesResponse.data || [];

    return {
      companies: companies.map((company: any) => ({
        value: company.value,
        label: company.label,
      })),
    };
  } catch (error) {
    console.error("Error fetching initial data:", error);
    return {
      companies: [],
    };
  }
}

export default async function CompanyLogsPage() {
  const { companies } = await getInitialData();

  return (
    <div className="p-4 bg-base-100 min-h-screen" dir="rtl">
      <div className="card bg-base-100 shadow-xl">
        <div className="card-body">
          <h2 className="card-title text-right text-base-content text-2xl mb-6">
            مشاهده لاگ شرکت‌ها
          </h2>

          <Suspense fallback={
            <div className="flex justify-center items-center py-8">
              <div className="loading loading-spinner"></div>
              <span className="mr-2">در حال بارگذاری...</span>
            </div>
          }>
            <CompanyLogsClient initialCompanies={companies} />
          </Suspense>
        </div>
      </div>
    </div>
  );
}