import { StoreApi } from "zustand";
import { useEffect } from "react";

import { addSidelineAnalysisMove } from "@/features/analysis/lib/handleAnalysisMove";
import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { LiveChessStore } from "../stores/liveChessStore";
import { Move } from "@/features/chessboard/lib/types";
import { useGameEmitter } from "../hooks/useGameHub";

export default function useLiveMoveEmitter(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const { pieceMovementEvent } = chessboardStore.getState();
    const { markPendingMoveAck, gameToken } = liveChessStore.getState();

    const sendGameEvent = useGameEmitter(gameToken);

    useEffect(() => {
        async function emitMove(move: Move) {
            const { resultData, initialFen } = liveChessStore.getState();

            if (resultData === null) {
                markPendingMoveAck();
                await sendGameEvent("MovePieceAsync", gameToken, move.moveKey);
            } else {
                await addSidelineAnalysisMove(
                    chessboardStore,
                    initialFen,
                    move,
                );
            }
        }

        pieceMovementEvent.subscribe(emitMove);
        return () => {
            pieceMovementEvent.unsubscribe(emitMove);
        };
    }, [
        pieceMovementEvent,
        gameToken,
        markPendingMoveAck,
        sendGameEvent,
        liveChessStore,
        chessboardStore,
    ]);
}
