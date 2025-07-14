import Link from "next/link";
// import HeaderDashboardsComponent from "./component/HeaderDashboardsComponent"
import Image from "next/image";
import { redirect } from "next/navigation";
import Logout from "@/components/logout";
import HeaderDashboardsComponent from "./HeaderDashboardsComponent";
import { auth } from "@/auth";
import ThemeToggleButton from "./ThemeToggleButton";
export default async function header() {
  const session = await auth();
  const UserType: string = session?.user?.role ?? "User";
  return (
    <header className="pt-4 mb-8 flex justify-between items-center border-b-2 border-text">
      <div className="md:hidden block">
        <HeaderDashboardsComponent />
      </div>
      <nav className="flex space-x-4">
        {(UserType == "HeadAdmin" || UserType == "Admin") && (
          <div className="hidden gap-4 md:flex ">
            {/* <Link href="/AdminDashboard">
              <button className=" text-sm lg:text-xl py-2 btn btn-ghost  hover:scale-105 transition-transform duration-200">
                داشبورد
              </button>
            </Link> */}
            <Link href="/Monitoring">
              <button className="text-sm lg:text-xl  py-2 btn btn-ghost hover:scale-105 transition-transform duration-200">
                مانیتورینگ
              </button>
            </Link>
            <Link href="/SystemManagement">
              <button className="text-sm lg:text-xl  py-2 btn btn-ghost hover:scale-105 transition-transform duration-200">
                مدیریت سیستم
              </button>
            </Link>

            <Link href="/SystemLogs">
              <button className="text-sm lg:text-xl  py-2 btn btn-ghost hover:scale-105 transition-transform duration-200">
                لاگ ها
              </button>
            </Link>
            <Link href="/CompanyLogs">
              <button className="text-sm lg:text-xl  py-2 btn btn-ghost hover:scale-105 transition-transform duration-200">
                لاگ استان ها
              </button>
            </Link>
            {/* <Link href="/TicketManagement">
              <button className="text-sm lg:text-xl  py-2 btn btn-ghost hover:scale-105 transition-transform duration-200">
                تیکت ها
              </button>
            </Link> */}
          </div>
        )}
        {UserType == "User" && (
          <div className="hidden gap-6 md:flex ">
             <Link href="/Monitoring">
              <button className="text-sm lg:text-xl  py-2 btn btn-ghost hover:scale-105 transition-transform duration-200">
                مانیتورینگ
              </button>
            </Link>
          </div>
        )}
        <div className="hidden gap-6 md:flex ">
             <Link href="/ChangePassword">
              <button className="text-sm lg:text-xl  py-2 btn btn-ghost hover:scale-105 transition-transform duration-200">
                تغییر رمز عبور
              </button>
            </Link>
          </div>
        
      </nav>
      <div className="flex justify-center items-center">
        <Logout />
        <ThemeToggleButton />
        <Link href="/Monitoring">
          <div className={` m-2 mb-4 rounded-md `}>
            <Image
              alt="logo"
              src={"/images/Image (1).png"}
              height={100}
              width={100}
            ></Image>
          </div>
        </Link>
      </div>
    </header>
  );
}
