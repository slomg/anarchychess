import { renderHook } from "@testing-library/react";

import { createFakeVoiceLineContext } from "@/lib/testUtils/fakers/voiceLineContextFaker";
import useBotVoiceLines, {
    BotVoiceLineOptions,
    LoreVoiceLine,
    ReactionVoiceLine,
} from "../useBotVoiceLines";
import { GameColor, GameResult } from "@/lib/apiClient";

describe("useBotVoiceLines", () => {
    function getOptions(
        overrides?: Partial<BotVoiceLineOptions>,
    ): BotVoiceLineOptions {
        return {
            reactionVoiceLines: [],
            loreVoiceLines: [],
            generalVoiceLines: [],
            startVoiceLines: [],
            botWinVoiceLines: [],
            botLoseVoiceLines: [],
            ...overrides,
        };
    }

    describe("Reaction Lines", () => {
        it("should return a valid reaction line when condition is met", () => {
            const lines: ReactionVoiceLine[] = [
                { condition: () => true, lines: ["R1"] },
            ];
            const { result } = renderHook(() =>
                useBotVoiceLines(getOptions({ reactionVoiceLines: lines })),
            );
            const line = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext(),
            );
            expect(line).toBe("R1");
        });

        it("should return null if no reaction line condition is met", () => {
            const lines: ReactionVoiceLine[] = [
                { condition: () => false, lines: ["R1"] },
            ];
            const { result } = renderHook(() =>
                useBotVoiceLines(getOptions({ reactionVoiceLines: lines })),
            );
            const line = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext(),
            );
            expect(line).toBeNull();
        });

        it("should not repeat reaction lines until all have been used", () => {
            const lines: ReactionVoiceLine[] = [
                { condition: () => true, lines: ["R1"] },
                { condition: () => true, lines: ["R2"] },
            ];
            const { result } = renderHook(() =>
                useBotVoiceLines(getOptions({ reactionVoiceLines: lines })),
            );
            const first = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext({ plyNumber: 1 }),
            );
            const second = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext({ plyNumber: 2 }),
            );
            const third = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext({ plyNumber: 2 }),
            );
            expect(first).not.toBe(second);
            expect(third).toBeNull();
        });

        it("should pass the correct context to the condition", () => {
            const conditionMock = vi.fn();
            const lines: ReactionVoiceLine[] = [
                { condition: conditionMock, lines: ["R1"] },
            ];
            const { result } = renderHook(() =>
                useBotVoiceLines(getOptions({ reactionVoiceLines: lines })),
            );

            const ctx = createFakeVoiceLineContext();
            result.current.getVoiceLineForMove(ctx);

            expect(conditionMock).toHaveBeenCalledExactlyOnceWith(ctx);
        });

        it("should pick different reaction lines randomly when multiple conditions are met", () => {
            const lines: ReactionVoiceLine[] = [
                { condition: () => true, lines: ["R1"] },
                { condition: () => true, lines: ["R2"] },
            ];

            const { result } = renderHook(() =>
                useBotVoiceLines(getOptions({ reactionVoiceLines: lines })),
            );
            const ctx1 = createFakeVoiceLineContext({ plyNumber: 1 });
            const ctx2 = createFakeVoiceLineContext({ plyNumber: 5 });

            const first = result.current.getVoiceLineForMove(ctx1);
            const second = result.current.getVoiceLineForMove(ctx2);

            expect([first, second].sort()).toEqual(["R1", "R2"]);
        });
    });

    describe("General Lines", () => {
        it("should return null on first call (interval not reached)", () => {
            const lines = ["G1", "G2"];
            const { result } = renderHook(() =>
                useBotVoiceLines(getOptions({ generalVoiceLines: lines })),
            );
            const line = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext(),
            );
            expect(line).toBeNull();
        });

        it("should return a general line when ply interval is reached", () => {
            const lines = ["G1", "G2"];
            const { result } = renderHook(() =>
                useBotVoiceLines(getOptions({ generalVoiceLines: lines })),
            );

            result.current.getVoiceLineForMove(
                createFakeVoiceLineContext({ playerType: 1 }),
            );

            const line = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext({ plyNumber: 100 }),
            );
            expect(lines).toContain(line);
        });

        it("should reschedule the next general line after picking a line", () => {
            const lines = ["G1", "G2"];
            const { result } = renderHook(() =>
                useBotVoiceLines(getOptions({ generalVoiceLines: lines })),
            );

            expect(
                result.current.getVoiceLineForMove(
                    createFakeVoiceLineContext({ plyNumber: 1 }),
                ),
            ).toBeNull();

            const firstLine = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext({ plyNumber: 100 }),
            );
            expect(lines).toContain(firstLine);

            const secondLineSamePly = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext({ plyNumber: 103 }),
            );
            expect(secondLineSamePly).toBeNull();

            const nextLine = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext({ plyNumber: 200 }),
            );
            expect(lines).toContain(nextLine);
            expect(nextLine).not.toBe(firstLine);

            const final = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext({ plyNumber: 201 }),
            );
            expect(final).toBeNull();
        });
    });

    describe("Lore Lines", () => {
        it("should return a lore line if ply number matches", () => {
            const lines: LoreVoiceLine[] = [{ onPly: 3, lines: ["L1"] }];
            const { result } = renderHook(() =>
                useBotVoiceLines(getOptions({ loreVoiceLines: lines })),
            );
            const line = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext({ plyNumber: 3 }),
            );
            expect(line).toBe("L1");
        });

        it("should not return a lore line before its ply", () => {
            const lines: LoreVoiceLine[] = [{ onPly: 5, lines: ["L1"] }];
            const { result } = renderHook(() =>
                useBotVoiceLines(getOptions({ loreVoiceLines: lines })),
            );
            const line = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext({ plyNumber: 3 }),
            );
            expect(line).toBeNull();
        });

        it("should not repeat lore lines for previous plies", () => {
            const lines: LoreVoiceLine[] = [{ onPly: 3, lines: ["L1", "L2"] }];
            const { result } = renderHook(() =>
                useBotVoiceLines(getOptions({ loreVoiceLines: lines })),
            );

            const first = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext({ plyNumber: 3 }),
            );
            const second = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext({ plyNumber: 3 }),
            );

            expect(["L1", "L2"]).toContain(first);
            expect(second).toBeNull();
        });

        it("should pick only one lore line per ply even if multiple are available", () => {
            const lines: LoreVoiceLine[] = [
                { onPly: 3, lines: ["L1", "L2", "L3"] },
            ];

            const { result } = renderHook(() =>
                useBotVoiceLines(getOptions({ loreVoiceLines: lines })),
            );

            const firstLine = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext({ plyNumber: 3 }),
            );
            expect(["L1", "L2", "L3"]).toContain(firstLine);

            const secondLine = result.current.getVoiceLineForMove(
                createFakeVoiceLineContext({ plyNumber: 3 }),
            );
            expect(secondLine).toBeNull();
        });
    });

    describe("getVoiceLineForGameStart", () => {
        it("should return a line from startVoiceLines", () => {
            const startLines = ["start1", "start2", "start3"];
            const { result } = renderHook(() =>
                useBotVoiceLines(getOptions({ startVoiceLines: startLines })),
            );

            const line = result.current.getVoiceLineForGameStart("game 123");
            expect(startLines).toContain(line);
        });

        it("should return the same line for the same game token", () => {
            const startLines = ["Start1", "Start2", "Start3"];
            const { result } = renderHook(() =>
                useBotVoiceLines(getOptions({ startVoiceLines: startLines })),
            );

            const line1 = result.current.getVoiceLineForGameStart("game 123");
            const line2 = result.current.getVoiceLineForGameStart("game 123");
            expect(line1).toBe(line2);
        });
    });

    describe("getVoiceLineForGameEnd", () => {
        it("returns a bot win line when bot wins", () => {
            const botWinLines = ["win1", "win2"];
            const botLoseLines = ["lose1", "lose2"];
            const { result } = renderHook(() =>
                useBotVoiceLines(
                    getOptions({
                        botWinVoiceLines: botWinLines,
                        botLoseVoiceLines: botLoseLines,
                    }),
                ),
            );

            const lineWhiteWin = result.current.getVoiceLineForGameEnd(
                GameResult.WHITE_WIN,
                GameColor.WHITE,
                "game 1",
            );
            const lineBlackWin = result.current.getVoiceLineForGameEnd(
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
                useBotVoiceLines(
                    getOptions({
                        botWinVoiceLines: botWinLines,
                        botLoseVoiceLines: botLoseLines,
                    }),
                ),
            );

            const lineWhiteLose = result.current.getVoiceLineForGameEnd(
                GameResult.BLACK_WIN,
                GameColor.WHITE,
                "game 1",
            );
            const lineBlackLose = result.current.getVoiceLineForGameEnd(
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
                useBotVoiceLines(
                    getOptions({
                        botWinVoiceLines: botWinLines,
                        botLoseVoiceLines: botLoseLines,
                    }),
                ),
            );

            const line1 = result.current.getVoiceLineForGameEnd(
                GameResult.WHITE_WIN,
                GameColor.WHITE,
                "game 1",
            );
            const line2 = result.current.getVoiceLineForGameEnd(
                GameResult.WHITE_WIN,
                GameColor.WHITE,
                "game 1",
            );

            expect(line1).toBe(line2);
        });
    });

    it("should prioritize reaction lines over general and lore lines", () => {
        const reactionLines: ReactionVoiceLine[] = [
            { condition: () => true, lines: ["R1"] },
        ];
        const loreLines: LoreVoiceLine[] = [{ onPly: 1, lines: ["L1"] }];
        const generalLines = ["G1"];

        const { result } = renderHook(() =>
            useBotVoiceLines(
                getOptions({
                    reactionVoiceLines: reactionLines,
                    loreVoiceLines: loreLines,
                    generalVoiceLines: generalLines,
                }),
            ),
        );
        const line = result.current.getVoiceLineForMove(
            createFakeVoiceLineContext({ plyNumber: 1 }),
        );
        expect(line).toBe("R1");
    });

    it("should fallback to general lines if no reaction lines trigger", () => {
        const reactionLines: ReactionVoiceLine[] = [
            { condition: () => false, lines: ["R1"] },
        ];
        const generalLines = ["G1"];

        const { result } = renderHook(() =>
            useBotVoiceLines(
                getOptions({
                    reactionVoiceLines: reactionLines,
                    generalVoiceLines: generalLines,
                }),
            ),
        );

        result.current.getVoiceLineForMove(
            createFakeVoiceLineContext({ plyNumber: 1 }),
        );
        const line = result.current.getVoiceLineForMove(
            createFakeVoiceLineContext({ plyNumber: 100 }),
        );
        expect(line).toBe("G1");
    });

    it("should fallback to lore lines if no reaction or general lines trigger", () => {
        const reactionLines: ReactionVoiceLine[] = [
            { condition: () => false, lines: ["R1"] },
        ];
        const loreLines: LoreVoiceLine[] = [{ onPly: 1, lines: ["L1"] }];
        const { result } = renderHook(() =>
            useBotVoiceLines(
                getOptions({
                    reactionVoiceLines: reactionLines,
                    loreVoiceLines: loreLines,
                }),
            ),
        );

        const line = result.current.getVoiceLineForMove(
            createFakeVoiceLineContext({ plyNumber: 1 }),
        );
        expect(line).toBe("L1");
    });

    it("should return null if the last voice line was within a ply even if other lines are available", () => {
        const reactionLines: ReactionVoiceLine[] = [
            { condition: () => true, lines: ["R1"] },
            { condition: () => true, lines: ["R2"] },
        ];

        const { result } = renderHook(() =>
            useBotVoiceLines(
                getOptions({
                    reactionVoiceLines: reactionLines,
                }),
            ),
        );

        const first = result.current.getVoiceLineForMove(
            createFakeVoiceLineContext({ plyNumber: 1 }),
        );
        expect(first).not.toBeNull();

        const second = result.current.getVoiceLineForMove(
            createFakeVoiceLineContext({ plyNumber: 2 }),
        );
        expect(second).toBeNull();

        const third = result.current.getVoiceLineForMove(
            createFakeVoiceLineContext({ plyNumber: 3 }),
        );
        expect(third).not.toBeNull();
    });
});
