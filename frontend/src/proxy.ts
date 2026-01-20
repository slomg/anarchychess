import { NextRequest, NextResponse } from "next/server";

import constants from "@/lib/constants";

export async function proxy(request: NextRequest) {
    const hasAuthCookie = request.cookies.has(constants.COOKIES.ACCESS_TOKEN);
    const shouldBeAuthed = request.cookies.has(constants.COOKIES.IS_LOGGED_IN);

    if (
        hasAuthCookie &&
        constants.DISALLOW_AUTH_PATHS.has(request.nextUrl.pathname)
    ) {
        const url = request.nextUrl.clone();
        url.pathname = "/";
        const response = NextResponse.redirect(url);
        return response;
    }

    // if the user doesn't have an auth cookie
    // but is expected to be authenticated, we should refresh the token
    if (!hasAuthCookie && shouldBeAuthed) {
        const url = request.nextUrl.clone();
        url.pathname = constants.PATHS.REFRESH;
        const response = NextResponse.rewrite(url);
        return response;
    }

    return NextResponse.next();
}

export const config = {
    matcher: ["/((?!_next|favicon.ico|api|assets|data|logout).*)"],
};
