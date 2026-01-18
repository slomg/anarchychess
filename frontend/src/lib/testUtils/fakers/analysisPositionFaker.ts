import { AnalysisPosition, GameColor } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";
import { createFakeMovePath } from "./movePathFaker";

export function createFakeAnalysisPosition(
    overrides?: Partial<AnalysisPosition>,
): AnalysisPosition {
    return {
        fen: faker.string.uuid(),
        san: faker.string.alphanumeric(10),
        legalMoves: [createFakeMovePath()],
        sideToMove: faker.helpers.enumValue(GameColor),
        ...overrides,
    };
}
