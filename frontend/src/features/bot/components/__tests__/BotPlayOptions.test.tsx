import { GameColor, startBotGame } from "@/lib/apiClient";
import { mockRouter, RouterMock } from "@/lib/testUtils/mocks/mockRouter";
import { render, screen, within } from "@testing-library/react";
import BotPlayOptions from "../BotPlayOptions";
import userEvent from "@testing-library/user-event";
import constants from "@/lib/constants";
import { randomizeColor } from "@/lib/utils/chessUtils";

vi.mock("@/lib/apiClient/definition");
vi.mock("@/lib/utils/chessUtils");

describe("BotPlayOptions", () => {
    let routerMock: RouterMock;
    const mockStartBotGame = vi.mocked(startBotGame);
    const fakeGameToken = "fakeGameToken";
    const randomizeColorMock = vi.mocked(randomizeColor);

    beforeEach(() => {
        routerMock = mockRouter();
        randomizeColorMock.mockReturnValue(GameColor.WHITE);
        mockStartBotGame.mockResolvedValue({
            data: fakeGameToken,
            error: undefined,
            response: new Response(),
        });
    });

    it("should start a bot game and navigate to it", async () => {
        const user = userEvent.setup();

        render(<BotPlayOptions />);

        await user.click(screen.getByTestId("botPlayOptionsStartButton"));

        expect(mockStartBotGame).toHaveBeenCalledOnce();
        expect(routerMock.push).toHaveBeenCalledExactlyOnceWith(
            `${constants.PATHS.BOT}/${fakeGameToken}`,
        );
    });

    it("should show an error message if starting a game fails", async () => {
        const user = userEvent.setup();
        mockStartBotGame.mockResolvedValue({
            data: undefined,
            error: { errors: [] },
            response: new Response(),
        });

        render(<BotPlayOptions />);

        await user.click(screen.getByTestId("botPlayOptionsStartButton"));

        expect(screen.getByTestId("botPlayOptionsError")).toHaveTextContent(
            "Failed to start game. Please try again.",
        );
    });

    it.each([GameColor.WHITE, GameColor.BLACK])(
        "should allow selecting color and pass it to startBotGame",
        async (color) => {
            const user = userEvent.setup();
            render(<BotPlayOptions />);

            const selector = screen.getByTestId("botPlayOptionsColorSelector");
            await user.click(within(selector).getByTestId("selector-" + color));

            await user.click(screen.getByTestId("botPlayOptionsStartButton"));

            expect(mockStartBotGame).toHaveBeenCalledExactlyOnceWith({
                query: { myColor: color },
            });
        },
    );

    it("should select a random color if random is selected", async () => {
        const user = userEvent.setup();
        render(<BotPlayOptions />);

        randomizeColorMock.mockReturnValueOnce(GameColor.WHITE);

        const selector = screen.getByTestId("botPlayOptionsColorSelector");
        await user.click(within(selector).getByTestId("selector-null"));

        await user.click(screen.getByTestId("botPlayOptionsStartButton"));

        expect(mockStartBotGame).toHaveBeenCalledExactlyOnceWith({
            query: { myColor: GameColor.WHITE },
        });

        randomizeColorMock.mockReturnValueOnce(GameColor.BLACK);
        await user.click(screen.getByTestId("botPlayOptionsStartButton"));
        expect(mockStartBotGame).toHaveBeenCalledWith({
            query: { myColor: GameColor.BLACK },
        });
    });
});
