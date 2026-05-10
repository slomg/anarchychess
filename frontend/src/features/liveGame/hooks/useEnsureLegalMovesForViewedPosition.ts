import { StoreApi } from "zustand";
import { useEffect } from "react";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { decodeMovePathIntoLegalMoves } from "../lib/moveDecoder";
import { getNextLegalMoves } from "@/lib/apiClient";

export default function useEnsureLegalMovesForViewedPosition(
    chessboardStore: StoreApi<ChessboardStore>,
) {
    useEffect(() => {
        const unsub = chessboardStore.subscribe(async (state, prev) => {
            const currentPosition = state.positionHistory.currentPosition;
            if (
                currentPosition.positionId ===
                    prev.positionHistory.currentPosition.positionId &&
                state.allowHistoryChanges === prev.allowHistoryChanges
            ) {
                return;
            }

            if (
                !state.allowHistoryChanges ||
                state.hasLegalMovesForPosition(currentPosition.positionId)
            ) {
                return;
            }

            const { error, data: movePaths } = await getNextLegalMoves({
                query: {
                    fen: currentPosition.fen,
                },
            });
            if (error || movePaths === undefined) {
                console.error(
                    "useEnsureLegalMovesForViewedPosition getNextLegalMoves",
                    error,
                );
                return;
            }

            const legalMoves = decodeMovePathIntoLegalMoves(movePaths);
            state.addLegalMovesForPosition(
                legalMoves,
                currentPosition.positionId,
            );
        });
        return unsub;
    }, [chessboardStore]);
}
