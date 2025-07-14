"use client";
import { useState } from "react";
import { signIn } from "next-auth/react";
import Swal from "sweetalert2";
const login = () => {
  const [phoneNumber, setPhoneNumber] = useState("");
  const [password, setPassword] = useState("");
  const [errorMessage, setErrorMessage] = useState(""); // State to hold the error message
  const [isSubmitting, setIsSubmitting] = useState(false);
  const handleSubmit = async (e: any) => {
    e.preventDefault();

    // Form validation
    if (!phoneNumber || !password) {
      setErrorMessage("شماره تلفن یا رمز عبور نمیتواند خالی باشد.");
      return;
    }
    setErrorMessage("");
    setIsSubmitting(true);

    try {
      const result = await signIn("credentials", {
        redirect: false,
        PhoneNumber: phoneNumber,
        PassWord: password,
      });

      console.log(result);
      if (result?.error) {
        try {
          const errorData = JSON.parse(result.error);
          setErrorMessage("خطا در ورود به سیستم");
          Swal.fire("خطا", "خطا در ورود به سیستم", "error");
        } catch {
          // If error is not parseable JSON
          setErrorMessage("خطا در ورود به سیستم");
          Swal.fire("خطا", "خطا در ورود به سیستم", "error");
        }
      } else if (result?.ok) {
        Swal.fire("موفق", "ورود با موفقیت انجام شد.", "success");

        // Redirect after successful login
        setTimeout(() => {
          window.location.href = "/Monitoring";
        }, 1500);
      }
    } catch (error) {
      console.log(error);
      setErrorMessage("خطا در ارتباط با سرور");
      Swal.fire("خطا", "خطا در ارتباط با سرور", "error");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div>
      <div className="relative pb-6 pt-10 mt-10 px-4 mb-8 overflow-hidden rounded-lg bg-gradient-to-br to-emerald-500 from-blue-800">
        {/* شکل‌های تزئینی در پس‌زمینه */}
        <div className="absolute top-0 left-0 w-full h-full opacity-10">
          <div className="absolute top-4 right-8 w-24 h-24 rounded-full bg-blue-400"></div>
          <div className="absolute bottom-6 left-10 w-16 h-16 rounded-full bg-indigo-500"></div>
          <div className="absolute top-1/2 left-1/4 w-12 h-12 rounded-full bg-cyan-300"></div>
        </div>

        {/* خطوط اتصال تزئینی */}
        <div className="absolute inset-0 opacity-20">
          <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-cyan-300 to-transparent"></div>
          <div className="absolute bottom-0 right-0 w-full h-1 bg-gradient-to-l from-blue-300 to-transparent"></div>
        </div>

        <h1 className="text-xl lg:text-4xl font-bold text-center text-white relative z-10 drop-shadow-lg">
          <span className="inline-block py-2 relative">
            سامانه هوشمند پشتیبانی و مانیتورینگ شرکت توسعه سازه های اطلاعاتی
            نوین مهراب
          </span>
        </h1>
        <div className="flex justify-center mt-2">
          <span className="px-3 py-1 text-xs bg-blue-800 text-cyan-300 rounded-full shadow-inner">
            نسخه 1.0
          </span>
        </div>
      </div>
      <div className="flex items-center justify-center min-h-[400] rounded-md ">
        <form
          onSubmit={handleSubmit}
          className="w-full min-w-full p-8 space-y-4 rounded-lg  bg-gradient-to-br from-emerald-500 to-blue-800 shadow-2xl"
        >
          <div className="text-xl lg:text-3xl font-bold text-center text-white">
            ورود به حساب کاربری
          </div>
          {errorMessage && (
            <div
              className="p-3 mb-4 text-sm text-red-700 bg-red-100 rounded-lg"
              role="alert"
            >
              {errorMessage}
            </div>
          )}
          <div>
            <label
              htmlFor="PhoneNumber"
              className="block text-base lg:text-xl pb-1 font-medium text-white"
            >
              تلفن همراه:
            </label>
            <input
              type="text"
              id="PhoneNumber"
              value={phoneNumber}
              onChange={(e) => setPhoneNumber(e.target.value)}
              className="mt-1 block text-black w-full px-3 py-2 bg-gray-50 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
            />
          </div>
          <div>
            <label
              htmlFor="password"
              className="block text-base lg:text-xl pb-1 font-medium text-white"
            >
              رمز عبور:
            </label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="mt-1 block text-black w-full px-3 py-2 bg-gray-50 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
            />
          </div>
          <button
            type="submit"
            disabled={isSubmitting} // Disable button based on isSubmitting state
            className={`block w-full px-4 py-2 text-base lg:text-2xl font-medium text-white bg-indigo-500 rounded-md ${
              isSubmitting ? "bg-indigo-300" : "hover:bg-indigo-700"
            } focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500`}
          >
            {isSubmitting ? "در حال بررسی ..." : "ورود"} {}
          </button>
          <div className="border-b-4 border-blue-600 border-dotted"></div>
        </form>
      </div>
    </div>
  );
};
export default login;
