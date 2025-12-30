import { PositionProps } from "@/features/chessboard/lib/positionHistory";
import { createFakeBoardPieces, createFakeMove } from "./chessboardFakers";
import { createFakeSan } from "./sanFaker";

export function createFakePositionProps(
    overrides: Partial<PositionProps> = {},
): PositionProps {
    return {
        pieces: createFakeBoardPieces(),
        move: createFakeMove(),
        san: createFakeSan(),
        ...overrides,
    };
}
