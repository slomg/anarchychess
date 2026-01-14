import type { ChessboardProps, ChessboardStore } from "./chessboardStore";
import { StateCreator } from "zustand";

export interface CoreSlice {
    resetState(initState: ChessboardProps): Promise<void>;
}

export const createCoreSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    CoreSlice
> = (set, get, store) => ({
    async resetState(initState) {
        const { pieces: oldPieces, updatePiecesFromPosition } = get();
        const latestPosition = initState.positionHistory?.viewingPosition;

        set(() => ({
            ...store.getInitialState(),
            ...initState,
            pieces: latestPosition ? oldPieces : initState.pieces,
        }));
        if (latestPosition) {
            await updatePiecesFromPosition(latestPosition);
        }
    },
});
