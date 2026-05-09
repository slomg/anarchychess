import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import traitorRook from "@public/assets/pieces-svg/traitorrook.svg";
import whiteKing from "@public/assets/pieces-svg/king-white.svg";
import blackKing from "@public/assets/pieces-svg/king-black.svg";
import { BotType, GameColor } from "@/lib/apiClient";
import useBotMatch from "../../hooks/useBotMatch";
import BotPlayOptions from "../BotPlayOptions";

vi.mock("../../hooks/useBotMatch");
vi.mock("next/image");

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

    it.each([
        [BotType.ANARCHY_BOT, "Anarchy Bot"],
        [BotType.LOBOTOMIZED_ANARCHY_BOT, "Lobotomized Anarchy Bot"],
    ])(
        "should start the game with the correct bot type",
        async (botType, botName) => {
            const user = userEvent.setup();
            render(<BotPlayOptions />);

            await user.click(screen.getByText(botName));

            await user.click(screen.getByTestId("botPlayOptionsStartButton"));

            expect(matchBotGameMock).toHaveBeenCalledExactlyOnceWith(
                expect.toBeOneOf([null, GameColor.WHITE, GameColor.BLACK]),
                botType,
            );
        },
    );

    it.each([GameColor.WHITE, GameColor.BLACK, null])(
        "should start the game with the correct color",
        async (color) => {
            const user = userEvent.setup();
            render(<BotPlayOptions />);

            const selector = screen.getByTestId("botPlayOptionsColorSelector");
            await user.click(within(selector).getByTestId("selector-" + color));

            await user.click(screen.getByTestId("botPlayOptionsStartButton"));

            expect(matchBotGameMock).toHaveBeenCalledExactlyOnceWith(
                color,
                expect.anything(),
            );
        },
    );

    it.each([
        [GameColor.WHITE, whiteKing, "play as white"],
        [GameColor.BLACK, blackKing, "play as black"],
        [null, traitorRook, "random color"],
    ])(
        "should render the correct image for color options",
        (color, image, alt) => {
            render(<BotPlayOptions />);

            const selector = screen.getByTestId("botPlayOptionsColorSelector");
            const selectorImage = within(
                within(selector).getByTestId("selector-" + color),
            ).getByAltText(alt);

            expect(selectorImage).toBeInTheDocument();
            expect(selectorImage.getAttribute("src")).toEqual(image);
        },
    );
});
