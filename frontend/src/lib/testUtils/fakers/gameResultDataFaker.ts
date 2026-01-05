import { GameResult, GameResultData } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createFakeGameResultData(
    overrides?: Partial<GameResultData>,
): GameResultData {
    return {
        result: faker.helpers.enumValue(GameResult),
        resultDescription: faker.lorem.sentence(),
        whiteRatingChange: faker.number.int({ min: -10, max: 10 }),
        blackRatingChange: faker.number.int({ min: -10, max: 10 }),
        ...overrides,
    };
}
