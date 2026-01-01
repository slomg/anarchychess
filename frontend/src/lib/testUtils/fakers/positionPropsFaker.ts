import { faker } from "@faker-js/faker";

import { PositionProps } from "@/features/chessboard/lib/positionHistory";
import { createFakeBoardPieces, createFakeMove } from "./chessboardFakers";
import { GameColor } from "@/lib/apiClient";
import { createFakeSan } from "./sanFaker";
import constants from "@/lib/constants";

export function createFakePositionProps(
    overrides: Partial<PositionProps> = {},
): PositionProps {
    return {
        pieces: createFakeBoardPieces(),
        fen: constants.INITIAL_FEN,
        move: createFakeMove(),
        movedBy: faker.helpers.enumValue(GameColor),
        san: createFakeSan(),
        ...overrides,
    };
}
