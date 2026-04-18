import { StoreApi } from "zustand";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import useMoveEmitterForLiveGames from "./useMoveEmitterForLiveGames";
import { LiveChessStore } from "../stores/liveChessStore";
import { useGameEmitter } from "../hooks/useGameHub";

export default function useLiveMoveEmitter(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const { gameToken } = liveChessStore.getState();
    const sendGameEvent = useGameEmitter(gameToken);
    useMoveEmitterForLiveGames(liveChessStore, chessboardStore, ({ move }) =>
        sendGameEvent("MovePieceAsync", gameToken, move.moveKey),
    );
}
