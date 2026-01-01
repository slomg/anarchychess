import { faker } from "@faker-js/faker";

import {
    Position,
    PositionId,
} from "@/features/chessboard/lib/positionHistory";

import { createFakeBoardPieces, createFakeMove } from "./chessboardFakers";
import { GameColor } from "@/lib/apiClient";
import { createFakeSan } from "./sanFaker";

export function createFakePosition(overrides?: Partial<Position>): Position {
    return {
        pieces: createFakeBoardPieces(),
        fen: faker.string.alphanumeric(100),
        movedBy: faker.helpers.enumValue(GameColor),
        move: createFakeMove(),
        san: createFakeSan(),
        variations: [],
        positionId: crypto.randomUUID() as PositionId,
        ...overrides,
    };
}
