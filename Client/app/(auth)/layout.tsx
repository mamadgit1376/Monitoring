

import Header from "@/components/Header";
import type { Metadata } from "next";
import { SessionProvider } from "next-auth/react";
export const metadata: Metadata = {
  title: "Create Next App",
  description: "Generated by create next app",
};
export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="fa" dir="rtl" className="bg-[#002344] ">
      <body
 className="antialiased font-[IRANSans] "
  >
      <SessionProvider>
        <div className="container mt-4 w-11/12 mx-auto ">
          <main>{children}</main>
        </div>{" "}
      </SessionProvider>
      </body>
      </html>
  );
}
