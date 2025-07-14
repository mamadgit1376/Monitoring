"use client";
import React from "react";
import { useSession } from "next-auth/react";
import Link from "next/link";
import { ImMenu3 } from "react-icons/im";

function DashboardLinks() {
  const { data: session } = useSession();
  //@ts-ignore
  const role :string = session?.user?.role;

  const commonLinks = [
    // { href: "/api/auth/changepassword", label: "تغییر پسورد" },
  ];

  const roleLinks: Record<string, { href: string; label: string }[]> = {
    HeadAdmin: [
      { href: "/SystemLogs", label: "لاگ آیتم ها " },
      { href: "/SystemManagement", label: "مدیریت سیستم" },
      { href: "/Monitoring", label: "مانیتورینگ" },
      { href: "/CompanyLogs", label: "لاگ استانها" },
    ],
    Admin: [
      { href: "/SystemLogs", label: "لاگ آیتم ها " },
      { href: "/SystemManagement", label: "مدیریت سیستم" },
      { href: "/Monitoring", label: "مانیتورینگ" },
      { href: "/CompanyLogs", label: "لاگ استانها" },
    ],
    User: [
      { href: "/Monitoring", label: "مانیتورینگ" },
    ],
  };

  const allLinks = [...(roleLinks[role] || []), ...commonLinks];

  return (
    <div className="dropdown dropdown-start z-40">
      <label tabIndex={0} className="btn btn-ghost btn-circle">
        <ImMenu3 size={30} />
      </label>
      <ul
        tabIndex={0}
        className="menu dropdown-content mt-3 p-2 shadow bg-base-100 rounded-box"
      >
        {allLinks.map((link, index) => (
          <li key={index}>
            <Link href={link.href} className="justify-center text-center text-nowrap">
              {link.label}
            </Link>
          </li>
        ))}
        <li>
          <Link href="/ChangePassword" className="justify-center text-center text-nowrap">
            تغییر رمز عبور
          </Link>
        </li>
      </ul>
    </div>
  );
}

export default DashboardLinks;
