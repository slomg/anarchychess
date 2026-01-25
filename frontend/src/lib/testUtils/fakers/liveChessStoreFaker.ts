import { faker } from "@faker-js/faker";

import { LiveChessStoreProps } from "@/features/liveGame/stores/liveChessStore";
import { GameColor, GamePlayer, PoolType, TimeControl } from "@/lib/apiClient";
import { createFakePlayer } from "./playerFaker";
import { createFakeClocks } from "./clocksFaker";
import constants from "@/lib/constants";

export function createFakeLiveChessStoreProps(
    override: Partial<LiveChessStoreProps> & {
        viewerColor?: GameColor | null;
    } = {},
): LiveChessStoreProps {
    const whitePlayer = createFakePlayer(GameColor.WHITE);
    const blackPlayer = createFakePlayer(GameColor.BLACK);

    const viewerColor =
        override.viewerColor === undefined
            ? GameColor.WHITE
            : override.viewerColor;
    const viewer = createFakeViewer(whitePlayer, blackPlayer, viewerColor);

    return {
        gameToken: faker.string.alpha(16),
        initialFen: constants.INITIAL_FEN,

        sideToMove: faker.helpers.enumValue(GameColor),
        sourceRevision: faker.number.int({ min: 5, max: 100 }),
        viewer,
        whitePlayer: whitePlayer,
        blackPlayer: blackPlayer,

        pool: {
            poolType: faker.helpers.enumValue(PoolType),
            timeControl: {
                baseSeconds: faker.number.int({ min: 60, max: 1200 }),
                incrementSeconds: faker.number.int({ min: 3, max: 30 }),
                type: faker.helpers.enumValue(TimeControl),
            },
        },

        clocks: createFakeClocks(),
        drawState: {
            activeRequester: null,
            whiteCooldown: 0,
            blackCooldown: 0,
        },

        whiteOvertime: null,
        blackOvertime: null,

        resultData: null,

        ...override,
    };
}

function createFakeViewer(
    whitePlayer: GamePlayer,
    blackPlayer: GamePlayer,
    viewerColor: GameColor | null,
) {
    switch (viewerColor) {
        case GameColor.WHITE:
            return { playerColor: GameColor.WHITE, userId: whitePlayer.userId };
        case GameColor.BLACK:
            return { playerColor: GameColor.BLACK, userId: blackPlayer.userId };
        default:
            return { playerColor: null, userId: faker.string.uuid() };
    }
}
