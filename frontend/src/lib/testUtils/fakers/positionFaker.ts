import { faker } from "@faker-js/faker";

import { PositionId } from "@/features/chessboard/lib/position";
import { Position } from "@/features/chessboard/lib/position";

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
        ply: faker.number.int({ min: 0, max: 100 }),

        positionId: crypto.randomUUID() as PositionId,
        variations: [],
        subVariationBySan: new Map(),
        *[Symbol.iterator]() {},
        ...overrides,
    };
}
