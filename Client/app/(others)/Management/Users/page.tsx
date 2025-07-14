// app/users/page.tsx
"use client";

import { useEffect, useState, useActionState } from "react";
import { useFormStatus } from "react-dom";
import {
  getUsersAction,
  addUserAction,
  resetPasswordAction,
  toggleUserStatusAction,
  UserModel,
} from "./actions";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import Swal from "sweetalert2";

function SubmitButton() {
  const { pending } = useFormStatus();
  return (
    <button type="submit" className="btn btn-accent" disabled={pending}>
      {pending ? "در حال ثبت..." : "ثبت"}
    </button>
  );
}

const initialFormData = {
  username: "",
  fullName: "",
  userRole: 0,
  companeyId: "",
};

export default function UsersPage() {
  const [users, setUsers] = useState<any>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  
  const [addUserResult, addUserFormAction] = useActionState(addUserAction, undefined);

  const loadUsers = async () => {
    setIsLoading(true);
    const result = await getUsersAction();
    if (result.success) {
      setUsers(result.data?.data || []);
    } else {
      Swal.fire("خطا", result.error?.message || "خطا در دریافت لیست کاربران", "error");
    }
    setIsLoading(false);
  };

  useEffect(() => {
    loadUsers();
  }, []);

  useEffect(() => {
    if (addUserResult?.success) {
      Swal.fire("موفق!", "کاربر با موفقیت افزوده شد.", "success");
      setIsModalOpen(false);
      loadUsers();
    } else if (addUserResult?.error) {
      Swal.fire("خطا", addUserResult.error.message, "error");
    }
  }, [addUserResult]);

  const handleResetPassword = async (userId: number) => {
    const confirm = await Swal.fire({
      title: "آیا از ریست پسورد مطمئن هستید؟",
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "بله، ریست کن",
      cancelButtonText: "انصراف",
    });

    if (confirm.isConfirmed) {
      const result = await resetPasswordAction(userId);
      console.log("Reset Password Result:", result);
      if (result.success) {
        Swal.fire("موفق!", result.data.message, "success");
      } else {
        Swal.fire("خطا", result.error?.message || "عملیات ناموفق بود", "error");
      }
    }
  };

  const handleToggleStatus = async (user: UserModel) => {
    const isEnabling = !!user.removeDate;
    const actionText = isEnabling ? "فعال" : "غیرفعال";
    
    const confirm = await Swal.fire({
        title: `آیا از ${actionText} کردن این کاربر مطمئن هستید؟`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: `بله، ${actionText} کن`,
        cancelButtonText: 'انصراف'
    });

    if (confirm.isConfirmed) {
        const result = await toggleUserStatusAction(user.id, isEnabling);
        if(result.success) {
            Swal.fire("موفق!", `کاربر با موفقیت ${actionText} شد.`, "success");
            loadUsers();
        } else {
            Swal.fire("خطا", result.error?.message || "عملیات ناموفق بود", "error");
        }
    }
  };

  return (
    <div className="p-8 min-h-screen bg-base-300 text-base-content">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">مدیریت کاربران سیستم</h1>
        <button
          className="btn btn-accent"
          onClick={() => setIsModalOpen(true)}
        >
          افزودن کاربر جدید
        </button>
      </div>

      {isLoading ? <p className="text-center">در حال بارگذاری...</p> : (
        <div className="mt-6 overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>نام کامل</TableHead>
                <TableHead>نام کاربری</TableHead>
                <TableHead>نقش</TableHead>
                <TableHead>شرکت</TableHead>
                <TableHead>وضعیت</TableHead>
                <TableHead>عملیات</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {users.map((user) => (
                <TableRow key={user.id}>
                  <TableCell>{user.fullName}</TableCell>
                  <TableCell>{user.userName}</TableCell>
                  <TableCell>{user.role}</TableCell>
                  <TableCell>{user.companies || "-"}</TableCell>
                  <TableCell>
                    <span className={`px-2 py-1 rounded-full text-xs font-semibold ${user.removeDate ? "bg-red-200 text-red-800" : "bg-green-200 text-green-800"}`}>
                      {user.removeDate ? "غیرفعال" : "فعال"}
                    </span>
                  </TableCell>
                  <TableCell>
                    <div className="flex gap-2">
                      <button className="btn btn-sm btn-warning" onClick={() => handleResetPassword(user.id)}>
                        ریست رمز
                      </button>
                      <button className={`btn btn-sm ${user.removeDate ? "btn-success" : "btn-error"}`} onClick={() => handleToggleStatus(user)}>
                        {user.removeDate ? "فعال کردن" : "غیرفعال کردن"}
                      </button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      {isModalOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-70 flex justify-center items-center z-50">
          <div className="bg-[#1f1f1f] p-8 rounded-2xl shadow-2xl w-full max-w-md text-white">
            <h2 className="text-2xl mb-6 font-bold text-center">فرم ثبت کاربر</h2>
            <form action={addUserFormAction} onSubmit={(e) => {
              // The form will be submitted via the action, but we can close the modal optimistically
              // Or wait for the result in the useEffect hook. Let's wait.
            }} className="flex flex-col gap-5">
              <div>
                <label className="block mb-2 font-semibold">شماره تلفن (یوزرنیم):</label>
                <input type="text" name="username" required minLength={11} maxLength={11} className="w-full bg-[#2a2a2a] border border-gray-600 rounded-xl p-3"/>
              </div>
              <div>
                <label className="block mb-2 font-semibold">نام کامل:</label>
                <input type="text" name="fullName" required className="w-full bg-[#2a2a2a] border border-gray-600 rounded-xl p-3"/>
              </div>
              <div>
                <label className="block mb-2 font-semibold">نقش کاربر:</label>
                <select name="userRole" defaultValue={0} className="w-full bg-[#2a2a2a] border border-gray-600 rounded-xl p-3">
                  <option value={0}>مدیر</option>
                  <option value={1}>کاربر</option>
                </select>
              </div>
              <div>
                <label className="block mb-2 font-semibold">کد شرکت (اختیاری):</label>
                <input type="number" name="companeyId" className="w-full bg-[#2a2a2a] border border-gray-600 rounded-xl p-3"/>
              </div>
              <div className="flex justify-between mt-8">
                <SubmitButton />
                <button type="button" onClick={() => setIsModalOpen(false)} className="btn btn-error">
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
