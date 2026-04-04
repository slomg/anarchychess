import { StateCreator } from "zustand";

import type { ChessboardStore } from "./chessboardStore";
import { BoardEffect } from "../components/boardEffects/BoardEffects";

export type BoardEffectId = string & "BoardEffect";

export interface BoardEffectsSlice {
    activeBoardEffects: Map<string, BoardEffect>;

    addBoardEffect(effect: BoardEffect): BoardEffectId;
    updateBoardEffect(id: BoardEffectId, effect: BoardEffect): void;
    removeBoardEffect(id?: BoardEffectId | null): void;
}

export const createBoardEffectsSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    BoardEffectsSlice
> = (set, get) => ({
    activeBoardEffects: new Map(),

    addBoardEffect(effect) {
        const id = crypto.randomUUID();
        set((state) => {
            state.activeBoardEffects.set(id, effect);
        });
        return id as BoardEffectId;
    },

    updateBoardEffect(id, effect) {
        const { activeBoardEffects } = get();
        if (!activeBoardEffects.has(id)) {
            return;
        }
        set((state) => {
            state.activeBoardEffects.set(id, effect);
        });
    },

    removeBoardEffect(id) {
        if (id == null) {
            return;
        }
        set((state) => {
            state.activeBoardEffects.delete(id);
        });
    },
});
