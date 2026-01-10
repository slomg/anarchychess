import { render } from "@testing-library/react";

import { mockRouter, RouterMock } from "@/lib/testUtils/mocks/mockRouter";
import { mockJsCookie } from "@/lib/testUtils/mocks/mockCookies";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";
import SessionBootstrap from "../SessionBootstrap";
import { createGuestUser } from "@/lib/apiClient";
import AuthRefresh from "../AuthRefresh";
import constants from "@/lib/constants";

vi.mock("@/lib/apiClient/definition");
vi.mock("../AuthRefresh");
vi.mock("js-cookie");

describe("SessionBootstrap", () => {
    const createGuestUserMock = vi.mocked(createGuestUser);
    const AuthRefreshMock = vi.mocked(AuthRefresh);
    let routerMock: RouterMock;

    beforeEach(() => {
        routerMock = mockRouter();
        createGuestUserMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
    });

    it("should create guest and refresh", async () => {
        render(<SessionBootstrap />);
        await flushMicrotasks();

        expect(createGuestUserMock).toHaveBeenCalledOnce();
        expect(routerMock.refresh).toHaveBeenCalledOnce();
        expect(routerMock.replace).not.toHaveBeenCalled();
    });

    it("should redirect to signin if guest creation fails", async () => {
        createGuestUserMock.mockResolvedValue({
            error: "test error",
            data: undefined,
            response: new Response(),
        });

        render(<SessionBootstrap />);
        await flushMicrotasks();

        expect(createGuestUserMock).toHaveBeenCalledOnce();
        expect(routerMock.replace).toHaveBeenCalledExactlyOnceWith(
            constants.PATHS.SIGNIN,
        );
        expect(routerMock.refresh).not.toHaveBeenCalled();
    });

    it("should not create a guest and refresh if we should be logged in", async () => {
        mockJsCookie({ [constants.COOKIES.IS_LOGGED_IN]: "true" });

        render(<SessionBootstrap />);
        await flushMicrotasks();

        expect(AuthRefreshMock).toHaveBeenCalled();
        expect(routerMock.replace).not.toHaveBeenCalled();
        expect(routerMock.refresh).not.toHaveBeenCalled();
        expect(createGuestUserMock).not.toHaveBeenCalled();
    });
});
