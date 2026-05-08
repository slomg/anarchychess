import { StateCreator } from "zustand";

import { LogicalPoint, ScreenPoint } from "@/features/point/types";
import { pointEquals } from "@/features/point/pointUtils";
import type { ChessboardStore } from "./chessboardStore";
import BoardPieces from "../lib/boardPieces";

export interface SetupModeMoveEvent {
    from: LogicalPoint;
    to: LogicalPoint;
}

export interface SetupModeSlice {
    isSetupMode: boolean;

    setSetupMode(setupMode: boolean): void;
    makeSetupModeMove(to: ScreenPoint): void;
}

export const createSetupModeSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    SetupModeSlice
> = (set, get) => ({
    isSetupMode: false,

    setSetupMode(setupMode) {
        set((state) => {
            state.isSetupMode = setupMode;
        });
    },

    makeSetupModeMove(to) {
        const {
            pieces,
            selectedPieceId,
            screenToLogicalPoint,
            overrideRoot,
            resetLastMove,
        } = get();

        if (!selectedPieceId) {
            return;
        }

        const dest = screenToLogicalPoint(to);
        if (!dest) {
            return;
        }

        const piece = pieces.getById(selectedPieceId);
        if (!piece) {
            console.warn(
                `Could not find selected piece ${selectedPieceId} on setup move`,
            );
            return;
        }

        if (pointEquals(piece.position, dest)) {
            return;
        }

        const newPieces = new BoardPieces(pieces);
        newPieces.movePiece(selectedPieceId, dest);

        resetLastMove();
        overrideRoot({ pieces: newPieces });
    },
});
