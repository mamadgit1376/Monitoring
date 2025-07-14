// app/item-logs/ItemLogsClient.tsx
"use client";

import { useState, useTransition } from "react";
import { useRouter, usePathname, useSearchParams } from "next/navigation";
import { DatePicker } from "@/components/intentui/date-picker";
import { today, CalendarDate } from "@internationalized/date";
import { SelectNative } from "@/components/ui/select-native";
import { ItemLogViewModel, FilterOption, PaginationInfo } from "./actions";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Label } from "@/components/ui/label";

const httpStatusOptions = [
  { label: "OK", value: "200" },
  { label: "Created", value: "201" },
  { label: "BadRequest", value: "400" },
  { label: "Unauthorized", value: "401" },
  { label: "NotFound", value: "404" },
  { label: "InternalServerError", value: "500" },
];

// Helper function to get status color
const getStatusColorClass = (statusType: number) => {
  switch (statusType) {
    case 0: return "text-error";   // Error
    case 1: return "text-success"; // Success
    case 2: return "text-warning"; // Warning
    default: return "";
  }
};

// Helper function to parse a yyyy-MM-dd string into a CalendarDate object
const parseDateString = (dateString: string): CalendarDate => {
    try {
        const [year, month, day] = dateString.split('-').map(Number);
        if (isNaN(year) || isNaN(month) || isNaN(day)) throw new Error("Invalid date parts");
        return new CalendarDate(year, month, day);
    } catch (e) {
        // Fallback to today if the string is invalid
        return today("Asia/Tehran");
    }
}

