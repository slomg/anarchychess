import { GameColor, MoveSnapshot } from "@/lib/apiClient";
import { createFakeMovePath } from "./movePathFaker";
import { faker } from "@faker-js/faker";
import { createFakeSan } from "./sanFaker";

export function createFakeMoveSnapshot(
    overrides: Partial<MoveSnapshot> = {},
): MoveSnapshot {
    return {
        path: createFakeMovePath(),
        fen: faker.string.alphanumeric(100),
        movedBy: faker.helpers.enumValue(GameColor),
        san: createFakeSan(),
        timeLeft: faker.number.int({ min: 100, max: 10000 }),
        ...overrides,
    };
}
