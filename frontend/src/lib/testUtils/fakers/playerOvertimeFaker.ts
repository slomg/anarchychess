import { PlayerOvertime } from "@/features/liveGame/lib/types";
import { faker } from "@faker-js/faker";
import { createFakePendingOvertimeRemoval } from "./pendingOvertimeRemovalFaker";

export function createFakePlayerOvertime(
    overrides?: Partial<PlayerOvertime>,
): PlayerOvertime {
    return {
        secondRemainderMs: faker.number.int({ min: 0, max: 999 }),
        pendingRemoval: Array.from({
            length: faker.number.int({ min: 1, max: 3 }),
        }).map(() => createFakePendingOvertimeRemoval()),
        ...overrides,
    };
}
