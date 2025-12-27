import { useEffect } from "react";

import { Move } from "@/features/chessboard/lib/types";
import { useGameEmitter } from "../hooks/useGameHub";
import { StoreApi } from "zustand";
import { LiveChessStore } from "../stores/liveChessStore";
import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";

export default function useLiveMoveEmitter(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const { pieceMovementEvent } = chessboardStore.getState();
    const { markPendingMoveAck, gameToken } = liveChessStore.getState();

    const sendGameEvent = useGameEmitter(gameToken);

    useEffect(() => {
        async function emitMove(move: Move) {
            markPendingMoveAck();
            await sendGameEvent("MovePieceAsync", gameToken, move.moveKey);
        }

        pieceMovementEvent.subscribe(emitMove);
        return () => {
            pieceMovementEvent.unsubscribe(emitMove);
        };
    }, [pieceMovementEvent]);
}
