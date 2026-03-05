import { DialogContext } from "@/features/bot/hooks/useBotDialog";
import { createFakeBoardPieces, createFakeMove } from "./chessboardFakers";
import { faker } from "@faker-js/faker";
import { PlayerType } from "@/features/liveGame/lib/types";

export function createFakeDialogContext(
    overrides?: Partial<DialogContext>,
): DialogContext {
    return {
        move: createFakeMove(),
        prevPieces: createFakeBoardPieces(),
        playerType: faker.helpers.enumValue(PlayerType),
        plyNumber: faker.number.int({ min: 1, max: 100 }),
        evalForBot: faker.number.int({ min: -10_000, max: 10_000 }),
        prevEvalForBot: faker.number.int({ min: -10_000, max: 10_000 }),
        ...overrides,
    };
}
