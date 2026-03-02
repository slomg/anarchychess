import { StoreApi, useStore } from "zustand";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { LiveChessStore } from "@/features/liveGame/stores/liveChessStore";
import handleMoveUpdate from "@/features/liveGame/lib/handleMoveUpdate";
import { decodeLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { refetchBotGame } from "../lib/botStateProcessor";
import { useBotEvent } from "./useBotHub";

export default function useLiveBotEvents(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const boardDimensions = useStore(chessboardStore, (x) => x.boardDimensions);
    const gameToken = useStore(liveChessStore, (x) => x.gameToken);

    useBotEvent(gameToken, "SyncPlyNumberAsync", async (plyNumber) => {
        const { positionHistory } = chessboardStore.getState();
        if (plyNumber !== positionHistory.mainPlyCount) {
            refetchBotGame(liveChessStore, chessboardStore);
        }
    });

    useBotEvent(
        gameToken,
        "PlayerMadeMoveAsync",
        async (move, plyNumber, didMoveEndGame) => {
            const legalMoves = didMoveEndGame
                ? LegalMoves.StableEmpty
                : undefined;

            const success = await handleMoveUpdate(
                liveChessStore,
                chessboardStore,
                {
                    move,
                    plyNumber,
                    legalMoves,
                },
            );
            if (!success) {
                await refetchBotGame(liveChessStore, chessboardStore);
            }
        },
    );

    useBotEvent(
        gameToken,
        "BotMadeMoveAsync",
        async (move, plyNumber, compressedLegalMoves) => {
            const legalMoves = decodeLegalMoves({
                encoded: compressedLegalMoves,
                boardWidth: boardDimensions.width,
            });

            const success = await handleMoveUpdate(
                liveChessStore,
                chessboardStore,
                {
                    move,
                    plyNumber,
                    legalMoves,
                },
            );
            if (!success) {
                await refetchBotGame(liveChessStore, chessboardStore);
            }
        },
    );

    useBotEvent(gameToken, "GameEndedAsync", async (result) => {
        const { setAllowHistoryChanges, positionHistory } =
            chessboardStore.getState();

        liveChessStore.getState().endGame(positionHistory.mainPlyCount, result);
        setAllowHistoryChanges(true);
        AudioPlayer.playAudio(AudioType.GAME_END);
    });
}
