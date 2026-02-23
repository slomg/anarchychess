import { renderHook } from "@testing-library/react";
import { act } from "react";

import { mockRouter, RouterMock } from "@/lib/testUtils/mocks/mockRouter";
import { checkBotHealth, GameColor, startBotGame } from "@/lib/apiClient";
import { randomizeColor } from "@/lib/utils/chessUtils";
import useBotMatch from "../useBotMatch";
import constants from "@/lib/constants";

vi.mock("@/lib/apiClient/definition");
vi.mock("@/lib/utils/chessUtils");

describe("useBotMatch", () => {
    const fakeGameToken = "testgametoken123";

    let routerMock: RouterMock;
    const checkBotHealthMock = vi.mocked(checkBotHealth);
    const startBotGameMock = vi.mocked(startBotGame);
    const randomizeColorMock = vi.mocked(randomizeColor);

    beforeEach(() => {
        routerMock = mockRouter();

        checkBotHealthMock.mockResolvedValue({
            error: undefined,
            data: true,
            response: new Response(),
        });
        startBotGameMock.mockResolvedValue({
            error: undefined,
            data: fakeGameToken,
            response: new Response(),
        });
        randomizeColorMock.mockReturnValue(GameColor.WHITE);

        vi.clearAllMocks();
    });

    it("should match a game and navigate to it when bot is healthy", async () => {
        const { result } = renderHook(() => useBotMatch());
        let success: boolean | undefined;

        await act(async () => {
            success = await result.current.matchBotGame(GameColor.BLACK);
        });

        expect(success).toBe(true);
        expect(checkBotHealthMock).toHaveBeenCalledOnce();
        expect(startBotGameMock).toHaveBeenCalledExactlyOnceWith({
            query: { myColor: GameColor.BLACK },
        });
        expect(routerMock.push).toHaveBeenCalledExactlyOnceWith(
            `${constants.PATHS.BOT}/${fakeGameToken}`,
        );
    });

    it("should use random color if none provided", async () => {
        const { result } = renderHook(() => useBotMatch());
        randomizeColorMock.mockReturnValueOnce(GameColor.BLACK);

        await act(async () => {
            await result.current.matchBotGame(null);
        });

        expect(startBotGameMock).toHaveBeenCalledExactlyOnceWith({
            query: { myColor: GameColor.BLACK },
        });
    });

    it("should return false and navigate offline if bot is unhealthy", async () => {
        checkBotHealthMock.mockResolvedValueOnce({
            error: undefined,
            data: false,
            response: new Response(),
        });

        const { result } = renderHook(() => useBotMatch());
        let success: boolean | undefined;

        await act(async () => {
            success = await result.current.matchBotGame(GameColor.WHITE);
        });

        expect(success).toBe(false);
        expect(routerMock.push).toHaveBeenCalledExactlyOnceWith(
            constants.PATHS.BOT_OFFLINE,
        );
        expect(startBotGameMock).not.toHaveBeenCalled();
    });

    it("should return false if startBotGame fails", async () => {
        startBotGameMock.mockResolvedValueOnce({
            error: { errors: [] },
            data: undefined,
            response: new Response(),
        });

        const { result } = renderHook(() => useBotMatch());
        let success: boolean | undefined;

        await act(async () => {
            success = await result.current.matchBotGame(GameColor.WHITE);
        });

        expect(success).toBe(false);
        expect(routerMock.push).not.toHaveBeenCalled();
    });
});
