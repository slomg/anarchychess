import { faker } from "@faker-js/faker";

import { PawnThrowEffect } from "@/features/chessboard/components/boardEffects/PawnThrowEffect";
import { TransientBoardEffectType } from "@/features/chessboard/stores/boardEffectsSlice";
import { createRandomPoint } from "./chessboardFakers";
import { GameColor } from "@/lib/apiClient";

export function createFakePawnThrowEffect(
    overrides?: Partial<PawnThrowEffect>,
): PawnThrowEffect {
    return {
        type: TransientBoardEffectType.PAWN_THROW,
        from: createRandomPoint(),
        to: createRandomPoint(),
        color: faker.helpers.enumValue(GameColor),
        ...overrides,
    };
}
