import { StoreApi } from "zustand";
import { useEffect } from "react";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { PieceMovementEvent } from "@/features/chessboard/stores/piecesSlice";
import { addAnalysisMove } from "../lib/handleAnalysisMove";

export default function useAnalysisMoveResolver(
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const { pieceMovementEvent } = chessboardStore.getState();

    useEffect(() => {
        async function emitMove({ move, prevPieces }: PieceMovementEvent) {
            await addAnalysisMove({
                chessboardStore,
                move,
                prevPieces,
            });
        }

        pieceMovementEvent.subscribe(emitMove);
        return () => {
            pieceMovementEvent.unsubscribe(emitMove);
        };
    }, [pieceMovementEvent, chessboardStore]);
}
