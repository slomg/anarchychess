import { ReactionDialog } from "../../hooks/useBotDialog";
import { jumbleDialog, jumbleReactionDialog } from "../jumbleBotDialog";

describe("jumbleDialog", () => {
    beforeEach(() => {
        vi.spyOn(Math, "random").mockReturnValue(0);
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

    it("preserves condition", () => {
        const condition = () => true;
        const input: ReactionDialog[] = [{ condition, lines: ["Hello"] }];

        const result = jumbleReactionDialog(input, {
            wordIntensity: 0,
            letterIntensity: 0,
        });

        expect(result[0].condition).toBe(condition);
    });

    it("jumbles lines", () => {
        const input: ReactionDialog[] = [
            { condition: () => true, lines: ["Hello"] },
        ];

        const result = jumbleReactionDialog(input, {
            wordIntensity: 1,
            letterIntensity: 1,
        });

        expect(result[0].lines[0]).not.toBe("Hello");
        expect(result[0].lines[0][0]).toBe("H");
        expect(result[0].lines[0].slice(-1)).toBe("o");
    });
});
