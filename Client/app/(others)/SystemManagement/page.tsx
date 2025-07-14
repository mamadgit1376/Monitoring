import Link from "next/link";
import { FaUserCog } from "react-icons/fa";
import { BsBuildingFillGear } from "react-icons/bs";
import { TbCategoryPlus } from "react-icons/tb";
import { FaSitemap } from "react-icons/fa6";

export default async function Page() {
  // Sample data for the cards
  const quickAccessItems = [
    {
      title: "مدیریت کاربران",
      description: "مدیریت کاربران و نقش‌ها",
      icon: <FaUserCog />,
      color: "bg-primary text-primary-content",
      linkAddress : "/Management/Users"
    },
    {
      title: "مدیریت شرکت‌ها",
      description: "مدیریت شرکت‌ها و اطلاعات آن‌ها",
      icon: <BsBuildingFillGear />,
      color: "bg-primary text-primary-content",
      linkAddress : "/Management/Companies"
    },
    {
      title: "مدیریت دسته‌بندی‌ها",
      description: "مدیریت دسته‌بندی‌ها و زیرمجموعه‌ها",
      icon: <TbCategoryPlus  />,
      color: "bg-primary text-white text-primary-content",
      linkAddress : "/Management/Categories"
    },
    {
      title: "مدیریت آیتم ها",
      description: "مدیریت آیتم‌هایی که در استان ها باید چک شوند",
      icon: <FaSitemap />,
      color: "bg-primary text-primary-content",
      linkAddress : "/Management/ItemCheck"
    },
  ];
  
  return (
    <div className="min-h-screen p-6">
      <h1 className="text-3xl mb-12 text-start">
        به <span className="text-primary">داشبورد مدیریتی</span> خوش آمدید
      </h1>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 ">
        {quickAccessItems.map((item, index) => (
          <Link
            key={index}
            className={` ${item.color}  p-6 rounded-lg shadow-lg flex flex-col items-center hover:scale-105 hover:shadow-lg hover:shadow-gray-500 transition hover:cursor-pointer`}
            href={item.linkAddress}
          >
            <div className=" text-4xl mb-4">
             {item.icon}
            </div>
            <h2 className="text-2xl   mb-2">
              {item.title}
            </h2>
            <p className=" text-sm text-center">{item.description}</p>

          </Link>
        ))}
      </div>
    </div>
  );
}
