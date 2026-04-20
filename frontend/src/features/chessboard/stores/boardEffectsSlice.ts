import { StateCreator } from "zustand";

import type { ThrowAimEffect } from "../components/boardEffects/ThrowAimLine";
import type { ExplosionEffect } from "../components/boardEffects/Explosion";
import type { PawnThrowEffect } from "../components/boardEffects/PawnThrow";
import type { ChessboardStore } from "./chessboardStore";

export enum PersistentBoardEffectType {
    THROW_AIM_LINE,
}

export enum TransientBoardEffectType {
    PAWN_THROW,
    EXPLOSION,
}

export type PersistentBoardEffect = ThrowAimEffect;
export type TransientBoardEffect = PawnThrowEffect | ExplosionEffect;

export type BoardEffectId = string & "BoardEffect";

interface ManagedTransientBoardEffect<T extends TransientBoardEffect> {
    value: T;
    settle: () => void;
    complete: () => void;
}

export interface BoardEffectsSlice {
    activePersistentBoardEffects: Map<BoardEffectId, PersistentBoardEffect>;
    activeTransientBoardEffects: Map<
        BoardEffectId,
        ManagedTransientBoardEffect<TransientBoardEffect>
    >;

    addPersistentBoardEffect(effect: PersistentBoardEffect): BoardEffectId;
    updatePersistentBoardEffect(
        id: BoardEffectId,
        effect: PersistentBoardEffect,
    ): void;
    removePersistentBoardEffect(id: BoardEffectId): void;

    addTransientBoardEffect(effect: TransientBoardEffect): {
        id: BoardEffectId;
        promise: Promise<void>;
    };
    resolveTransientBoardEffect(id: BoardEffectId): void;
    resolveAllTransientBoardEffects(): void;
}

export const createBoardEffectsSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    BoardEffectsSlice
> = (set, get) => ({
    activePersistentBoardEffects: new Map(),
    activeTransientBoardEffects: new Map(),

    addPersistentBoardEffect(effect) {
        const id = crypto.randomUUID() as BoardEffectId;
        set((state) => {
            state.activePersistentBoardEffects.set(id, effect);
        });
        return id;
    },

    updatePersistentBoardEffect(id, effect) {
        const { activePersistentBoardEffects } = get();
        if (!activePersistentBoardEffects.has(id)) {
            return;
        }
        set((state) => {
            state.activePersistentBoardEffects.set(id, effect);
        });
    },

    removePersistentBoardEffect(id) {
        set((state) => {
            state.activePersistentBoardEffects.delete(id);
        });
    },

    addTransientBoardEffect(effect) {
        const id = crypto.randomUUID() as BoardEffectId;

        let resolveFn!: () => void;
        const promise = new Promise<void>((resolve) => {
            resolveFn = resolve;
        });

        const complete = () => {
            set((state) => {
                state.activeTransientBoardEffects.delete(id);
            });
            resolveFn();
        };

        set((state) => {
            state.activeTransientBoardEffects.set(id, {
                value: effect,
                settle: resolveFn,
                complete,
            });
        });

        return {
            id,
            promise,
        };
    },

    resolveTransientBoardEffect(id) {
        const { activeTransientBoardEffects } = get();
        const effect = activeTransientBoardEffects.get(id);
        if (!effect) {
            return;
        }

        effect.complete();
    },

    resolveAllTransientBoardEffects() {
        const { activeTransientBoardEffects } = get();
        for (const effect of activeTransientBoardEffects.values()) {
            effect.complete();
        }
    },
});
