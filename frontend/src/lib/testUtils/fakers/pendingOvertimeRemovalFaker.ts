import { PendingOvertimeRemoval } from "@/features/liveGame/lib/types";
import { createFakeLegalMoves, createRandomPoint } from "./chessboardFakers";
import { faker } from "@faker-js/faker";

export function createFakePendingOvertimeRemoval(
    overrides?: Partial<PendingOvertimeRemoval>,
): PendingOvertimeRemoval {
    return {
        legalMoves: createFakeLegalMoves(),
        removeFrom: createRandomPoint(),
        removeAtTimestamp: faker.date.future().valueOf(),
        ...overrides,
    };
}
