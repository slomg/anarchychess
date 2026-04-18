import { StoreApi } from "zustand";
import { useEffect } from "react";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { PieceMovementEvent } from "@/features/chessboard/stores/piecesSlice";
import { addAnalysisMove } from "../lib/handleAnalysisMove";
import { RootAnalysisPosition } from "@/lib/apiClient";

export default function useAnalysisMoveResolver(
    rootPosition: RootAnalysisPosition,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const { pieceMovementEvent } = chessboardStore.getState();

    useEffect(() => {
        async function emitMove({ move, prevPieces }: PieceMovementEvent) {
            await addAnalysisMove({
                chessboardStore,
                rootFen: rootPosition.fen,
                move,
                prevPieces,
            });
        }

        pieceMovementEvent.subscribe(emitMove);
        return () => {
            pieceMovementEvent.unsubscribe(emitMove);
        };
    }, [pieceMovementEvent, rootPosition.fen, chessboardStore]);
}
