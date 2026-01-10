import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import { fetchAuthedUserSession } from "../../lib/getLoggedIn";
import SessionProvider from "../../contexts/sessionContext";
import AuthRefresh from "../../components/AuthRefresh";
import WithAuthedUser from "../WithAuthedUser";
import { PrivateUser } from "@/lib/apiClient";

vi.mock("@/lib/apiClient/definition");
vi.mock("../../lib/getLoggedIn");
vi.mock("next/navigation");

describe("WithAuthedUser", () => {
    let userMock: PrivateUser;
    let sessionMock: { user: PrivateUser; accessToken: string };

    const fetchAuthedUserSessionMock = vi.mocked(fetchAuthedUserSession);
    const childrenMock = vi.fn();

    beforeEach(() => {
        userMock = createFakePrivateUser();
        sessionMock = { user: userMock, accessToken: "test access token" };
    });

    it("should render children with session when user is authenticated", async () => {
        fetchAuthedUserSessionMock.mockResolvedValue(sessionMock);

        const ui = await WithAuthedUser({
            children: childrenMock,
        });
        expect(ui.type).toBe(SessionProvider);
        expect(ui.props).toEqual(
            expect.objectContaining({ fetchAttempted: true, user: userMock }),
        );
        expect(childrenMock).toHaveBeenCalledWith(sessionMock);
    });

    it("should render AuthRefresh when no session", async () => {
        fetchAuthedUserSessionMock.mockResolvedValue(null);

        const ui = await WithAuthedUser({
            children: childrenMock,
        });

        expect(ui.type).toBe(AuthRefresh);
        expect(childrenMock).not.toHaveBeenCalled();
    });
});
