import { StateCreator } from "zustand";

import { LogicalPoint, ScreenPoint } from "@/features/point/types";
import createDefaultChessboard from "../lib/defaultBoard";
import { pointEquals } from "@/features/point/pointUtils";
import type { ChessboardStore } from "./chessboardStore";
import { GameColor, PieceType } from "@/lib/apiClient";
import { createPieceId } from "../lib/pieceUtils";
import BoardPieces from "../lib/boardPieces";

export interface SetupModeMoveEvent {
    from: LogicalPoint;
    to: LogicalPoint;
}

export interface SetupModeSlice {
    isSetupMode: boolean;

    setSetupMode(setupMode: boolean): void;
    makeSetupModeMove(to: ScreenPoint): void;
    addSetupModePiece(
        pieceType: PieceType,
        color: GameColor | null,
        at: ScreenPoint,
    ): void;
    clearSetupModeBoard(): void;
    resetSetupModeBoard(): void;
}

export const createSetupModeSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    SetupModeSlice
> = (set, get) => ({
    isSetupMode: false,

    setSetupMode(setupMode) {
        get().discardAllPrompts();
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

        const piece = pieces.getById(selectedPieceId);
        if (!piece) {
            console.warn(
                `Could not find selected piece ${selectedPieceId} on setup move`,
            );
            return;
        }

        const dest = screenToLogicalPoint(to);
        if (pointEquals(piece.position, dest)) {
            return;
        }

        const newPieces = new BoardPieces(pieces);
        if (dest) {
            newPieces.movePiece(selectedPieceId, dest);
        } else {
            newPieces.remove(selectedPieceId);
        }

        resetLastMove();
        overrideRoot({ pieces: newPieces });
    },

    addSetupModePiece(pieceType, color, at) {
        const { pieces, screenToLogicalPoint, overrideRoot, resetLastMove } =
            get();

        const dest = screenToLogicalPoint(at);
        if (!dest) {
            return;
        }

        const newPieces = new BoardPieces(pieces);
        newPieces.add({
            id: createPieceId(),
            type: pieceType,
            color,
            position: dest,
            stunnedForTurns: 0,
            hasMoved: false,
        });

        resetLastMove();
        overrideRoot({ pieces: newPieces });
    },

    clearSetupModeBoard() {
        const { overrideRoot, resetLastMove } = get();

        resetLastMove();
        overrideRoot({ pieces: new BoardPieces() });
    },
    resetSetupModeBoard() {
        const { overrideRoot, resetLastMove } = get();

        resetLastMove();
        overrideRoot({ pieces: createDefaultChessboard() });
    },
});
