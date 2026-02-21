import {
    createChessboardProps,
    getViewer,
    ProcessedGameState,
} from "@/features/liveGame/lib/gameStateProcessor";

import { LiveChessStoreProps } from "@/features/liveGame/stores/liveChessStore";
import { BotGameState } from "@/lib/apiClient";

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
        initialFen: gameState.initialFen,

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
