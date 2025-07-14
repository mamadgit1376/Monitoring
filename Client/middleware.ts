import {auth} from "@/auth"
import NextAuth, { NextAuthRequest } from "next-auth"
 
// Use only one of the two middleware options below
// 1. Use middleware directly
// export const { auth: middleware } = NextAuth(authConfig)
 
// 2. Wrapped middleware option

export default auth((req: NextAuthRequest, ) => {
  // Your custom middleware logic goes here
  
  
    

    const isLoggedIn = !!req.auth
  const { pathname } = req.nextUrl

    const isNotAuthRoute = pathname =="/" ? true: false;


  // اگر لاگین نیست و در مسیرهای غیر از auth هست
  if (isLoggedIn ==false && !isNotAuthRoute) {
    const signUpUrl = new URL("/", req.url)
    return Response.redirect(signUpUrl)
  }

  // اجازه ادامه درخواست
  return undefined
})
export const config = {
    matcher: [
      /*
       * Match all request paths except for the ones starting with:
       * - api (API routes)
       * - _next/static (static files)
       * - _next/image (image optimization files)
       * - favicon.ico, sitemap.xml, robots.txt (metadata files)
       */
      '/((?!api|_next/static|_next/image|favicon.ico|sitemap.xml|robots.txt).*)',
    ],
  }