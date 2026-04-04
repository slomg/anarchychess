import { BoardEffectType } from "@/features/chessboard/components/boardEffects/BoardEffects";
import { ThrowAimEffect } from "@/features/chessboard/components/boardEffects/ThrowAimLine";
import { createRandomPoint } from "./chessboardFakers";

export function createFakeThrowAimEffect(
    overrides?: Partial<ThrowAimEffect>,
): ThrowAimEffect {
    return {
        type: BoardEffectType.THROW_AIM_LINE,
        from: createRandomPoint(),
        mid: createRandomPoint(),
        to: createRandomPoint(),
        ...overrides,
    };
}
