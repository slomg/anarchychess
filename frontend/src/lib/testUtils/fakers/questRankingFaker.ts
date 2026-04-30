import { QuestRanking } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createFakeQuestRanking(
    overrides?: Partial<QuestRanking>,
): QuestRanking {
    return {
        totalQuestPoints: faker.number.int({ min: 0, max: 100 }),
        totalRank: faker.number.int({ min: 101, max: 200 }),
        monthlyQuestPoints: faker.number.int({ min: 201, max: 300 }),
        monthlyRank: faker.number.int({ min: 301, max: 400 }),
        ...overrides,
    };
}
