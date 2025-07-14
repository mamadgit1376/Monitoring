// page.jsx
"use client";

import { changeUserPasswordAction } from "./actions";
import Image from "next/image";
import { useActionState, useState } from "react"; 
import { useFormStatus } from "react-dom";
// کامپوننت داخلی برای مدیریت وضعیت دکمه ارسال
function SubmitButton() {
  const { pending } = useFormStatus();
  return (
    <button type="submit" className="btn btn-primary w-full" disabled={pending}>
      {pending ? (
        <>
          <span className="loading loading-spinner"></span>
          <span>در حال بررسی...</span>
        </>
      ) : (
        "تغییر رمز عبور"
      )}
    </button>
  );
}

export default function ChangePasswordPage() {
  const initialState = { success: false, message: "" };
  const [state, formAction] = useActionState(
  changeUserPasswordAction,
  initialState
);
  const [showAnimation, setShowAnimation] = useState(false);
  function handleDoubleClick() {
    setShowAnimation((prev) => !prev);
  }
  return (
    <div
      className="min-h-screen bg-base-200 flex items-center justify-center p-4"
      dir="rtl"
    >
      <div className="card w-full max-w-md bg-base-100 shadow-xl">
        <div className="card-body">
          <h2 className="card-title justify-center text-2xl mb-6">
            تغییر رمز عبور
          </h2>

          <form action={formAction} className="space-y-4">
            {/* فیلد رمز عبور فعلی */}
            <div className="form-control">
              <label className="label" htmlFor="oldPassword">
                <span className="label-text">رمز عبور فعلی</span>
              </label>
              <input
                id="oldPassword"
                name="oldPassword"
                type="password"
                required
                className="input input-bordered w-full"
              />
            </div>

            {/* فیلد رمز عبور جدید */}
            <div className="form-control">
              <label className="label" htmlFor="newPassword">
                <span className="label-text">رمز عبور جدید</span>
              </label>
              <input
                id="newPassword"
                name="newPassword"
                type="password"
                required
                className="input input-bordered w-full"
              />
            </div>

            {/* فیلد تکرار رمز عبور جدید */}
            <div className="form-control">
              <label className="label" htmlFor="rNewPassword">
                <span className="label-text">تکرار رمز عبور جدید</span>
              </label>
              <input
                id="rNewPassword"
                name="rNewPassword"
                type="password"
                required
                className="input input-bordered w-full"
              />
            </div>

            {/* نمایش پیام‌های وضعیت */}
            {state?.message && (
              <div
                className={`alert ${
                  state.success ? "alert-success" : "alert-error"
                } text-sm`}
              >
                <span>{state.message}</span>
              </div>
            )}

            {/* دکمه ارسال فرم */}
            <div className="form-control pt-4">
              <SubmitButton />
            </div>
          </form>
        </div>
      </div>
      {showAnimation && (
        <Image
          alt="Dance"
          className="rounded-xl"
          src="/images/ignore/dance.gif" // 1. پسوند فایل اصلاح شد
          width={800} // 2. عرض تصویر را وارد کنید
          height={600} // 2. ارتفاع تصویر را وارد کنید
          unoptimized={true} // 3. برای جلوگیری از بهینه‌سازی و حفظ انیمیشن
        />
      )}
      <button className="w-2 h-2   absolute top-0 left-0" onDoubleClick={handleDoubleClick}> </button>
    </div>
  );
}
