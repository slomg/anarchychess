import { VoteOption } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createFakeVoteOption(
    overrides?: Partial<VoteOption>,
): VoteOption {
    return {
        optionKey: faker.string.uuid(),
        name: faker.lorem.words(),
        description: faker.lorem.paragraph(),
        ...overrides,
    };
}
