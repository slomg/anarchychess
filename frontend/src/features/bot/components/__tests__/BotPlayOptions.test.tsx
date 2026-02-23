import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import useBotMatch from "../../hooks/useBotMatch";
import BotPlayOptions from "../BotPlayOptions";
import { GameColor } from "@/lib/apiClient";

vi.mock("../../hooks/useBotMatch");

describe("BotPlayOptions", () => {
    const useBotMatchMock = vi.mocked(useBotMatch);
    const matchBotGameMock = vi.fn();

    beforeEach(() => {
        useBotMatchMock.mockReturnValue({
            matchBotGame: matchBotGameMock,
            isMatching: false,
        });
    });

    it("should start a bot game and navigate to it", async () => {
        const user = userEvent.setup();

        render(<BotPlayOptions />);

        await user.click(screen.getByTestId("botPlayOptionsStartButton"));

        expect(matchBotGameMock).toHaveBeenCalledOnce();
    });

    it("should show an error message if starting a game fails", async () => {
        matchBotGameMock.mockResolvedValue(false);

        const user = userEvent.setup();
        render(<BotPlayOptions />);

        await user.click(screen.getByTestId("botPlayOptionsStartButton"));

        expect(screen.getByTestId("botPlayOptionsError")).toHaveTextContent(
            "Failed to start game. Please try again.",
        );
    });

    it.each([GameColor.WHITE, GameColor.BLACK, null])(
        "should allow selecting color and pass it to startBotGame",
        async (color) => {
            const user = userEvent.setup();
            render(<BotPlayOptions />);

            const selector = screen.getByTestId("botPlayOptionsColorSelector");
            await user.click(within(selector).getByTestId("selector-" + color));

            await user.click(screen.getByTestId("botPlayOptionsStartButton"));

            expect(matchBotGameMock).toHaveBeenCalledExactlyOnceWith(color);
        },
    );
});
