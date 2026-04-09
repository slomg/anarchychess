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
        const { resolvePromotion, resolveNextIntermediate, pendingThrow } =
            get();

        resolvePromotion?.(null);
        resolveNextIntermediate?.(null);
        pendingThrow?.resolve(null);
    },

    discardPromptsForPiece(pieceId) {
        const {
            pendingPromotion,
            resolvePromotion,
            pendingIntermediate,
            resolveNextIntermediate,
            pendingThrow,
        } = get();

        if (pendingPromotion?.piece.id === pieceId) {
            resolvePromotion?.(null);
        }

        if (pendingIntermediate?.pieceId === pieceId) {
            resolveNextIntermediate?.(null);
        }

        if (pendingThrow?.piece.id === pieceId) {
            pendingThrow.resolve(null);
        }
    },
});
