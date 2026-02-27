import {
    createFakeBoardPieces,
    createFakeMove,
} from "@/lib/testUtils/fakers/chessboardFakers";
import useBotVoiceLines, {
    LoreVoiceLine,
    ReactionVoiceLine,
} from "../useBotVoiceLines";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { Move } from "@/features/chessboard/lib/types";
import { renderHook } from "@testing-library/react";

describe("useBotVoiceLines", () => {
    let move: Move;
    let prevPieces: BoardPieces;

    beforeEach(() => {
        move = createFakeMove();
        prevPieces = createFakeBoardPieces();
    });

    describe("Reaction Lines", () => {
        it("should return a valid reaction line when condition is met", () => {
            const reactionLines: ReactionVoiceLine[] = [
                { condition: () => true, line: "R1" },
            ];
            const { result } = renderHook(() =>
                useBotVoiceLines(reactionLines, [], []),
            );
            const line = result.current(move, prevPieces, 1);
            expect(line).toBe("R1");
        });

        it("should return null if no reaction line condition is met", () => {
            const reactionLines: ReactionVoiceLine[] = [
                { condition: () => false, line: "R1" },
            ];
            const { result } = renderHook(() =>
                useBotVoiceLines(reactionLines, [], []),
            );
            const line = result.current(move, prevPieces, 1);
            expect(line).toBeNull();
        });

        it("should not repeat reaction lines until all have been used", () => {
            const reactionLines: ReactionVoiceLine[] = [
                { condition: () => true, line: "R1" },
                { condition: () => true, line: "R2" },
            ];
            const { result } = renderHook(() =>
                useBotVoiceLines(reactionLines, [], []),
            );
            const first = result.current(move, prevPieces, 1);
            const second = result.current(move, prevPieces, 2);
            const third = result.current(move, prevPieces, 3);
            expect(first).not.toBe(second);
            expect(third).toBeNull();
        });
    });

    describe("General Lines", () => {
        it("should return null on first call (interval not reached)", () => {
            const generalLines = ["G1", "G2"];
            const { result } = renderHook(() =>
                useBotVoiceLines([], [], generalLines),
            );
            const line = result.current(move, prevPieces, 1);
            expect(line).toBeNull();
        });

        it("should return a general line when ply interval is reached", () => {
            const generalLines = ["G1", "G2"];
            const { result } = renderHook(() =>
                useBotVoiceLines([], [], generalLines),
            );

            result.current(move, prevPieces, 1);

            const line = result.current(move, prevPieces, 100);
            expect(generalLines).toContain(line!);
        });

        it("should reschedule the next general line after picking a line", () => {
            const generalLines = ["G1", "G2"];
            const { result } = renderHook(() =>
                useBotVoiceLines([], [], generalLines),
            );

            expect(result.current(move, prevPieces, 1)).toBeNull();

            const firstLine = result.current(move, prevPieces, 100);
            expect(generalLines).toContain(firstLine!);

            const secondLineSamePly = result.current(move, prevPieces, 102);
            expect(secondLineSamePly).toBeNull();

            const nextLine = result.current(move, prevPieces, 200);
            expect(generalLines).toContain(nextLine);
            expect(nextLine).not.toBe(firstLine);

            const final = result.current(move, prevPieces, 201);
            expect(final).toBeNull();
        });

        it("should not repeat general lines until all have been used", () => {
            const generalLines = ["G1", "G2"];
            const { result } = renderHook(() =>
                useBotVoiceLines([], [], generalLines),
            );

            result.current(move, prevPieces, 1);

            const first = result.current(move, prevPieces, 100);
            const second = result.current(move, prevPieces, 101);
            const third = result.current(move, prevPieces, 101);
            expect(first).not.toBe(second);
            expect(third).toBeNull();
        });
    });

    describe("Lore Lines", () => {
        it("should return a lore line if ply number matches", () => {
            const loreLines: LoreVoiceLine[] = [{ onPly: 3, line: "L1" }];
            const { result } = renderHook(() =>
                useBotVoiceLines([], loreLines, []),
            );
            const line = result.current(move, prevPieces, 3);
            expect(line).toBe("L1");
        });

        it("should not return a lore line before its ply", () => {
            const loreLines: LoreVoiceLine[] = [{ onPly: 5, line: "L1" }];
            const { result } = renderHook(() =>
                useBotVoiceLines([], loreLines, []),
            );
            const line = result.current(move, prevPieces, 3);
            expect(line).toBeNull();
        });

        it("should not repeat lore lines for previous plies", () => {
            const loreLines: LoreVoiceLine[] = [
                { onPly: 3, line: "L1" },
                { onPly: 3, line: "L2" },
            ];
            const { result } = renderHook(() =>
                useBotVoiceLines([], loreLines, []),
            );

            const first = result.current(move, prevPieces, 3);
            const second = result.current(move, prevPieces, 3);

            expect(["L1", "L2"]).toContain(first);
            expect(second).toBeNull();
        });

        it("should pick only one lore line per ply even if multiple are available", () => {
            const loreLines: LoreVoiceLine[] = [
                { onPly: 3, line: "L1" },
                { onPly: 3, line: "L2" },
                { onPly: 3, line: "L3" },
            ];

            const { result } = renderHook(() =>
                useBotVoiceLines([], loreLines, []),
            );

            const firstLine = result.current(move, prevPieces, 3);
            expect(["L1", "L2", "L3"]).toContain(firstLine);

            const secondLine = result.current(move, prevPieces, 3);
            expect(secondLine).toBeNull();
        });
    });

    it("should prioritize reaction lines over general and lore lines", () => {
        const reactionLines: ReactionVoiceLine[] = [
            { condition: () => true, line: "R1" },
        ];
        const loreLines: LoreVoiceLine[] = [{ onPly: 1, line: "L1" }];
        const generalLines = ["G1"];

        const { result } = renderHook(() =>
            useBotVoiceLines(reactionLines, loreLines, generalLines),
        );
        const line = result.current(move, prevPieces, 1);
        expect(line).toBe("R1");
    });

    it("should fallback to general lines if no reaction lines trigger", () => {
        const reactionLines: ReactionVoiceLine[] = [
            { condition: () => false, line: "R1" },
        ];
        const generalLines = ["G1"];

        const { result } = renderHook(() =>
            useBotVoiceLines(reactionLines, [], generalLines),
        );

        result.current(move, prevPieces, 1);
        const line = result.current(move, prevPieces, 100);
        expect(line).toBe("G1");
    });

    it("should fallback to lore lines if no reaction or general lines trigger", () => {
        const reactionLines: ReactionVoiceLine[] = [
            { condition: () => false, line: "R1" },
        ];
        const loreLines: LoreVoiceLine[] = [{ onPly: 1, line: "L1" }];
        const { result } = renderHook(() =>
            useBotVoiceLines(reactionLines, loreLines, []),
        );
        const line = result.current(move, prevPieces, 1);
        expect(line).toBe("L1");
    });
});
