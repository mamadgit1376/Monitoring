// app/item-logs/page.tsx
import {
  getFilterOptionsAction,
  getItemLogsAction,
} from "./actions";
import { ItemLogsClient } from "./ItemLogsClient";

// This is now a Server Component
export default async function ItemLogsPage({
  searchParams,
}: {
  searchParams: { [key: string]: string | string[] | undefined };
}) {
  try {
    // Fetch filter options and item logs concurrently on the server
    const [options, logsResult] = await Promise.all([
      getFilterOptionsAction(),
      getItemLogsAction({
        page: Number(searchParams.page) || 1,
        pageSize: Number(searchParams.pageSize) || 10,
        fromDate: searchParams.fromDate as string,
        toDate: searchParams.toDate as string,
        htmlStatus: searchParams.htmlStatus as string,
        companyCode: searchParams.companyCode as string,
        itemCode: searchParams.itemCode as string,
        statusItem: searchParams.statusItem as string,
      }),
    ]);

    // The entire interactive UI is now encapsulated in a Client Component
    return (
      <div className="p-4 bg-base-100 min-h-screen" dir="rtl">
        <div className="card bg-base-100 shadow-xl">
          <div className="card-body">
            <h2 className="card-title text-right text-base-content text-2xl mb-6">
              مشاهده لاگ آیتم‌ها
            </h2>
            <ItemLogsClient
              filterOptions={options}
              initialLogs={logsResult.logs}
              paginationData={logsResult.pagination}
            />
          </div>
        </div>
      </div>
    );
  } catch (error) {
    console.error('Error loading item logs:', error);
    return (
      <div className="p-4 text-center text-red-500">
        <h1>خطا در بارگذاری اطلاعات</h1>
        <p>لاگ‌ها یافت نشدند.</p>
      </div>
    );
  }
}