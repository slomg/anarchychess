import { createFakeBoardPieces } from "./chessboardFakers";
import { Position } from "@/features/chessboard/lib/types";
import { createFakeSan } from "./sanFaker";

export function createFakePosition(
    overrides: Partial<Position> = {},
): Position {
    return {
        pieces: createFakeBoardPieces(),
        san: createFakeSan(),
        ...overrides,
    };
}

export function createFakeStartingPosition(): Position {
    return {
        pieces: createFakeBoardPieces(),
    };
}
