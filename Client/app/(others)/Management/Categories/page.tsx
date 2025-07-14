// app/categories/page.tsx
"use client";

import { useEffect, useState, useRef, useActionState } from "react";
// import { useActionState } from "react-dom";
import {
  getCategoriesAction,
  addCategoryAction,
} from "./actions"; // اکشن‌ها را از فایل جداگانه وارد کنید
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import Swal from "sweetalert2";

// کامپوننت دکمه برای نمایش وضعیت pending
function SubmitButton() {
    // نکته: useFormStatus باید مستقیما فرزند فرم باشد.
    // برای سادگی، فعلا دکمه را بدون وضعیت pending قرار می‌دهیم.
    // می‌توانید این بخش را بعدا با useFormStatus بهینه کنید.
  return <button type="submit" className="btn btn-success">ثبت</button>;
}


export default function CategoriesPage() {
  const [categories, setCategories] = useState<any>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const formRef = useRef<HTMLFormElement>(null);

  // ۱. اتصال فرم به Server Action با استفاده از useActionState
  const [state, formAction] = useActionState(addCategoryAction, undefined);

  // ۲. تابع برای واکشی اولیه دسته‌بندی‌ها
  const loadCategories = async () => {
    setIsLoading(true);
    // فراخوانی Server Action برای دریافت داده‌ها
    const result = await getCategoriesAction();
    console.log("Categories fetched:", result);
    if (result.success) {
      setCategories(result.data.data || []);
    } else {
      Swal.fire("خطا", result.error?.message || "خطا در دریافت اطلاعات", "error");
    }
    setIsLoading(false);
  };

  // ۳. واکشی داده‌ها در اولین بارگذاری
  useEffect(() => {
    loadCategories();
  }, []);

  // ۴. مدیریت نتیجه بازگشتی از فرم و به‌روزرسانی UI
  useEffect(() => {
    if (state?.success) {
      Swal.fire("موفق!", "دسته‌بندی با موفقیت افزوده شد.", "success");
      setIsModalOpen(false);
      formRef.current?.reset();
      // به جای فراخوانی مجدد، به revalidatePath در اکشن تکیه می‌کنیم.
      // برای مشاهده فوری تغییر، می‌توانیم لیست را به صورت دستی آپدیت کنیم یا دوباره fetch کنیم.
      loadCategories(); // ساده‌ترین راه برای دیدن فوری تغییرات
    } else if (state && state.error) {
      Swal.fire("خطا", state.error.message, "error");
    }
  }, [state]);

  return (
    <div className="p-8 bg-base-300 min-h-screen">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">مدیریت دسته‌بندی‌ها</h1>
        <button
          className="btn btn-success"
          onClick={() => setIsModalOpen(true)}
        >
          افزودن دسته‌بندی
        </button>
      </div>

      {isLoading ? (
        <p>در حال بارگذاری...</p>
      ) : categories.length > 0 ? (
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow >
                <TableHead className="text-start">نام دسته‌بندی</TableHead>
                <TableHead className="text-start">تاریخ ایجاد</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {categories.map((cat) => (
                <TableRow key={cat.id}>
                  <TableCell>{cat.categoryName}</TableCell>
                  <TableCell>
                    {new Date(cat.createDate).toLocaleDateString("fa-IR")}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      ) : (
        <p className="text-gray-400 mt-4">دسته‌بندی‌ای یافت نشد.</p>
      )}

      {isModalOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-70 flex justify-center items-center z-50">
          <div className="bg-[#1f1f1f] p-8 rounded-2xl shadow-2xl w-full max-w-md">
            <h2 className="text-xl font-bold mb-4 text-center">
              افزودن دسته‌بندی جدید
            </h2>
            <form action={formAction} ref={formRef} className="flex flex-col gap-4">
              <input
                type="text"
                name="categoryName"
                placeholder="نام دسته‌بندی"
                className="input input-bordered bg-[#2a2a2a]"
                required
              />
              <div className="flex justify-between mt-6">
                <SubmitButton />
                <button
                  type="button"
                  className="btn btn-error"
                  onClick={() => setIsModalOpen(false)}
                >
                  بستن
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}