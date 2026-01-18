import { mockJsCookie } from "@/lib/testUtils/mocks/mockCookies";
import { renderHook } from "@testing-library/react";
import useCookieValue from "../useCookieValue";

vi.mock("js-cookie");

describe("useCookieValue", () => {
    const cookieName = "testCookie";

    it("should initialize with the initial value if cookie is not set", () => {
        const { result } = renderHook(() =>
            useCookieValue<number>(cookieName, 42),
        );

        expect(result.current).toBe(42);
    });

    it("should initialize with the value from cookie if present", () => {
        const cookieValue = 100;
        mockJsCookie({ [cookieName]: JSON.stringify(cookieValue) });

        const { result } = renderHook(() =>
            useCookieValue<number>(cookieName, 42),
        );

        expect(result.current).toBe(100);
    });
});
