import { PlayerSummary, PublicUser } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createFakePlayerSummary(
    overrides: Partial<PlayerSummary> = {},
): PlayerSummary {
    return {
        userId: faker.string.uuid(),
        userName: faker.internet.username(),
        ...overrides,
    };
}

export const createFakePlayerSummaryFromUser = (user: PublicUser) =>
    createFakePlayerSummary({
        userId: user.userId,
        userName: user.userName ?? "",
    });
