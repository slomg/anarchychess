import { ClockPlayer } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createFakeClockPlayer(
    overrides?: Partial<ClockPlayer>,
): ClockPlayer {
    return {
        timeLeftMs: faker.number.int({ min: 10000, max: 100000 }),
        timeUntilAbandonMs: null,
        isInGracePeriod: false,
        ...overrides,
    };
}
