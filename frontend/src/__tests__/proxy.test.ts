import { RequestCookies } from "next/dist/compiled/@edge-runtime/cookies";
import { NextURL } from "next/dist/server/web/next-url";
import { NextRequest, NextResponse } from "next/server";

import constants from "@/lib/constants";
import { proxy } from "../proxy";

vi.mock("next/server", () => ({
    NextRequest: vi.fn(),
    NextResponse: {
        next: vi.fn(() => ({ type: "next" })),
        rewrite: vi.fn((url) => ({ type: "rewrite", url })),
        redirect: vi.fn((url) => ({ type: "redirect", url })),
    },
}));

describe("proxy", () => {
    function createRequest({
        setCookies,
        pathname,
    }: {
        setCookies: Record<string, string>;
        pathname?: string;
    }): NextRequest {
        pathname ??= "/some-path";
        const url = new URL(`http://localhost:3000${pathname}`);
        const nextUrl = new NextURL(url);
        const headers = new Headers();
        const cookies = new RequestCookies(headers);
        for (const [name, value] of Object.entries(setCookies)) {
            cookies.set(name, value);
        }

        const request = {
            nextUrl,
            cookies,
            headers,
        } as unknown as NextRequest;
        return request;
    }

    it.each([
        {
            [constants.COOKIES.ACCESS_TOKEN]: "access",
            [constants.COOKIES.IS_LOGGED_IN]: "true",
        },
        {
            [constants.COOKIES.ACCESS_TOKEN]: "access",
        },
    ])(
        "should not attempt to refresh when not needed",
        async (setCookies: Record<string, string>) => {
            const request = createRequest({ setCookies });

            const response = await proxy(request);

            expect(NextResponse.next).toHaveBeenCalled();
            expect(response).toEqual({ type: "next" });
        },
    );

    it("should rewrite to refresh path if needed", async () => {
        const pathname = "/some-path";
        const request = createRequest({
            setCookies: {
                [constants.COOKIES.IS_LOGGED_IN]: "true",
            },
            pathname,
        });

        const response = await proxy(request);

        expect(NextResponse.rewrite).toHaveBeenCalled();
        expect(response.type).toBe("rewrite");

        const url = new URL(response.url);
        expect(url.pathname).toBe(constants.PATHS.REFRESH);
    });

    it("should redirect to '/' if access token exists and path is disallowed", async () => {
        const request = createRequest({
            setCookies: {
                [constants.COOKIES.IS_LOGGED_IN]: "true",
            },
            pathname: "/signin",
        });

        const response = await proxy(request);

        expect(NextResponse.redirect).toHaveBeenCalled();
        expect(response.type).toBe("redirect");

        const url = new URL(response.url);
        expect(url.pathname).toBe("/");
    });
});
