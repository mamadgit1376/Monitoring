import Image from "next/image";
import "@/app/globals.css";
import Login from "@/components/login";

export default function Home() {
  return (
    <div className="container block lg:flex mt-8 gap-6 bg-red">
      <div className="basis-1/2 lg:basis-full"><Login/></div>
      <div className="basis-1/2 lg:basis-full"><Image className="rounded-2xl" alt="" src="/images/monitoring2.png" width={2000} height={2000}></Image></div>
    </div>
  );
}
