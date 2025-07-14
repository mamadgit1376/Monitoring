import NextAuth from "next-auth";
import CredentialsProvider from "next-auth/providers/credentials";
import { JWT } from "next-auth/jwt";

// Extend the built-in types
declare module "next-auth" {
  interface Session {
    user: {
      id?: string;
      phoneNumber?: string;
      role?: string;
      name?: string | null;
      email?: string | null;
      image?: string | null;
    };
    accessToken?: string;
    refreshToken?: string; // <-- اضافه شده
    error?: string;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    phoneNumber?: string;
    role?: string;
    accessToken?: string;
    refreshToken?: string; // <-- اضافه شده
    error?: string;
  }
}

export const { handlers, signIn, signOut, auth } = NextAuth({
  pages: {
    signIn: "/",
  },
  session: {
    strategy: "jwt",
    maxAge: 30 * 24 * 60 * 60, // 30 days
  },
  callbacks: {
    async jwt({ token, user, account, trigger }) {
      // در زمان لاگین اولیه یا زمانی که session آپدیت می‌شود
      if (user) {
        // @ts-ignore
        token.phoneNumber = user.phoneNumber;
        // @ts-ignore
        token.role = user.role;
        // @ts-ignore
        token.accessToken = user.token;
        // @ts-ignore
        token.refreshToken = user.refreshToken;
      }
      return token;
    },
    async session({ session, token }) {
      if (token) {
        session.user.phoneNumber = token.phoneNumber;
        session.user.role = token.role;
        session.accessToken = token.accessToken;
        // @ts-ignore
        session.refreshToken = token.refreshToken;
        if (token.error) {
          session.error = token.error;
        }
      }
      return session;
    },
  },
  providers: [
    CredentialsProvider({
      name: "mehrab",
      credentials: {
        PhoneNumber: { label: "PhoneNumber", type: "text" },
        PassWord: { label: "PassWord", type: "password" },
        // فیلدهای مجازی برای آپدیت کردن توکن
        accessToken: { label: "Access Token", type: "text" },
        refreshToken: { label: "Refresh Token", type: "text" },
        role: { label: "Role", type: "text" },
      },//@ts-ignore
      async authorize(credentials) {
        // --- بخش کلیدی اصلاح شده ---
        // حالت 1: آپدیت کردن session با توکن‌های جدید
        // اگر توکن‌ها مستقیماً پاس داده شدند، یعنی در حال آپدیت هستیم.
        if (credentials.accessToken && credentials.refreshToken) {
          console.log("Authorizing session update...");
          return {
            id: credentials.PhoneNumber, // یا هر ID دیگری که از session قبلی دارید
            phoneNumber: credentials.PhoneNumber,
            role: credentials.role,
            token: credentials.accessToken,
            refreshToken: credentials.refreshToken,
          };
        }

        // حالت 2: لاگین استاندارد با شماره و رمز عبور
        try {
          if (!credentials?.PhoneNumber || !credentials?.PassWord) {
            throw new Error(JSON.stringify({ message: "اطلاعات ورود ناقص است" }));
          }

          const loginUrl = process.env.NEXT_PUBLIC_AUTH_API_URL + "/User/Login";
          const response = await fetch(loginUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              PhoneNumber: credentials.PhoneNumber,
              PassWord: credentials.PassWord,
            }),
          });
          const data = await response.json();

          if (response.status === 200 && data.NeedSendCode) {
            throw new Error(
              JSON.stringify({
                needSmsCode: true,
                message: "کد تایید برای شما ارسال شد",
              })
            );
          }

          const UserInfo = data.data;

          if (response.status === 200 && UserInfo) {
            console.log("Authorizing initial login...");
            return {
              id: UserInfo.Id || credentials.PhoneNumber,
              phoneNumber: UserInfo.phoneNumber || credentials.PhoneNumber,
              token: UserInfo.token,
              role: UserInfo.role,
              name: UserInfo.fullName || null,
              email: null,
              refreshToken: UserInfo.refreshToken,
            };
          }

          throw null;
        } catch (error) {
          throw error;
        }
      },
    }),
  ],
});