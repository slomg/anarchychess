import { StoreApi } from "zustand";
import { useEffect } from "react";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { decodeMovePathIntoLegalMoves } from "../lib/moveDecoder";
import { getNextLegalMoves } from "@/lib/apiClient";

export default function useEnsureLegalMovesForViewedPosition(
    initialFen: string,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    useEffect(() => {
        const unsub = chessboardStore.subscribe(async (state, prev) => {
            const viewingPosition = state.positionHistory.viewingPosition;
            if (
                viewingPosition?.positionId ===
                    prev.positionHistory.viewingPosition?.positionId &&
                state.allowHistoryChanges === prev.allowHistoryChanges
            ) {
                return;
            }

            if (
                !state.allowHistoryChanges ||
                state.hasLegalMovesForPosition(viewingPosition?.positionId)
            ) {
                return;
            }

            const { error, data } = await getNextLegalMoves({
                query: { fen: viewingPosition?.fen ?? initialFen },
            });
            if (error || data === undefined) {
                console.error(
                    "useEnsureLegalMovesForViewedPosition getNextLegalMoves",
                    error,
                );
                return;
            }

            const legalMoves = decodeMovePathIntoLegalMoves({
                paths: data.legalMoves,
                hasForcedMoves: data.hasForcedMoves,
                boardWidth: state.boardDimensions.width,
            });
            state.addLegalMoves(legalMoves, viewingPosition?.positionId);
        });
        return unsub;
    }, [initialFen, chessboardStore]);
}
