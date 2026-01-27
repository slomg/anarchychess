import { StateCreator } from "zustand";

import type { ChessboardStore } from "./chessboardStore";
import { PieceID } from "../lib/types";

export interface PromptSlice {
    discardAllPrompts(): void;
    discardPromptsForPiece(pieceId: PieceID): void;
}

export const createPromptSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    PromptSlice
> = (_, get) => ({
    discardAllPrompts() {
        const { resolvePromotion, resolveNextIntermediate } = get();

        resolvePromotion?.(null);
        resolveNextIntermediate?.(null);
    },

    discardPromptsForPiece(pieceId) {
        const {
            pendingPromotion,
            resolvePromotion,
            pendingIntermediate,
            resolveNextIntermediate,
        } = get();

        if (pendingPromotion?.piece.id === pieceId) {
            resolvePromotion?.(null);
        }

        if (pendingIntermediate?.pieceId === pieceId) {
            resolveNextIntermediate?.(null);
        }
    },
});
