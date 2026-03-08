import { faker } from "@faker-js/faker";

import { invertColor, randomizeColor } from "@/lib/utils/chessUtils";
import { createFakeMoveSnapshot } from "./moveSnapshotFaker";
import { BotGameState, BotType, GameColor } from "@/lib/apiClient";
import { createFakeMovePath } from "./movePathFaker";
import { createFakePlayer } from "./playerFaker";
import constants from "@/lib/constants";

export function createFakeBotGameState(
    overrides?: Partial<BotGameState>,
): BotGameState {
    const botColor = randomizeColor();
    const botPlayer = createFakePlayer(botColor, {
        userId: "bot:anarchybot",
        userName: "Anarchy Bot",
        countryCode: "XX",
        rating: 161660,
    });
    const player = createFakePlayer(invertColor(botColor));

    return {
        whitePlayer: botColor === GameColor.WHITE ? botPlayer : player,
        blackPlayer: botColor === GameColor.BLACK ? botPlayer : player,
        botColor,
        botType: faker.helpers.enumValue(BotType),
        sideToMove: faker.helpers.enumValue(GameColor),

        initialFen: constants.INITIAL_FEN,
        moveHistory: Array.from({
            length: faker.number.int({ min: 1, max: 5 }),
        }).map(() => createFakeMoveSnapshot()),
        legalMoves: Array.from({
            length: faker.number.int({ min: 1, max: 5 }),
        }).map(() => createFakeMovePath()),
        ...overrides,
    };
}
