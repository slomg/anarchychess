import { render, screen } from "@testing-library/react";

import { createFakeGameSummary } from "@/lib/testUtils/fakers/gameSummaryFaker";
import { GameResult, GameSummary, PublicUser } from "@/lib/apiClient";
import { createFakeUser } from "@/lib/testUtils/fakers/userFaker";
import constants from "@/lib/constants";
import GameRow from "../GameRow";

vi.mock("@/features/lobby/components/TimeControlIconFromSeconds", () => ({
    default: ({ baseSeconds }: { baseSeconds: number }) => (
        <div data-testid="timeControlFromSecondsIcon">{baseSeconds}</div>
    ),
}));

describe("GameRow", () => {
    let gameSummaryMock: GameSummary;
    let userMock: PublicUser;

    beforeEach(() => {
        gameSummaryMock = createFakeGameSummary();
        userMock = createFakeUser();
    });

    it("should display the correct usernames", () => {
        render(
            <table>
                <tbody>
                    <GameRow
                        game={gameSummaryMock}
                        profileViewpoint={userMock}
                        index={0}
                    />
                </tbody>
            </table>,
        );

        expect(screen.getByTestId("gameRowWhiteUsername").textContent).toBe(
            gameSummaryMock.whitePlayer?.userName,
        );
        expect(screen.getByTestId("gameRowBlackUsername").textContent).toBe(
            gameSummaryMock.blackPlayer?.userName,
        );
    });

    it.each([
        [GameResult.WHITE_WIN, "1", "0"],
        [GameResult.BLACK_WIN, "0", "1"],
        [GameResult.DRAW, "½", "½"],
    ])(
        "should correctly calculate the score of each player",
        (result, whiteScore, blackScore) => {
            gameSummaryMock.result = result;

            render(
                <table>
                    <tbody>
                        <GameRow
                            game={gameSummaryMock}
                            profileViewpoint={userMock}
                            index={0}
                        />
                    </tbody>
                </table>,
            );

            expect(screen.getByTestId("gameRowScoreWhite").textContent).toBe(
                whiteScore,
            );
            expect(screen.getByTestId("gameRowScoreBlack").textContent).toBe(
                blackScore,
            );
        },
    );

    it("should display the correct game link", () => {
        render(
            <table>
                <tbody>
                    <GameRow
                        game={gameSummaryMock}
                        profileViewpoint={userMock}
                        index={0}
                    />
                </tbody>
            </table>,
        );
        screen
            .getAllByTestId("gameRowLink")
            .forEach((gameLink) =>
                expect(gameLink.getAttribute("href")).toBe(
                    `${constants.PATHS.GAME}/${gameSummaryMock.gameToken}`,
                ),
            );
    });

    it("should display the correct date", () => {
        render(
            <table>
                <tbody>
                    <GameRow
                        game={gameSummaryMock}
                        profileViewpoint={userMock}
                        index={0}
                    />
                </tbody>
            </table>,
        );

        const formattedDate = new Date(
            gameSummaryMock.createdAt,
        ).toLocaleDateString("en-us", {
            month: "short",
            day: "numeric",
            year: "numeric",
        });
        expect(screen.getByTestId("gameRowDate").textContent).toBe(
            formattedDate,
        );
    });

    it("should display the correct base seconds in the icon", () => {
        render(
            <table>
                <tbody>
                    <GameRow
                        game={gameSummaryMock}
                        profileViewpoint={userMock}
                        index={0}
                    />
                </tbody>
            </table>,
        );

        expect(
            screen.getByTestId("timeControlFromSecondsIcon"),
        ).toHaveTextContent(gameSummaryMock.baseSeconds.toString());
    });

    it("should display the correct time control string", () => {
        render(
            <table>
                <tbody>
                    <GameRow
                        game={gameSummaryMock}
                        profileViewpoint={userMock}
                        index={0}
                    />
                </tbody>
            </table>,
        );

        expect(screen.getByTestId("gameRowTimeControlText")).toHaveTextContent(
            `${gameSummaryMock.baseSeconds / 60}+${gameSummaryMock.incrementSeconds}`,
        );
    });

    it("should display the correct minute when the time control is < 1 minute", () => {
        gameSummaryMock.baseSeconds = 30;

        render(
            <table>
                <tbody>
                    <GameRow
                        game={gameSummaryMock}
                        profileViewpoint={userMock}
                        index={0}
                    />
                </tbody>
            </table>,
        );

        expect(screen.getByTestId("gameRowTimeControlText")).toHaveTextContent(
            `.5+${gameSummaryMock.incrementSeconds}`,
        );
    });

    it("should display bot icon for bot games", () => {
        gameSummaryMock.isBotGame = true;

        render(
            <table>
                <tbody>
                    <GameRow
                        game={gameSummaryMock}
                        profileViewpoint={userMock}
                        index={0}
                    />
                </tbody>
            </table>,
        );

        expect(screen.getByTestId("gameRowBotIcon")).toBeInTheDocument();
        expect(
            screen.queryByTestId("gameRowTimeControlText"),
        ).not.toBeInTheDocument();
    });

    it("should display the correct game link for bot games", () => {
        gameSummaryMock.isBotGame = true;

        render(
            <table>
                <tbody>
                    <GameRow
                        game={gameSummaryMock}
                        profileViewpoint={userMock}
                        index={0}
                    />
                </tbody>
            </table>,
        );

        screen
            .getAllByTestId("gameRowLink")
            .forEach((gameLink) =>
                expect(gameLink.getAttribute("href")).toBe(
                    `${constants.PATHS.BOT}/${gameSummaryMock.gameToken}`,
                ),
            );
    });
});
