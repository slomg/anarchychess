import { render } from "@testing-library/react";

import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";
import LogoutHandler from "../LogoutHandler";
import { logout } from "@/lib/apiClient";
import constants from "@/lib/constants";

vi.mock("@/lib/apiClient/definition");

describe("LogoutHandler", () => {
    const logoutMock = vi.mocked(logout);

    it("should call logout when mounted", async () => {
        render(<LogoutHandler />);
        await flushMicrotasks();

        expect(logoutMock).toHaveBeenCalledOnce();
    });

    it("should navigate to the signin page after logout", async () => {
        const routerMock = mockRouter();

        render(<LogoutHandler />);
        await flushMicrotasks();

        expect(routerMock.replace).toHaveBeenCalledWith(constants.PATHS.SIGNIN);
    });
});
