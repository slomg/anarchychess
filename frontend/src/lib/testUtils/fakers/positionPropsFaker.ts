import { PositionProps } from "@/features/chessboard/lib/positionHistory";
import { createFakeBoardPieces, createFakeMove } from "./chessboardFakers";
import { createFakeSan } from "./sanFaker";
import constants from "@/lib/constants";

export function createFakePositionProps(
    overrides: Partial<PositionProps> = {},
): PositionProps {
    return {
        pieces: createFakeBoardPieces(),
        fen: constants.INITIAL_FEN,
        move: createFakeMove(),
        san: createFakeSan(),
        ...overrides,
    };
}
