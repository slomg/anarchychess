import { createFakeVoiceLineContext } from "@/lib/testUtils/fakers/voiceLineContextFaker";
import useBotVoiceLines, {
    LoreVoiceLine,
    ReactionVoiceLine,
} from "../useBotVoiceLines";

import { renderHook } from "@testing-library/react";

describe("useBotVoiceLines", () => {
    describe("Reaction Lines", () => {
        it("should return a valid reaction line when condition is met", () => {
            const lines: ReactionVoiceLine[] = [
                { condition: () => true, line: "R1" },
            ];
            const { result } = renderHook(() =>
                useBotVoiceLines(lines, [], []),
            );
            const line = result.current(createFakeVoiceLineContext());
            expect(line).toBe("R1");
        });

        it("should return null if no reaction line condition is met", () => {
            const lines: ReactionVoiceLine[] = [
                { condition: () => false, line: "R1" },
            ];
            const { result } = renderHook(() =>
                useBotVoiceLines(lines, [], []),
            );
            const line = result.current(createFakeVoiceLineContext());
            expect(line).toBeNull();
        });

        it("should not repeat reaction lines until all have been used", () => {
            const lines: ReactionVoiceLine[] = [
                { condition: () => true, line: "R1" },
                { condition: () => true, line: "R2" },
            ];
            const { result } = renderHook(() =>
                useBotVoiceLines(lines, [], []),
            );
            const first = result.current(
                createFakeVoiceLineContext({ plyNumber: 1 }),
            );
            const second = result.current(
                createFakeVoiceLineContext({ plyNumber: 2 }),
            );
            const third = result.current(
                createFakeVoiceLineContext({ plyNumber: 2 }),
            );
            expect(first).not.toBe(second);
            expect(third).toBeNull();
        });

        it("should pass the correct context to the condition", () => {
            const conditionMock = vi.fn();
            const lines = [{ condition: conditionMock, line: "R1" }];
            const { result } = renderHook(() =>
                useBotVoiceLines(lines, [], []),
            );

            const ctx = createFakeVoiceLineContext();
            result.current(ctx);

            expect(conditionMock).toHaveBeenCalledExactlyOnceWith(ctx);
        });
    });

    describe("General Lines", () => {
        it("should return null on first call (interval not reached)", () => {
            const lines = ["G1", "G2"];
            const { result } = renderHook(() =>
                useBotVoiceLines([], [], lines),
            );
            const line = result.current(createFakeVoiceLineContext());
            expect(line).toBeNull();
        });

        it("should return a general line when ply interval is reached", () => {
            const lines = ["G1", "G2"];
            const { result } = renderHook(() =>
                useBotVoiceLines([], [], lines),
            );

            result.current(createFakeVoiceLineContext({ playerType: 1 }));

            const line = result.current(
                createFakeVoiceLineContext({ plyNumber: 100 }),
            );
            expect(lines).toContain(line!);
        });

        it("should reschedule the next general line after picking a line", () => {
            const lines = ["G1", "G2"];
            const { result } = renderHook(() =>
                useBotVoiceLines([], [], lines),
            );

            expect(
                result.current(createFakeVoiceLineContext({ plyNumber: 1 })),
            ).toBeNull();

            const firstLine = result.current(
                createFakeVoiceLineContext({ plyNumber: 100 }),
            );
            expect(lines).toContain(firstLine!);

            const secondLineSamePly = result.current(
                createFakeVoiceLineContext({ plyNumber: 102 }),
            );
            expect(secondLineSamePly).toBeNull();

            const nextLine = result.current(
                createFakeVoiceLineContext({ plyNumber: 200 }),
            );
            expect(lines).toContain(nextLine);
            expect(nextLine).not.toBe(firstLine);

            const final = result.current(
                createFakeVoiceLineContext({ plyNumber: 201 }),
            );
            expect(final).toBeNull();
        });
    });

    describe("Lore Lines", () => {
        it("should return a lore line if ply number matches", () => {
            const lines: LoreVoiceLine[] = [{ onPly: 3, line: "L1" }];
            const { result } = renderHook(() =>
                useBotVoiceLines([], lines, []),
            );
            const line = result.current(
                createFakeVoiceLineContext({ plyNumber: 3 }),
            );
            expect(line).toBe("L1");
        });

        it("should not return a lore line before its ply", () => {
            const lines: LoreVoiceLine[] = [{ onPly: 5, line: "L1" }];
            const { result } = renderHook(() =>
                useBotVoiceLines([], lines, []),
            );
            const line = result.current(
                createFakeVoiceLineContext({ plyNumber: 3 }),
            );
            expect(line).toBeNull();
        });

        it("should not repeat lore lines for previous plies", () => {
            const lines: LoreVoiceLine[] = [
                { onPly: 3, line: "L1" },
                { onPly: 3, line: "L2" },
            ];
            const { result } = renderHook(() =>
                useBotVoiceLines([], lines, []),
            );

            const first = result.current(
                createFakeVoiceLineContext({ plyNumber: 3 }),
            );
            const second = result.current(
                createFakeVoiceLineContext({ plyNumber: 3 }),
            );

            expect(["L1", "L2"]).toContain(first);
            expect(second).toBeNull();
        });

        it("should pick only one lore line per ply even if multiple are available", () => {
            const lines: LoreVoiceLine[] = [
                { onPly: 3, line: "L1" },
                { onPly: 3, line: "L2" },
                { onPly: 3, line: "L3" },
            ];

            const { result } = renderHook(() =>
                useBotVoiceLines([], lines, []),
            );

            const firstLine = result.current(
                createFakeVoiceLineContext({ plyNumber: 3 }),
            );
            expect(["L1", "L2", "L3"]).toContain(firstLine);

            const secondLine = result.current(
                createFakeVoiceLineContext({ plyNumber: 3 }),
            );
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
        const line = result.current(
            createFakeVoiceLineContext({ plyNumber: 1 }),
        );
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

        result.current(createFakeVoiceLineContext({ plyNumber: 1 }));
        const line = result.current(
            createFakeVoiceLineContext({ plyNumber: 100 }),
        );
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

        const line = result.current(
            createFakeVoiceLineContext({ plyNumber: 1 }),
        );
        expect(line).toBe("L1");
    });
});
