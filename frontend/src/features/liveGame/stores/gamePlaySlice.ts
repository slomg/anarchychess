import { StateCreator } from "zustand";

import type { LiveChessStore } from "./liveChessStore";
import { Clocks, GameColor } from "@/lib/apiClient";
import { ClockSnapshot } from "../lib/types";

export interface LiveChessViewer {
    userId: string;
    playerColor: GameColor | null;
}

export interface GamePlaySliceProps {
    sideToMove: GameColor;
    clockSnapshotByPly: Map<number, ClockSnapshot>;
    liveClocks: Clocks | null;

    viewer: LiveChessViewer;
}

export interface GamePlaySlice {
    sideToMove: GameColor;
    clockSnapshotByPly: Map<number, ClockSnapshot>;
    liveClocks: Clocks | null;

    viewer: LiveChessViewer;
    isPendingMoveAck: boolean;
    serverClockAheadByMs: number;

    isInteractionAllowed(): boolean;
    receiveLiveMove(
        plyNumber: number,
        sideToMove: GameColor,
        clocks?: Clocks,
    ): void;
    markPendingMoveAck(): void;
    cancelPendingMoveAch(): void;

    setClocks(plyNumber: number, clocks: Clocks): void;
    getClockSnapshot(plyNumber: number): ClockSnapshot | null;
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
        serverClockAheadByMs:
            initState.liveClocks !== null
                ? initState.liveClocks.serverTime - new Date().valueOf()
                : 0,

        isInteractionAllowed() {
            const { resultData, viewer, sideToMove, isPendingMoveAck } = get();
            if (isPendingMoveAck) {
                return false;
            }

            // allow interaction if the game is over (now in analysis mode)
            // or if it's our turn
            const isGameOver = resultData !== null;
            return isGameOver || viewer.playerColor === sideToMove;
        },

        receiveLiveMove(plyNumber, sideToMove, clocks) {
            const { decrementDrawCooldown, setClocks } = get();

            decrementDrawCooldown();
            if (clocks) {
                setClocks(plyNumber, clocks);
            }
            set((state) => {
                state.sideToMove = sideToMove;
                state.isPendingMoveAck = false;
            });
        },

        markPendingMoveAck() {
            set((state) => {
                state.isPendingMoveAck = true;
            });
        },
        cancelPendingMoveAch() {
            set((state) => {
                state.isPendingMoveAck = false;
            });
        },

        setClocks(plyNumber, clocks) {
            const serverClockAheadByMs =
                clocks.serverTime - new Date().valueOf();
            set((state) => {
                state.serverClockAheadByMs = serverClockAheadByMs;
                state.liveClocks = clocks;
                state.clockSnapshotByPly.set(plyNumber, {
                    whiteClock: clocks.whiteClock.timeLeftMs,
                    blackClock: clocks.blackClock.timeLeftMs,
                });
            });
        },
        getClockSnapshot(plyNumber) {
            const { clockSnapshotByPly, resultData } = get();
            if (resultData === null) {
                return null;
            }

            return clockSnapshotByPly.get(plyNumber) ?? null;
        },
    });
}
