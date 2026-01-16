import { createFakeClockPlayer } from "./createFakeClockPlayer";
import { Clocks } from "@/lib/apiClient";

export function createFakeClocks(overrides: Partial<Clocks> = {}): Clocks {
    return {
        whiteClock: createFakeClockPlayer(),
        blackClock: createFakeClockPlayer(),
        lastUpdated: Date.now().valueOf(),
        serverTime: Date.now().valueOf(),
        isFrozen: false,
        ...overrides,
    };
}
