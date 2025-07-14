// app/(others)/CompanyLogs/company-logs-client.tsx
"use client";

import { useEffect, useState } from "react";
import { DatePicker } from "@/components/intentui/date-picker";
import { today, CalendarDate } from "@internationalized/date";
import { SelectNative } from "@/components/ui/select-native";
import MultipleSelector from "@/components/ui/multiselect";
// Import actions and types from the new actions file
import {
  fetchLogAndFunctionCombosAction,
  fetchLogsAction,
  LogsModel,
  FilteredLogsModel,
  CompanyOption,
} from "./actions";

interface CompanyLogsClientProps {
  initialCompanies: CompanyOption[];
}

export default function CompanyLogsClient({ initialCompanies }: CompanyLogsClientProps) {
  const todayPersian = today("Asia/Tehran");

  const [companyLogs, setCompanyLogs] = useState<LogsModel[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [companyOptions] = useState<CompanyOption[]>(initialCompanies);
  const [logTypeOptions, setLogTypeOptions] = useState<{ value: string; label: string }[]>([]);
  const [functionOptions, setFunctionOptions] = useState<{ value: string; label: string }[]>([]);

  const defaultSelectedLogTypeValues = ["13", "26", "408"];
  const [filters, setFilters] = useState<FilteredLogsModel>({
    companyId: initialCompanies.length > 0 ? Number(initialCompanies[0].value) : null,
    filterDate: formatDateForApi(todayPersian),
    fromTime: "",
    toTime: "",
    filterIp: "",
    filterText: "",
    enteredUsedIds: "",
    filteredUsedIds: "",
    logTypeCodes: defaultSelectedLogTypeValues.join(","),
    functionIds: "",
  });

  function formatDateForApi(date: CalendarDate | null): string {
    if (!date) return "";
    return `${date.year}-${String(date.month).padStart(2, "0")}-${String(
      date.day
    ).padStart(2, "0")}`;
  }

  const fetchLogAndFunctionCombos = async (companyId: number) => {
    try {
      const result = await fetchLogAndFunctionCombosAction(companyId);

      if (!result.success) {
        console.error("Error fetching combos:", result.error);
        return;
      }

      if (result.data) {
        const mappedLogTypes = result.data.typeLogs.map((item) => ({
          value: item.id?.toString() || "",
          label: item.name || "",
        }));
        const mappedFunctions = result.data.functionLogs.map((item) => ({
          value: item.id?.toString() || "",
          label: item.name || "",
        }));

        setLogTypeOptions(mappedLogTypes);
        setFunctionOptions(mappedFunctions);

        const defaultLogTypes = mappedLogTypes.filter((opt) =>
          defaultSelectedLogTypeValues.includes(opt.value)
        );
        setFilters((prevFilters) => ({
          ...prevFilters,
          logTypeCodes: defaultLogTypes.map((opt) => opt.value).join(","),
        }));
      }
    } catch (err) {
      console.error("Error fetching log and function combos:", err);
    }
  };

  const fetchCompanyLogs = async () => {
    if (!filters.companyId) {
      setError("لطفاً ابتدا شرکت را انتخاب کنید");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const result = await fetchLogsAction(filters);

      if (!result.success) {
        setError(result.error || "خطا در دریافت داده‌ها");
        setCompanyLogs([]);
        return;
      }

      setCompanyLogs(result.data || []);
    } catch (err: any) {
      setError(err?.message || "خطا در دریافت داده‌ها");
      setCompanyLogs([]);
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (name: string, value: any) => {
    setFilters((prev) => ({ ...prev, [name]: value }));
  };

  const applyFilters = () => {
    fetchCompanyLogs();
  };

  const formatDate = (dateString?: string | null) => {
    if (!dateString) return "-";
    if (dateString.length === 10 && dateString.includes("-")) {
      const [year, month, day] = dateString.split("-");
      return `${year}/${month}/${day}`;
    }
    return dateString;
  };

  const formatTime = (timeString?: string | null) => {
    if (!timeString) return "-";
    if (timeString.includes(":")) {
      return timeString;
    }
    return timeString;
  };

  // Load initial combos when company changes
  useEffect(() => {
    if (filters.companyId) {
      fetchLogAndFunctionCombos(filters.companyId);
    }
  }, [filters.companyId]);

  // The rest of your JSX remains exactly the same...
  return (
     <>
      <div className="card bg-base-300 text-base-content shadow-sm">
        <div className="card-body">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* Company Select */}
            <div className="form-control">
              <label className="label">
                <span className="label-text text-base-content">
                  شرکت: <span className="text-red-500">*</span>
                </span>
              </label>
              <SelectNative
                value={filters.companyId?.toString() || ""}
                onChange={(e) =>
                  handleFilterChange(
                    "companyId",
                    e.target.value ? parseInt(e.target.value) : null
                  )
                }
                className={!filters.companyId ? "select-error" : ""}
              >
                <option value="">انتخاب شرکت</option>
                {companyOptions.map((company, idx) => (
                  <option
                    key={`${company.value}-${idx}`}
                    value={company.value}
                  >
                    {company.label}
                  </option>
                ))}
              </SelectNative>
              {!filters.companyId && (
                <label className="label">
                  <span className="label-text-alt text-red-500">
                    انتخاب شرکت اجباری است
                  </span>
                </label>
              )}
            </div>

            {/* Date Picker */}
            <div className="form-control">
              <DatePicker
                label="تاریخ:"
                defaultValue={todayPersian}
                onChange={(value) => {
                  if (value) {
                    handleFilterChange(
                      "filterDate",
                      formatDateForApi(value)
                    );
                  }
                }}
              />
            </div>

            {/* From Time */}
            <div className="form-control">
              <label className="label">
                <span className="label-text text-base-content">
                  از ساعت:
                </span>
              </label>
              <input
                type="time"
                className="input input-bordered w-full"
                value={filters.fromTime || ""}
                onChange={(e) =>
                  handleFilterChange("fromTime", e.target.value)
                }
              />
            </div>

            {/* To Time */}
            <div className="form-control">
              <label className="label">
                <span className="label-text text-base-content">
                  تا ساعت:
                </span>
              </label>
              <input
                type="time"
                className="input input-bordered w-full"
                value={filters.toTime || ""}
                onChange={(e) =>
                  handleFilterChange("toTime", e.target.value)
                }
              />
            </div>

            {/* IP Address */}
            <div className="form-control">
              <label className="label">
                <span className="label-text text-base-content">
                  آدرس IP:
                </span>
              </label>
              <input
                type="text"
                className="input input-bordered w-full"
                placeholder="مثال: 192.168.1.1"
                value={filters.filterIp || ""}
                onChange={(e) =>
                  handleFilterChange("filterIp", e.target.value)
                }
              />
            </div>

            {/* Search Text */}
            <div className="form-control">
              <label className="label">
                <span className="label-text text-base-content">
                  متن جستجو:
                </span>
              </label>
              <input
                type="text"
                className="input input-bordered w-full"
                placeholder="جستجو در توضیحات..."
                value={filters.filterText || ""}
                onChange={(e) =>
                  handleFilterChange("filterText", e.target.value)
                }
              />
            </div>
            
            {/* Log Type Multiselect */}
            <div className="form-control">
              <label className="label">
                <span className="label-text text-base-content">
                  نوع عملیات:
                </span>
              </label>
              {logTypeOptions?.length > 0 && (
                <MultipleSelector
                  loading={loading}
                  defaultOptions={logTypeOptions}
                  hideClearAllButton
                  hidePlaceholderWhenSelected
                  emptyIndicator={<p className="text-center text-sm">No results found</p>}
                  value={
                    filters.logTypeCodes
                      ? logTypeOptions.filter((opt) =>
                          filters.logTypeCodes!.split(",").includes(opt.value)
                        )
                      : []
                  }
                  onChange={(
                    selectedOptions: { value: string; label: string }[]
                  ) => {
                    handleFilterChange(
                      "logTypeCodes",
                      selectedOptions.map((opt) => opt.value).join(",")
                    );
                  }}
                />
              )}
            </div>
            
            {/* Function Name Multiselect */}
            <div className="form-control">
              <label className="label">
                <span className="label-text text-base-content">نام تابع:</span>
              </label>
              {functionOptions?.length > 0 && (
                <MultipleSelector
                  loading={loading}
                  defaultOptions={functionOptions}
                  hideClearAllButton
                  hidePlaceholderWhenSelected
                  emptyIndicator={<p className="text-center text-sm">No results found</p>}
                  value={
                    filters.functionIds
                      ? functionOptions.filter((opt) =>
                          filters.functionIds!.split(",").includes(opt.value)
                        )
                      : []
                  }
                  onChange={(
                    selectedOptions: { value: string; label: string }[]
                  ) => {
                    handleFilterChange(
                      "functionIds",
                      selectedOptions.map((opt) => opt.value).join(",")
                    );
                  }}
                />
              )}
            </div>

            {/* User ID Input */}
            <div className="form-control">
              <label className="label">
                <span className="label-text text-base-content">
                  شناسه کاربر:
                </span>
              </label>
              <input
                type="text"
                className="input input-bordered w-full"
                placeholder="مثال: 1,2,3"
                value={filters.enteredUsedIds || ""}
                onChange={(e) =>
                  handleFilterChange("enteredUsedIds", e.target.value)
                }
              />
            </div>
          </div>

          <div className="mt-6 flex justify-end">
            <button
              className={`btn ${
                !filters.companyId ? "btn-disabled" : "btn-outline"
              }`}
              onClick={applyFilters}
              disabled={loading || !filters.companyId}
            >
              {loading ? "در حال بارگذاری..." : "جستجو"}
            </button>
          </div>
        </div>
      </div>

      {error && (
        <div className="alert alert-error mt-4">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            className="stroke-current shrink-0 h-6 w-6"
            fill="none"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
              d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
          <span>{error}</span>
        </div>
      )}

      <div className="overflow-x-auto mt-6">
        <table className="table w-full bg-base-300">
          <thead>
            <tr>
              <th>تاریخ ایجاد</th>
              <th>ساعت شروع</th>
              <th>ساعت پایان</th>
              <th>زمان انجام (ثانیه)</th>
              <th>کاربر</th>
              <th>عملیات انجام شده</th>
              <th>توضیحات</th>
              <th>نتیجه عملیات</th>
              <th>آدرس IP</th>
              <th>پیام ایجاد شده</th>
              <th>نام تابع</th>
              <th>نام کلاس</th>
              <th>خطای فنی</th>
              <th>جزئیات</th>
              <th>آدرس مرورگر</th>
              <th>مروگر و سیستم عامل</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={16} className="text-center py-4">
                  <div className="flex justify-center items-center">
                    <div className="loading loading-spinner"></div>
                    <span className="mr-2">در حال بارگذاری...</span>
                  </div>
                </td>
              </tr>
            ) : companyLogs.length === 0 ? (
              <tr>
                <td colSpan={16} className="text-center py-4">
                  {!filters.companyId
                    ? "لطفاً شرکت را انتخاب کرده و دکمه جستجو را بزنید"
                    : "هیچ لاگی یافت نشد"}
                </td>
              </tr>
            ) : (
              companyLogs.map((log, idx) => (
                <tr key={idx} className="hover:bg-base-200">
                  <td>{formatDate(log.addDate)}</td>
                  <td>{formatTime(log.addTime)}</td>
                  <td>{formatTime(log.finishTime)}</td>
                  <td>{log.time || "-"}</td>
                  <td>{log.nameOfUser || "-"}</td>
                  <td>{log.functionDescription || "-"}</td>
                  <td>{log.logType || "-"}</td>
                  <td>{log.ip || "-"}</td>
                  <td>
                    <span className="truncate max-w-xs block">
                      {log.message?.length && log.message.length > 50
                        ? log.message.substring(0, 50) + "..."
                        : log.message || "-"}
                    </span>
                  </td>
                  <td>{log.functionName || "-"}</td>
                  <td>{log.className || "-"}</td>
                  <td>
                    <span className="truncate max-w-xs block">
                      {log.stackTrace?.length && log.stackTrace.length > 50
                        ? log.stackTrace.substring(0, 50) + "..."
                        : log.stackTrace || "-"}
                    </span>
                  </td>
                  <td>
                    <span className="truncate max-w-xs block">
                      {log.extraInfo?.length && log.extraInfo.length > 50
                        ? log.extraInfo.substring(0, 50) + "..."
                        : log.extraInfo || "-"}
                    </span>
                  </td>
                  <td>{log.browserAddress || "-"}</td>
                  <td>{log.agentName || "-"}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </>
  );
}