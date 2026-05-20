import { render } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    PersistentBoardEffectType,
    TransientBoardEffectType,
} from "@/features/chessboard/stores/boardEffectsSlice";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { createRandomPoint } from "@/lib/testUtils/fakers/chessboardFakers";
import BoardEffectsWebGLFallback from "../BoardEffectsWebGLFallback";
import { GameColor } from "@/lib/apiClient";

describe("BoardEffectsWebGLFallback", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    it("should complete all transient effects", async () => {
        const { addTransientBoardEffect, addPersistentBoardEffect } =
            store.getState();
        const promise1 = addTransientBoardEffect({
            type: TransientBoardEffectType.EXPLOSION,
            at: createRandomPoint(),
        });
        const promise2 = addTransientBoardEffect({
            type: TransientBoardEffectType.QUEENTUM_TUNNELLING,
            queenPosition: createRandomPoint(),
            antiqueenPosition: createRandomPoint(),
            color: GameColor.WHITE,
        });
        const promise3 = addTransientBoardEffect({
            type: TransientBoardEffectType.PAWN_THROW,
            from: createRandomPoint(),
            to: createRandomPoint(),
            color: GameColor.WHITE,
        });
        const persistent = addPersistentBoardEffect({
            type: PersistentBoardEffectType.THROW_AIM_LINE,
            from: createRandomPoint(),
            mid: createRandomPoint(),
            to: createRandomPoint(),
        });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <BoardEffectsWebGLFallback />
            </ChessboardStoreContext.Provider>,
        );

        await promise1.promise;
        await promise2.promise;
        await promise3.promise;
        expect(store.getState().activeTransientBoardEffects.size).toBe(0);
        expect(
            store.getState().activePersistentBoardEffects.get(persistent),
        ).toBeDefined();
    });
});
