import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import {
    createChessboardProps,
    getViewer,
    ProcessedGameState,
} from "@/features/liveGame/lib/gameStateProcessor";

import {
    LiveChessStore,
    LiveChessStoreProps,
} from "@/features/liveGame/stores/liveChessStore";
import { BotGameState, getBotGame } from "@/lib/apiClient";
import { StoreApi } from "zustand";

export default function processBotGameState(
    gameToken: string,
    viewerUserId: string,
    gameState: BotGameState,
): ProcessedGameState {
    const viewer = getViewer(
        gameState.whitePlayer,
        gameState.blackPlayer,
        viewerUserId,
    );

    const live: LiveChessStoreProps = {
        gameToken,

        whitePlayer: gameState.whitePlayer,
        blackPlayer: gameState.blackPlayer,
        sideToMove: gameState.sideToMove,

        sourceRevision: 0,
        pool: null,
        viewer,

        drawState: null,
        liveClocks: null,
        clockSnapshotByPly: new Map(),
        resultData: gameState.resultData ?? null,
    };
    const board = createChessboardProps(
        viewer,
        gameState.initialFen,
        gameState.moveHistory,
        gameState.legalMoves,
        gameState.resultData,
    );
    return { live, board };
}

export async function refetchBotGame(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const {
        gameToken,
        viewer: { userId },
    } = liveChessStore.getState();

    const { error, data: gameState } = await getBotGame({
        path: { gameToken },
    });
    if (error || gameState === undefined) {
        console.error("refetchBotGame getGame", error);
        return;
    }

    const { live, board } = processBotGameState(gameToken, userId, gameState);
    liveChessStore.getState().resetState(live);
    chessboardStore.getState().resetState(board);
}
