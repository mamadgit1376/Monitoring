// app/Monitoring/[id]/page.tsx
import Link from "next/link";
import {
  getSingleMonitoringDataAction,
  ShowSingleCompanyMonitoring,
  ItemLogViewModel,
} from "./actions";
import { MonitoringGridClient } from "./MonitoringGridClient";

// --- Helper Functions (can stay in the Server Component) ---

const getStats = (itemLog: ItemLogViewModel[] = []) => {
  const success = itemLog.filter(item => item.statusType === 1).length;
  const warning = itemLog.filter(item => item.statusType === 2).length;
  const error = itemLog.filter(item => item.statusType === 0).length;
  return { success, warning, error, total: itemLog.length };
};

// --- Page Component (now a Server Component) ---

export default async function ShowSingleMonitoringPage({ params }: { params: { id: string } }) {
  // Data is fetched securely on the server when the page is requested.
  const { data: response, error } = await getSingleMonitoringDataAction(params.id);

  // Handle errors or no data gracefully
  if (error || !response?.data) {
    return (
      <div className="min-h-screen bg-base-100 p-6 flex items-center justify-center">
        <div className="card bg-base-200 shadow-xl max-w-md w-full text-center">
          <div className="card-body">
            <h2 className="card-title justify-center">خطا در بارگذاری</h2>
            <p className="text-error">{error?.message || "اطلاعات این شرکت یافت نشد."}</p>
            <div className="card-actions justify-center mt-4">
              <Link href="/Monitoring" className="btn btn-primary">بازگشت به داشبورد</Link>
            </div>
          </div>
        </div>
      </div>
    );
  }

  const data = response.data;
  const stats = getStats(data.itemLog);

  return (
    <div className="min-h-screen bg-base-100 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <div className="flex flex-wrap items-center justify-between gap-4 mb-4">
            <div>
              <h1 className="text-3xl font-bold text-base-content mb-2">
                مانیتورینگ {data.companyName}
              </h1>
              <div className="breadcrumbs text-sm">
                <ul>
                  <li><Link href="/Monitoring" className="link link-primary">داشبورد</Link></li>
                  <li>{data.companyName}</li>
                </ul>
              </div>
            </div>
            <div className="text-sm text-base-content opacity-60">
              آخرین به‌روزرسانی: {new Date().toLocaleString('fa-IR')}
            </div>
          </div>
          
          {/* Summary Stats */}
          <div className="stats shadow w-full">
            <div className="stat"><div className="stat-title">کل لاگ‌ها</div><div className="stat-value text-primary">{stats.total}</div></div>
            <div className="stat"><div className="stat-title">موفق</div><div className="stat-value text-success">{stats.success}</div></div>
            <div className="stat"><div className="stat-title">هشدار</div><div className="stat-value text-warning">{stats.warning}</div></div>
            <div className="stat"><div className="stat-title">خطا</div><div className="stat-value text-error">{stats.error}</div></div>
          </div>
        </div>

        {/* The interactive grid is now a Client Component */}
        <MonitoringGridClient itemLog={data.itemLog} />

      </div>
    </div>
  );
};
