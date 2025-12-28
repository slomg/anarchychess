import { StateCreator } from "zustand";

import type { LiveChessStore } from "./liveChessStore";
import { Clocks, GameColor } from "@/lib/apiClient";

export interface LiveChessViewer {
    userId: string;
    playerColor: GameColor | null;
}

export interface GamePlaySliceProps {
    sideToMove: GameColor;
    clocks: Clocks;

    viewer: LiveChessViewer;
}

export interface GamePlaySlice extends GamePlaySliceProps {
    isPendingMoveAck: boolean;

    receiveMove(clocks: Clocks, sideToMove: GameColor): void;
    markPendingMoveAck(): void;

    setClocks(clocks: Clocks): void;
}

export function createGamePlaySlice(
    initState: GamePlaySliceProps,
): StateCreator<
    LiveChessStore,
    [["zustand/immer", never], never],
    [],
    GamePlaySlice
> {
    return (set, get) => ({
        ...initState,

        isPendingMoveAck: false,

        receiveMove(clocks, sideToMove) {
            const { decrementDrawCooldown } = get();

            decrementDrawCooldown();
            set((state) => {
                state.clocks = clocks;
                state.sideToMove = sideToMove;
                state.isPendingMoveAck = false;
            });
        },

        markPendingMoveAck() {
            set((state) => {
                state.isPendingMoveAck = true;
            });
        },

        setClocks(clocks) {
            set((state) => {
                state.clocks = clocks;
            });
        },
    });
}
