import { createFakeGuestUser } from "@/lib/testUtils/fakers/userFaker";
import { GuestUser, SessionUser } from "@/lib/apiClient";
import { fetchUserSession } from "../../lib/getLoggedIn";
import SessionProvider from "../../contexts/sessionContext";
import WithSession from "../WithSession";
import SessionBootstrap from "../../components/SessionBootstrap";

vi.mock("@/lib/apiClient/definition");
vi.mock("../../lib/getLoggedIn");
vi.mock("next/navigation");

describe("WithSession", () => {
    let userMock: GuestUser;
    let sessionMock: { user: SessionUser; accessToken: string };

    const fetchUserSessionMock = vi.mocked(fetchUserSession);
    const childrenMock = vi.fn();

    beforeEach(() => {
        userMock = createFakeGuestUser();
        sessionMock = { user: userMock, accessToken: "token-abc" };
    });

    it("should render children inside SessionProvider when a session exists", async () => {
        fetchUserSessionMock.mockResolvedValue(sessionMock);

        const ui = await WithSession({
            children: childrenMock,
        });

        expect(ui.type).toBe(SessionProvider);
        expect(ui.props).toEqual(
            expect.objectContaining({
                user: userMock,
                fetchAttempted: true,
            }),
        );
        expect(childrenMock).toHaveBeenCalledWith(sessionMock);
    });

    it("should render SessionBootstrap when no session exists", async () => {
        fetchUserSessionMock.mockResolvedValue(null);

        const ui = await WithSession({
            children: childrenMock,
        });

        expect(ui.type).toBe(SessionBootstrap);
        expect(childrenMock).not.toHaveBeenCalled();
    });
});
