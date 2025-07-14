// app/Monitoring/[id]/MonitoringGridClient.tsx
"use client";

import Swal from "sweetalert2";
import { ItemLogViewModel } from "./actions"; // Import the type from actions

// --- Helper Function ---
const getStatusInfo = (statusType: number) => {
  switch (statusType) {
    case 1: return { bgColor: "bg-success/50", icon: "✅" };
    case 2: return { bgColor: "bg-warning/50", icon: "⚠️" };
    case 0: return { bgColor: "bg-error/50", icon: "❌" };
    default: return { bgColor: "bg-base-300", icon: "ℹ️" };
  }
};

// --- Client Component ---
export function MonitoringGridClient({ itemLog }: { itemLog: ItemLogViewModel[] }) {
  if (!itemLog || itemLog.length === 0) {
    return (
       <div className="card bg-base-200 shadow-xl">
         <div className="card-body text-center py-12">
           <h3 className="text-xl font-semibold text-base-content">هیچ لاگی یافت نشد</h3>
         </div>
       </div>
    );
  }

  return (
    <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
      {itemLog.map((log) => {
        const statusInfo = getStatusInfo(log.statusType);
        return (
          <div
            key={log.id}
            className={`cursor-pointer p-4 rounded-lg text-sm text-center ${statusInfo.bgColor} hover:brightness-90 transition-all`}
            onClick={() =>
              Swal.fire({
                title: log.itemName,
                html: `
                  <div style="text-align: right; direction: rtl; font-size: 14px;">
                    <p><strong>وضعیت:</strong> ${log.statusName}</p>
                    <p><strong>جزئیات:</strong> ${log.statusDescription || "-"}</p>
                    <p><strong>کد HTTP:</strong> ${log.httpStatus?.statusCode || "N/A"}</p>
                    <p><strong>URL:</strong> <a href="${log.fullUrl}" target="_blank" rel="noopener noreferrer" style="color: #6366f1;">مشاهده</a></p>
                    <p><strong>زمان:</strong> ${new Date(log.createDate).toLocaleString('fa-IR')}</p>
                  </div>
                `,
                confirmButtonText: 'بستن',
              })
            }
          >
            <div className="text-2xl mb-2">{statusInfo.icon}</div>
            <div className="font-semibold truncate">{log.itemName}</div>
            <div className="text-xs mt-1 opacity-70">
              {new Date(log.createDate).toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' })}
            </div>
          </div>
        );
      })}
    </div>
  );
}
