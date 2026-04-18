import { StoreApi } from "zustand";

import useMoveEmitterForLiveGames from "@/features/liveGame/hooks/useMoveEmitterForLiveGames";
import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { LiveChessStore } from "@/features/liveGame/stores/liveChessStore";
import { useBotEmitter } from "./useBotHub";

export default function useBotMoveEmitter(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const { gameToken } = liveChessStore.getState();
    const sendBotEvent = useBotEmitter(gameToken);
    useMoveEmitterForLiveGames(
        liveChessStore,
        chessboardStore,
        async ({ move, animationPromise }) => {
            await animationPromise;
            sendBotEvent("MakeMoveAsync", gameToken, move.moveKey);
        },
    );
}
