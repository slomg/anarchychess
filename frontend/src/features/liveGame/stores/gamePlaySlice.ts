import { StateCreator } from "zustand";

import { Clocks, GameColor } from "@/lib/apiClient";
import type { LiveChessStore } from "./liveChessStore";
import { Position } from "../lib/types";
import LegalMoves from "@/features/chessboard/lib/legalMoves";

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

    receiveMove(
        position: Position,
        clocks: Clocks,
        sideToMove: GameColor,
    ): void;
    resetLegalMovesForOpponentTurn(): void;
    receiveLegalMoves(legalMoves: LegalMoves): void;
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

        receiveMove(position, clocks, sideToMove) {
            const { decrementDrawCooldown, receivePosition } = get();

            decrementDrawCooldown();
            receivePosition(position);
            set((state) => {
                state.clocks = clocks;
                state.sideToMove = sideToMove;
                state.isPendingMoveAck = false;
            });
        },
        resetLegalMovesForOpponentTurn() {
            set((state) => {
                state.latestLegalMoves = new LegalMoves();
            });
        },
        receiveLegalMoves(legalMoves) {
            set((state) => {
                state.latestLegalMoves = legalMoves;
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
