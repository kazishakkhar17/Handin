import { NextRequest, NextResponse } from "next/server";

const ROLE_HOME: Record<string, string> = {
  Admin: "/admin",
  Teacher: "/teacher",
  Student: "/student",
};

// Client-side auth context is the source of truth for what data loads (and the API
// re-checks every request via the JWT), but this middleware stops obviously-wrong
// navigations early — e.g. a Student typing /admin into the address bar — for a
// smoother experience.
export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const role = request.cookies.get("role")?.value;

  const isProtectedPath =
    pathname.startsWith("/admin") ||
    pathname.startsWith("/teacher") ||
    pathname.startsWith("/student");

  if (isProtectedPath && !role) {
    return NextResponse.redirect(new URL("/login", request.url));
  }

  if (role && isProtectedPath) {
    const homePrefix = ROLE_HOME[role];
    if (homePrefix && !pathname.startsWith(homePrefix)) {
      return NextResponse.redirect(new URL(homePrefix, request.url));
    }
  }

  if (role && pathname === "/login") {
    return NextResponse.redirect(new URL(ROLE_HOME[role] ?? "/login", request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/admin/:path*", "/teacher/:path*", "/student/:path*", "/login"],
};
