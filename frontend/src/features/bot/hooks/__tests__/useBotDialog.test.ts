import { renderHook } from "@testing-library/react";

import useBotDialog, {
    BotDialogOptions,
    ReactionDialog,
} from "../useBotDialog";

import { createFakeDialogContext } from "@/lib/testUtils/fakers/dialogContextFaker";
import { GameColor, GameResult } from "@/lib/apiClient";

describe("useBotDialog", () => {
    function getOptions(
        overrides?: Partial<BotDialogOptions>,
    ): BotDialogOptions {
        return {
            reactionDialog: [],
            generalDialog: [],
            startDialog: [],
            botWinDialog: [],
            botLoseDialog: [],
            ...overrides,
        };
    }

    describe("Reaction Lines", () => {
        it("should return a valid reaction line when condition is met", () => {
            const lines: ReactionDialog[] = [
                { condition: () => true, lines: ["R1"] },
            ];
            const { result } = renderHook(() =>
                useBotDialog(getOptions({ reactionDialog: lines })),
            );
            const line = result.current.getDialogForMove(
                createFakeDialogContext(),
            );
            expect(line).toBe("R1");
        });

        it("should return null if no reaction line condition is met", () => {
            const lines: ReactionDialog[] = [
                { condition: () => false, lines: ["R1"] },
            ];
            const { result } = renderHook(() =>
                useBotDialog(getOptions({ reactionDialog: lines })),
            );
            const line = result.current.getDialogForMove(
                createFakeDialogContext(),
            );
            expect(line).toBeNull();
        });

        it("should not repeat reaction lines until all have been used", () => {
            const lines: ReactionDialog[] = [
                { condition: () => true, lines: ["R1"] },
                { condition: () => true, lines: ["R2"] },
            ];
            const { result } = renderHook(() =>
                useBotDialog(getOptions({ reactionDialog: lines })),
            );
            const first = result.current.getDialogForMove(
                createFakeDialogContext({ plyNumber: 1 }),
            );
            const second = result.current.getDialogForMove(
                createFakeDialogContext({ plyNumber: 2 }),
            );
            const third = result.current.getDialogForMove(
                createFakeDialogContext({ plyNumber: 2 }),
            );
            expect(first).not.toBe(second);
            expect(third).toBeNull();
        });

        it("should pass the correct context to the condition", () => {
            const conditionMock = vi.fn();
            const lines: ReactionDialog[] = [
                { condition: conditionMock, lines: ["R1"] },
            ];
            const { result } = renderHook(() =>
                useBotDialog(getOptions({ reactionDialog: lines })),
            );

            const ctx = createFakeDialogContext();
            result.current.getDialogForMove(ctx);

            expect(conditionMock).toHaveBeenCalledExactlyOnceWith(ctx);
        });

        it("should pick different reaction lines randomly when multiple conditions are met", () => {
            const lines: ReactionDialog[] = [
                { condition: () => true, lines: ["R1"] },
                { condition: () => true, lines: ["R2"] },
            ];

            const { result } = renderHook(() =>
                useBotDialog(getOptions({ reactionDialog: lines })),
            );
            const ctx1 = createFakeDialogContext({ plyNumber: 1 });
            const ctx2 = createFakeDialogContext({ plyNumber: 5 });

            const first = result.current.getDialogForMove(ctx1);
            const second = result.current.getDialogForMove(ctx2);

            expect([first, second].sort()).toEqual(["R1", "R2"]);
        });
    });

    describe("General Lines", () => {
        it("should return null on first call (interval not reached)", () => {
            const lines = ["G1", "G2"];
            const { result } = renderHook(() =>
                useBotDialog(getOptions({ generalDialog: lines })),
            );
            const line = result.current.getDialogForMove(
                createFakeDialogContext(),
            );
            expect(line).toBeNull();
        });

        it("should return a general line when ply interval is reached", () => {
            const lines = ["G1", "G2"];
            const { result } = renderHook(() =>
                useBotDialog(getOptions({ generalDialog: lines })),
            );

            result.current.getDialogForMove(
                createFakeDialogContext({ plyNumber: 1 }),
            );

            const line = result.current.getDialogForMove(
                createFakeDialogContext({ plyNumber: 100 }),
            );
            expect(lines).toContain(line);
        });

        it("should reschedule the next general line after picking a line", () => {
            const lines = ["G1", "G2"];
            const { result } = renderHook(() =>
                useBotDialog(getOptions({ generalDialog: lines })),
            );

            expect(
                result.current.getDialogForMove(
                    createFakeDialogContext({ plyNumber: 1 }),
                ),
            ).toBeNull();

            const firstLine = result.current.getDialogForMove(
                createFakeDialogContext({ plyNumber: 100 }),
            );
            expect(lines).toContain(firstLine);

            const secondLineSamePly = result.current.getDialogForMove(
                createFakeDialogContext({ plyNumber: 103 }),
            );
            expect(secondLineSamePly).toBeNull();

            const nextLine = result.current.getDialogForMove(
                createFakeDialogContext({ plyNumber: 200 }),
            );
            expect(lines).toContain(nextLine);
            expect(nextLine).not.toBe(firstLine);

            const final = result.current.getDialogForMove(
                createFakeDialogContext({ plyNumber: 201 }),
            );
            expect(final).toBeNull();
        });
    });

    describe("getDialogForGameStart", () => {
        it("should return a line from startDialog", () => {
            const startLines = ["start1", "start2", "start3"];
            const { result } = renderHook(() =>
                useBotDialog(getOptions({ startDialog: startLines })),
            );

            const line = result.current.getDialogForGameStart("game 123");
            expect(startLines).toContain(line);
        });

        it("should return the same line for the same game token", () => {
            const startLines = ["Start1", "Start2", "Start3"];
            const { result } = renderHook(() =>
                useBotDialog(getOptions({ startDialog: startLines })),
            );

            const line1 = result.current.getDialogForGameStart("game 123");
            const line2 = result.current.getDialogForGameStart("game 123");
            expect(line1).toBe(line2);
        });
    });

    describe("getDialogForGameEnd", () => {
        it("returns a bot win line when bot wins", () => {
            const botWinLines = ["win1", "win2"];
            const botLoseLines = ["lose1", "lose2"];
            const { result } = renderHook(() =>
                useBotDialog(
                    getOptions({
                        botWinDialog: botWinLines,
                        botLoseDialog: botLoseLines,
                    }),
                ),
            );

            const lineWhiteWin = result.current.getDialogForGameEnd(
                GameResult.WHITE_WIN,
                GameColor.WHITE,
                "game 1",
            );
            const lineBlackWin = result.current.getDialogForGameEnd(
                GameResult.BLACK_WIN,
                GameColor.BLACK,
                "game 2",
            );

            expect(botWinLines).toContain(lineWhiteWin);
            expect(botWinLines).toContain(lineBlackWin);
        });

        it("returns a bot lose line when bot loses", () => {
            const botWinLines = ["win1", "win2"];
            const botLoseLines = ["lose1", "lose2"];
            const { result } = renderHook(() =>
                useBotDialog(
                    getOptions({
                        botWinDialog: botWinLines,
                        botLoseDialog: botLoseLines,
                    }),
                ),
            );

            const lineWhiteLose = result.current.getDialogForGameEnd(
                GameResult.BLACK_WIN,
                GameColor.WHITE,
                "game 1",
            );
            const lineBlackLose = result.current.getDialogForGameEnd(
                GameResult.WHITE_WIN,
                GameColor.BLACK,
                "game 2",
            );

            expect(botLoseLines).toContain(lineWhiteLose);
            expect(botLoseLines).toContain(lineBlackLose);
        });

        it("returns the same line for the same game token and outcome", () => {
            const botWinLines = ["win1", "win2"];
            const botLoseLines = ["lose1", "lose2"];
            const { result } = renderHook(() =>
                useBotDialog(
                    getOptions({
                        botWinDialog: botWinLines,
                        botLoseDialog: botLoseLines,
                    }),
                ),
            );

            const line1 = result.current.getDialogForGameEnd(
                GameResult.WHITE_WIN,
                GameColor.WHITE,
                "game 1",
            );
            const line2 = result.current.getDialogForGameEnd(
                GameResult.WHITE_WIN,
                GameColor.WHITE,
                "game 1",
            );

            expect(line1).toBe(line2);
        });
    });

    it("should prioritize reaction lines over general lines", () => {
        const reactionLines: ReactionDialog[] = [
            { condition: () => true, lines: ["R1"] },
        ];
        const generalLines = ["G1"];

        const { result } = renderHook(() =>
            useBotDialog(
                getOptions({
                    reactionDialog: reactionLines,
                    generalDialog: generalLines,
                }),
            ),
        );
        const line = result.current.getDialogForMove(
            createFakeDialogContext({ plyNumber: 1 }),
        );
        expect(line).toBe("R1");
    });

    it("should fallback to general lines if no reaction lines trigger", () => {
        const reactionLines: ReactionDialog[] = [
            { condition: () => false, lines: ["R1"] },
        ];
        const generalLines = ["G1"];

        const { result } = renderHook(() =>
            useBotDialog(
                getOptions({
                    reactionDialog: reactionLines,
                    generalDialog: generalLines,
                }),
            ),
        );

        result.current.getDialogForMove(
            createFakeDialogContext({ plyNumber: 1 }),
        );
        const line = result.current.getDialogForMove(
            createFakeDialogContext({ plyNumber: 100 }),
        );
        expect(line).toBe("G1");
    });

    it("should return null if the last dialog was within a ply even if other lines are available", () => {
        const reactionLines: ReactionDialog[] = [
            { condition: () => true, lines: ["R1"] },
            { condition: () => true, lines: ["R2"] },
        ];

        const { result } = renderHook(() =>
            useBotDialog(
                getOptions({
                    reactionDialog: reactionLines,
                }),
            ),
        );

        const first = result.current.getDialogForMove(
            createFakeDialogContext({ plyNumber: 1 }),
        );
        expect(first).not.toBeNull();

        const second = result.current.getDialogForMove(
            createFakeDialogContext({ plyNumber: 2 }),
        );
        expect(second).toBeNull();

        const third = result.current.getDialogForMove(
            createFakeDialogContext({ plyNumber: 3 }),
        );
        expect(third).not.toBeNull();
    });
});
