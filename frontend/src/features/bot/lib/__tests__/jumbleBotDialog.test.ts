import { ReactionDialog } from "../../hooks/useBotDialog";
import { jumbleDialog, jumbleReactionDialog } from "../jumbleBotDialog";

describe("jumbleDialog", () => {
    beforeEach(() => {
        vi.spyOn(Math, "random").mockReturnValue(0.5);
    });

    it.each(["a", "ab", "abc"])(
        "should not jumble words that are too short",
        (word) => {
            expect(
                jumbleDialog([word], { wordIntensity: 1, letterIntensity: 1 }),
            ).toEqual([word]);
        },
    );

    it("should always preserve first and last letter of a long word", () => {
        const results = jumbleDialog(["elephant"], {
            wordIntensity: 1,
            letterIntensity: 1,
        });

        expect(results[0][0]).toBe("e");
        expect(results[0].slice(-1)).toBe("t");
    });

    it("should contain all the same letters after jumbling", () => {
        const result = jumbleDialog(["jumbled"], {
            wordIntensity: 1,
            letterIntensity: 1,
        });
        expect(result[0].split("").sort()).toEqual("jumbled".split("").sort());
    });

    it("should not jumble when letterIntensity is 0", () => {
        const input = ["scramble"];
        const result = jumbleDialog(input, {
            wordIntensity: 1,
            letterIntensity: 0,
        });
        expect(result).toEqual(input);
    });

    it("should not jumble when wordIntensity is 0", () => {
        const input = ["Hello world"];
        const result = jumbleDialog(input, {
            wordIntensity: 0,
            letterIntensity: 1,
        });

        expect(result).toEqual(input);
    });

    it("should jumble words when intensity is 1", () => {
        const input = ["Testing"];
        const result = jumbleDialog(input, {
            wordIntensity: 1,
            letterIntensity: 1,
        });

        expect(result[0]).not.toBe("Testing");
        expect(result[0][0]).toBe("T");
        expect(result[0].slice(-1)).toBe("g");
    });
});

describe("jumbleReactionDialog", () => {
    beforeEach(() => {
        vi.spyOn(Math, "random").mockReturnValue(0);
    });

    it("should return empty array when given empty array", () => {
        expect(
            jumbleReactionDialog([], { wordIntensity: 1, letterIntensity: 1 }),
        ).toEqual([]);
    });

    it("should preserve conditions", () => {
        const cond1 = () => true;
        const cond2 = () => false;
        const input: ReactionDialog[] = [
            { condition: cond1, lines: ["Hello world"] },
            { condition: cond2, lines: ["Goodbye world"] },
        ];
        const result = jumbleReactionDialog(input, {
            wordIntensity: 1,
            letterIntensity: 1,
        });
        expect(result[0].condition).toBe(cond1);
        expect(result[1].condition).toBe(cond2);
    });

    it("should jumble all lines within a reaction", () => {
        const input: ReactionDialog[] = [
            {
                condition: () => true,
                lines: ["Hello world", "Testing things"],
            },
        ];
        const result = jumbleReactionDialog(input, {
            wordIntensity: 1,
            letterIntensity: 1,
        });

        expect(result[0].lines[0]).not.toBe("Hello world");
        expect(result[0].lines[1]).not.toBe("Testing things");
        expect(result[0].lines[0][0]).toBe("H");
        expect(result[0].lines[1][0]).toBe("T");
        expect(result[0].lines[0].slice(-1)).toBe("d");
        expect(result[0].lines[1].slice(-1)).toBe("s");
    });

    it("should not mutate original input", () => {
        const original = "Hello world";
        const input: ReactionDialog[] = [
            { condition: () => true, lines: [original] },
        ];
        jumbleReactionDialog(input, { wordIntensity: 1, letterIntensity: 1 });
        expect(input[0].lines[0]).toBe(original);
    });
});
