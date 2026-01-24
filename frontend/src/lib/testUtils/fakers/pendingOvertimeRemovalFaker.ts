import { PendingOvertimeRemoval } from "@/features/liveGame/lib/types";
import { createFakeLegalMoves, createRandomPoint } from "./chessboardFakers";

export function createFakePendingOvertimeRemoval(
    overrides?: Partial<PendingOvertimeRemoval>,
): PendingOvertimeRemoval {
    return {
        legalMoves: createFakeLegalMoves(),
        removedPieceAt: createRandomPoint(),
        ...overrides,
    };
}
