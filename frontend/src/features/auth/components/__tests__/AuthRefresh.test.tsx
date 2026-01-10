import { render } from "@testing-library/react";

import { mockRouter, RouterMock } from "@/lib/testUtils/mocks/mockRouter";
import { refresh } from "@/lib/apiClient";
import AuthRefresh from "../AuthRefresh";
import constants from "@/lib/constants";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";

vi.mock("@/lib/apiClient/definition");

describe("AuthRefresh", () => {
    const refreshMock = vi.mocked(refresh);
    let routerMock: RouterMock;

    beforeEach(() => {
        routerMock = mockRouter();
        refreshMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
    });

    it("should refresh the page after refresh", async () => {
        render(<AuthRefresh />);
        await flushMicrotasks();

        expect(refreshMock).toHaveBeenCalledOnce();
        expect(routerMock.refresh).toHaveBeenCalledOnce();
        expect(routerMock.replace).not.toHaveBeenCalled();
    });

    it("should logout if refresh fails", async () => {
        refreshMock.mockResolvedValue({
            error: { errors: [] },
            data: undefined,
            response: new Response(),
        });

        render(<AuthRefresh />);
        await flushMicrotasks();

        expect(refreshMock).toHaveBeenCalledOnce();
        expect(routerMock.replace).toHaveBeenCalledExactlyOnceWith(
            constants.PATHS.LOGOUT,
        );
        expect(routerMock.refresh).not.toHaveBeenCalled();
    });
});
