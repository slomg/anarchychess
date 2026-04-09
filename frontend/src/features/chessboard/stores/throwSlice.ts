import { StateCreator } from "zustand";

import type { ChessboardStore } from "./chessboardStore";
import { LogicalPoint } from "@/features/point/types";
import { Move, Piece } from "../lib/types";
import { pointEquals } from "@/features/point/pointUtils";

export interface PendingThrow {
    piece: Piece;
    points: LogicalPoint[];
    throwerOrigin: LogicalPoint;

    resolve: (point: LogicalPoint | null) => void;
}

export interface ThrowSlice {
    pendingThrow: PendingThrow | null;

    promptThrow(
        throwerOrigin: LogicalPoint,
        piece: Piece,
        moves: Move[],
    ): Promise<Move | null>;
}

export const createThrowSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    ThrowSlice
> = (set) => ({
    pendingThrow: null,
    resolveThrow: null,

    async promptThrow(throwerOrigin, piece, moves) {
        try {
            const result = await new Promise<LogicalPoint | null>((resolve) => {
                set((state) => {
                    state.pendingThrow = {
                        throwerOrigin,
                        piece,
                        points: moves.map((x) => x.to),
                        resolve,
                    };
                });
            });
            if (!result) {
                return null;
            }

            return moves.find((move) => pointEquals(move.to, result)) ?? null;
        } finally {
            set((state) => {
                state.pendingThrow = null;
            });
        }
    },
});
