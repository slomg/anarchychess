import { ThrowAimEffect } from "@/features/chessboard/components/boardEffects/ThrowAimLine";
import { PersistentBoardEffectType } from "@/features/chessboard/stores/boardEffectsSlice";
import { createRandomPoint } from "./chessboardFakers";

export function createFakeThrowAimEffect(
    overrides?: Partial<ThrowAimEffect>,
): ThrowAimEffect {
    return {
        type: PersistentBoardEffectType.THROW_AIM_LINE,
        from: createRandomPoint(),
        mid: createRandomPoint(),
        to: createRandomPoint(),
        ...overrides,
    };
}
