// app/monitoring/page.tsx
import Link from "next/link";
import { getMonitoringDataAction, ShowMonitoringModel } from "./actions";

// --- Helper Functions (can stay in the same file) ---

const getStatusInfo = (percent: number) => {
  if (percent === 100) {
    return {
      borderColor: "border-l-success",
      textColor: "text-success",
      badgeBg: "badge-success",
      icon: "🟢",
      status: "عالی",
    };
  }
  if (percent >= 80) {
    return {
      borderColor: "border-l-warning",
      textColor: "text-warning",
      badgeBg: "badge-warning",
      icon: "🟡",
      status: "قابل قبول",
    };
  }
  return {
    borderColor: "border-l-error",
    textColor: "text-error",
    badgeBg: "badge-error",
    icon: "🔴",
    status: "نیاز به توجه",
  };
};

const getTotalStats = (data: ShowMonitoringModel[]) => {
  if (!data || data.length === 0)
    return { total: 0, healthy: 0, warning: 0, critical: 0 };

  const healthy = data.filter((c) => c.percentSuccess === 100).length;
  const warning = data.filter(
    (c) => c.percentSuccess >= 80 && c.percentSuccess < 100
  ).length;
  const critical = data.filter((c) => c.percentSuccess < 80).length;

  return { total: data.length, healthy, warning, critical };
};

// --- Page Component (now a Server Component) ---

export default async function MonitoringPage() {
  // Data is fetched directly on the server when the page is requested.
  // No need for useState, useEffect, or loading states.
  const { data, error } = await getMonitoringDataAction();

  if (error || !data?.data) {
     // This error state will be shown if the fetch fails but doesn't throw.
     // If apiFetch throws (e.g., on a 401), Next.js will show the error.js boundary.
    return (
       <div className="min-h-screen bg-base-100 p-6 flex items-center justify-center">
         <div className="card bg-base-200 shadow-xl max-w-md w-full">
           <div className="card-body text-center">
             <div className="text-error text-4xl mb-4">⚠️</div>
             <h2 className="card-title justify-center text-base-content mb-2">
               خطا در بارگذاری
             </h2>
             <p className="text-error mb-4">{error?.message || "اطلاعات مانیتورینگ در دسترس نیست."}</p>
             <div className="card-actions justify-center">
               <a href="/monitoring" className="btn btn-error">تلاش مجدد</a>
             </div>
           </div>
         </div>
       </div>
    );
  }
  
  const monitoringData = data.data;
  const stats = getTotalStats(monitoringData);

  return (
    <div className="min-h-screen bg-base-100 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <div className="flex items-center justify-between mb-4">
            <h1 className="text-3xl font-bold text-base-content">
              داشبورد مانیتورینگ شرکت‌ها
            </h1>
            <div className="text-sm text-base-content opacity-60">
              آخرین به‌روزرسانی: {new Date().toLocaleString("fa-IR")}
            </div>
          </div>

          {/* Summary Stats */}
          <div className="stats shadow w-full mb-6">
            <div className="stat">
              <div className="stat-title">کل شرکت‌ها</div>
              <div className="stat-value text-primary">{stats.total}</div>
            </div>
            <div className="stat">
              <div className="stat-title">عملکرد عالی</div>
              <div className="stat-value text-success">{stats.healthy}</div>
            </div>
            <div className="stat">
              <div className="stat-title">هشدار</div>
              <div className="stat-value text-warning">{stats.warning}</div>
            </div>
            <div className="stat">
              <div className="stat-title">بحرانی</div>
              <div className="stat-value text-error">{stats.critical}</div>
            </div>
          </div>
        </div>

        {/* Company Cards */}
        {monitoringData.length === 0 ? (
          <div className="card bg-base-200 shadow-xl">
            <div className="card-body text-center py-12">
              <h3 className="text-xl font-semibold text-base-content">
                هیچ داده‌ای یافت نشد
              </h3>
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {monitoringData.map((company) => {
              const statusInfo = getStatusInfo(company.percentSuccess);
              const totalOperations =
                company.successCount +
                company.warningCount +
                company.errorCount;

              return (
                <Link href={`/Monitoring/${company.companyId}`} key={company.companyId}>
                  <div className={`card bg-base-100 shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105 border-l-4 ${statusInfo.borderColor}`}>
                    <div className="card-body">
                      <div className="flex items-center justify-between mb-4">
                        <div className="flex items-center gap-2">
                          <span className="text-xl">{statusInfo.icon}</span>
                          <div className={`badge ${statusInfo.badgeBg} badge-sm`}>
                            {statusInfo.status}
                          </div>
                        </div>
                      </div>
                      <h2 className="card-title text-lg mb-4 text-base-content">
                        {company.companyName}
                      </h2>
                      <div className="mb-4">
                        <div className="flex items-center justify-between mb-2">
                          <span className="text-sm font-medium text-base-content opacity-70">
                            درصد موفقیت
                          </span>
                          <span className={`text-xl font-bold ${statusInfo.textColor}`}>
                            {company.percentSuccess}%
                          </span>
                        </div>
                        <progress
                          className={`progress w-full ${
                            company.percentSuccess === 100
                              ? "progress-success"
                              : company.percentSuccess >= 80
                              ? "progress-warning"
                              : "progress-error"
                          }`}
                          value={company.percentSuccess}
                          max="100"
                        ></progress>
                      </div>
                      <div className="space-y-3">
                         <div className="flex items-center justify-between text-sm text-base-content">
                           <span className="flex items-center opacity-70">
                             <span className="w-3 h-3 bg-success rounded-full ml-2"></span>
                             موفق
                           </span>
                           <span className="font-semibold">{company.successCount.toLocaleString("fa-IR")}</span>
                         </div>
                         <div className="flex items-center justify-between text-sm text-base-content">
                           <span className="flex items-center opacity-70">
                             <span className="w-3 h-3 bg-warning rounded-full ml-2"></span>
                             هشدار
                           </span>
                           <span className="font-semibold">{company.warningCount.toLocaleString("fa-IR")}</span>
                         </div>
                         <div className="flex items-center justify-between text-sm text-base-content">
                           <span className="flex items-center opacity-70">
                             <span className="w-3 h-3 bg-error rounded-full ml-2"></span>
                             خطا
                           </span>
                           <span className="font-semibold">{company.errorCount.toLocaleString("fa-IR")}</span>
                         </div>
                         <div className="pt-2 border-t border-base-300">
                           <div className="flex items-center justify-between text-sm font-medium text-base-content">
                             <span className="opacity-70">کل عملیات</span>
                             <span>{totalOperations.toLocaleString("fa-IR")}</span>
                           </div>
                         </div>
                       </div>
                    </div>
                  </div>
                </Link>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}
