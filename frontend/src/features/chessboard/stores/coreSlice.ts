import type { ChessboardProps, ChessboardStore } from "./chessboardStore";
import LegalMoves from "../lib/legalMoves";
import { StateCreator } from "zustand";

export interface CoreSlice {
    resetState(initState: ChessboardProps): void;
    disableMovement(): void;
}

export const createCoreSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    CoreSlice
> = (set, _, store) => ({
    resetState(initState) {
        set(() => ({
            ...store.getInitialState(),
            ...initState,
        }));
    },

    disableMovement(): void {
        set((state) => {
            state.legalMoves = new LegalMoves();
            state.highlightedLegalMoves = [];
            state.selectedPieceId = null;
        });
    },
});
