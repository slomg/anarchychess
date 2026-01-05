import { StoreApi } from "zustand";
import { useEffect } from "react";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import handleAnalysisMove from "../lib/handleAnalysisMove";
import { RootAnalysisPosition } from "@/lib/apiClient";
import { Move } from "@/features/chessboard/lib/types";

export default function useAnalysisMoveResolver(
    rootPosition: RootAnalysisPosition,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const { pieceMovementEvent } = chessboardStore.getState();

    useEffect(() => {
        async function emitMove(move: Move) {
            await handleAnalysisMove(chessboardStore, rootPosition.fen, move);
        }

        pieceMovementEvent.subscribe(emitMove);
        return () => {
            pieceMovementEvent.unsubscribe(emitMove);
        };
    }, [pieceMovementEvent, rootPosition.fen, chessboardStore]);
}
