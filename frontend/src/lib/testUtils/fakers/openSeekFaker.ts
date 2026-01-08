import { faker } from "@faker-js/faker";

import { OpenSeek } from "@/features/lobby/lib/types";
import { createFakePoolKey } from "./poolKeyFaker";

export default function createFakeOpenSeek(
    overrides: Partial<OpenSeek> = {},
): OpenSeek {
    return {
        userId: faker.string.uuid(),
        userName: faker.internet.username(),
        pool: createFakePoolKey(),
        ...overrides,
    };
}