export function ItemLogsClient({
  filterOptions,
  initialLogs,
  paginationData,
}: {
  filterOptions: {
    companies: FilterOption[];
    items: FilterOption[];
    statuses: FilterOption[];
  };
  initialLogs: ItemLogViewModel[];
  paginationData: PaginationInfo;
}) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [isPending, startTransition] = useTransition();

  const [filters, setFilters] = useState({
    fromDate: searchParams.get("fromDate") || today("Asia/Tehran").subtract({ days: 30 }).toString(),
    toDate: searchParams.get("toDate") || today("Asia/Tehran").toString(),
    htmlStatus: searchParams.get("htmlStatus") || "",
    companyCode: searchParams.get("companyCode") || "",
    itemCode: searchParams.get("itemCode") || "",
    statusItem: searchParams.get("statusItem") || "",
    pageSize: searchParams.get("pageSize") || "10",
  });

  const handleFilterChange = (name: string, value: string | CalendarDate) => {
    setFilters((prev) => ({ ...prev, [name]: value.toString() }));
  };

  const handleNavigation = (newParams: URLSearchParams) => {
    startTransition(() => {
      router.push(`${pathname}?${newParams.toString()}`);
    });
  };

  const applyFilters = () => {
    const newParams = new URLSearchParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value) newParams.set(key, value);
    });
    newParams.set("page", "1");
    handleNavigation(newParams);
  };

  const handlePageChange = (newPage: number) => {
    const newParams = new URLSearchParams(searchParams.toString());
    newParams.set("page", String(newPage));
    handleNavigation(newParams);
  };

  const handlePageSizeChange = (newSize: string) => {
    const newParams = new URLSearchParams(searchParams.toString());
    newParams.set("pageSize", newSize);
    newParams.set("page", "1"); // Reset to first page
    handleFilterChange("pageSize", newSize); // Update local state as well
    handleNavigation(newParams);
  }

  // Formatting functions
  const formatDate = (dateString: string) => new Date(dateString).toLocaleDateString('fa-IR');
  const formatTime = (dateString: string) => new Date(dateString).toLocaleTimeString('fa-IR');

  // Pagination logic based on actual data received
  const currentPage = paginationData?.page || 1;
  const pageSize = paginationData?.pageSize || parseInt(filters.pageSize);
  const dataCount = initialLogs.length;
  
  // If we received exactly the page size, there might be more data
  // If we received less than page size, we're probably on the last page
  const hasNextPage = dataCount === pageSize;
  const hasPrevPage = currentPage > 1;

  return (
    <>
      {/* Filters Section */}
      <div className="card bg-base-300 text-base-content shadow-sm mb-6">
        <div className="card-body">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <div className="form-control">
              <Label>شرکت:</Label>
              <SelectNative value={filters.companyCode} onChange={(e) => handleFilterChange("companyCode", e.target.value)}>
                <option value="">همه</option>
                {filterOptions.companies.map((c) => (<option key={c.value} value={c.value}>{c.label}</option>))}
              </SelectNative>
            </div>
            <div className="form-control">
              <Label>از تاریخ:</Label>
              <DatePicker defaultValue={parseDateString(filters.fromDate)} onChange={(v) => v && handleFilterChange("fromDate", v)} />
            </div>
            <div className="form-control">
              <Label>تا تاریخ:</Label>
              <DatePicker defaultValue={parseDateString(filters.toDate)} onChange={(v) => v && handleFilterChange("toDate", v)} />
            </div>
            <div className="form-control">
              <Label>وضعیت ارتباط:</Label>
              <SelectNative value={filters.htmlStatus} onChange={(e) => handleFilterChange("htmlStatus", e.target.value)}>
                <option value="">همه</option>
                {httpStatusOptions.map((s) => (<option key={s.value} value={s.value}>{s.label}</option>))}
              </SelectNative>
            </div>
            <div className="form-control">
              <Label>انتخاب آیتم:</Label>
              <SelectNative value={filters.itemCode} onChange={(e) => handleFilterChange("itemCode", e.target.value)}>
                <option value="">همه</option>
                {filterOptions.items.map((i) => (<option key={i.value} value={i.value}>{i.label}</option>))}
              </SelectNative>
            </div>
            <div className="form-control">
              <Label>انتخاب وضعیت پاسخ:</Label>
              <SelectNative value={filters.statusItem} onChange={(e) => handleFilterChange("statusItem", e.target.value)}>
                <option value="">همه</option>
                {filterOptions.statuses.map((s) => (<option key={s.value} value={s.value}>{s.label}</option>))}
              </SelectNative>
            </div>
          </div>
          <div className="mt-6 flex justify-end">
            <button className="btn btn-outline" onClick={applyFilters} disabled={isPending}>
              {isPending ? "جستجو..." : "جستجو"}
            </button>
          </div>
        </div>
      </div>
      
      <div className="flex justify-end mb-4">
          <div className="form-control w-full max-w-xs">
              <Label>تعداد در صفحه:</Label>
              <SelectNative value={filters.pageSize} onChange={(e) => handlePageSizeChange(e.target.value)}>
                  <option value="10">10</option>
                  <option value="25">25</option>
                  <option value="50">50</option>
                  <option value="100">100</option>
              </SelectNative>
          </div>
      </div>

      {/* Results Table */}
      <div className={`overflow-x-auto relative ${isPending ? 'opacity-50' : ''}`}>
        {isPending && (
            <div className="absolute inset-0 flex items-center justify-center bg-base-100 bg-opacity-50 z-10">
                <span className="loading loading-spinner loading-lg"></span>
            </div>
        )}
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>نام آیتم</TableHead>
              <TableHead>نام شرکت</TableHead>
              <TableHead>تاریخ</TableHead>
              <TableHead>زمان</TableHead>
              <TableHead>عنوان</TableHead>
              <TableHead>جزئیات</TableHead>
              <TableHead>وضعیت ارتباط</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {initialLogs.length === 0 ? (
              <TableRow><TableCell colSpan={7} className="text-center py-10">هیچ لاگی یافت نشد</TableCell></TableRow>
            ) : (
              initialLogs.map((log) => (
                <TableRow key={log.id}>
                  <TableCell>{log.itemName}</TableCell>
                  <TableCell>{log.companyName}</TableCell>
                  <TableCell>{formatDate(log.createDate)}</TableCell>
                  <TableCell>{formatTime(log.createDate)}</TableCell>
                  <TableCell>{log.statusName}</TableCell>
                  <TableCell className={getStatusColorClass(log.statusType)}>
                    {log.statusDescription}
                  </TableCell>
                  <TableCell>{log.responseHtmlStatus}</TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      <div className="flex justify-between items-center mt-6">
        <div className="flex items-center gap-4">
          <span className="text-base-content text-sm">
            صفحه {currentPage} ({dataCount} آیتم در این صفحه)
          </span>
          <div className="join">
            <button 
              className="join-item btn btn-sm" 
              onClick={() => handlePageChange(currentPage - 1)} 
              disabled={!hasPrevPage || isPending}
            >
              صفحه قبل
            </button>
            <button 
              className="join-item btn btn-sm" 
              onClick={() => handlePageChange(currentPage + 1)} 
              disabled={!hasNextPage || isPending}
            >
              صفحه بعد
            </button>
          </div>
        </div>
      </div>
    </>
  );
}