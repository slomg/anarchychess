import { useCallback, useEffect } from "react";
import { StoreApi } from "zustand";

import { addSidelineAnalysisMove } from "@/features/analysis/lib/handleAnalysisMove";
import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { PieceMovementEvent } from "@/features/chessboard/stores/piecesSlice";
import { LiveChessStore } from "../stores/liveChessStore";

export default function useMoveEmitterForLiveGames(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
    sendMoveEvent: (event: PieceMovementEvent) => Promise<void>,
) {
    const { pieceMovementEvent } = chessboardStore.getState();
    const { markPendingMoveAck } = liveChessStore.getState();
    const callback = useCallback(
        (event: PieceMovementEvent) => sendMoveEvent(event),
        [sendMoveEvent],
    );

    useEffect(() => {
        async function emitMove(event: PieceMovementEvent) {
            const { resultData } = liveChessStore.getState();

            if (resultData === null) {
                markPendingMoveAck();
                await callback(event);
            } else {
                await addSidelineAnalysisMove({
                    chessboardStore,
                    move: event.move,
                    prevPieces: event.prevPieces,
                });
            }
        }

        pieceMovementEvent.subscribe(emitMove);
        return () => {
            pieceMovementEvent.unsubscribe(emitMove);
        };
    }, [
        pieceMovementEvent,
        markPendingMoveAck,
        callback,
        liveChessStore,
        chessboardStore,
    ]);
}
