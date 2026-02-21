import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { LiveChessStore } from "../stores/liveChessStore";
import { StoreApi } from "zustand";
import { useCallback, useEffect } from "react";
import { Move, MoveKey } from "@/features/chessboard/lib/types";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { addSidelineAnalysisMove } from "@/features/analysis/lib/handleAnalysisMove";

export default function useMoveEmitterForLiveGames(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
    sendMoveEvent: (moveKey: MoveKey) => Promise<void>,
) {
    const { pieceMovementEvent } = chessboardStore.getState();
    const { markPendingMoveAck } = liveChessStore.getState();
    const callback = useCallback(
        (moveKey: MoveKey) => sendMoveEvent(moveKey),
        [sendMoveEvent],
    );

    useEffect(() => {
        async function emitMove(move: Move, prevPieces: BoardPieces) {
            const { resultData, initialFen } = liveChessStore.getState();

            if (resultData === null) {
                markPendingMoveAck();
                await callback(move.moveKey);
            } else {
                await addSidelineAnalysisMove({
                    chessboardStore,
                    rootFen: initialFen,
                    move,
                    prevPieces,
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
