import type { ChessboardProps, ChessboardStore } from "./chessboardStore";
import { StateCreator } from "zustand";

export interface CoreSlice {
    resetState(initState: ChessboardProps): void;
}

export const createCoreSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    CoreSlice
> = (set, get, store) => ({
    resetState(initState) {
        const { pieces: oldPieces, updatePiecesFromPosition } = get();
        const latestPosition = initState.positionHistory?.viewingPosition;

        set(() => ({
            ...store.getInitialState(),
            ...initState,
            oldPieces,
        }));
        if (latestPosition) {
            updatePiecesFromPosition(latestPosition);
        }
    },
});
