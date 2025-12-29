import { StoreApi } from "zustand";

import {
    ChessboardProps,
    ChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import {
    GameColor,
    GamePlayer,
    getGame,
    type GameState,
} from "@/lib/apiClient";

import { LiveChessStore, LiveChessStoreProps } from "../stores/liveChessStore";
import { decodeMovePath, decodeMovePathIntoLegalMoves } from "./moveDecoder";
import { Position, PositionHistory } from "@/features/chessboard/lib/types";
import { simulateMove } from "@/features/chessboard/lib/simulateMove";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { decodeFen } from "../../chessboard/lib/fenDecoder";
import { LiveChessViewer } from "../stores/gamePlaySlice";
import { ClockSnapshot } from "./types";
import constants from "@/lib/constants";

export interface ProcessedGameState {
    live: LiveChessStoreProps;
    board: ChessboardProps;
}

export function createStoreProps(
    gameToken: string,
    viewerUserId: string,
    gameState: GameState,
): ProcessedGameState {
    const positionHistory = getPositionHistory(gameState);
    const boardWidth = constants.BOARD_WIDTH;
    const boardHeight = constants.BOARD_HEIGHT;
    const legalMoves = decodeMovePathIntoLegalMoves({
        paths: gameState.moveOptions.legalMoves,
        boardWidth,
        hasForcedMoves: gameState.moveOptions.hasForcedMoves,
    });

    const lastPosition = positionHistory.at(-1);

    const viewerColor = getViewerColor(
        gameState.whitePlayer,
        gameState.blackPlayer,
        viewerUserId,
    );
    const viewer: LiveChessViewer = {
        userId: viewerUserId,
        playerColor: viewerColor,
    };

    const live: LiveChessStoreProps = {
        gameToken,

        whitePlayer: gameState.whitePlayer,
        blackPlayer: gameState.blackPlayer,
        sideToMove: gameState.sideToMove,

        sourceRevision: gameState.revision,
        pool: gameState.pool,
        viewer,

        drawState: gameState.drawState,
        clocks: gameState.clocks,
        resultData: gameState.resultData ?? null,
    };
    const board: ChessboardProps = {
        pieces: lastPosition?.pieces ?? new BoardPieces(),
        positionHistory,

        legalMovesByPly: new Map([[positionHistory.length - 1, legalMoves]]),
        lastMove: lastPosition?.move && {
            from: lastPosition.move.from,
            to: lastPosition.move.to,
        },

        boardDimensions: { width: boardWidth, height: boardHeight },
        viewingFrom: viewerColor ?? GameColor.WHITE,
        canDrag: true,
        muteAudio: false,
    };

    return { live, board };
}

function getViewerColor(
    whitePlayer: GamePlayer,
    blackPlayer: GamePlayer,
    userId: string,
): GameColor | null {
    if (userId === whitePlayer.userId) return GameColor.WHITE;
    else if (userId === blackPlayer.userId) return GameColor.BLACK;
    return null;
}

function getPositionHistory(gameState: GameState): PositionHistory {
    let pieces = decodeFen(gameState.initialFen);

    const baseClock = gameState.pool.timeControl.baseSeconds * 1000;
    let clockSnapshot: ClockSnapshot = {
        whiteClock: baseClock,
        blackClock: baseClock,
    };
    const positionHistory: PositionHistory = [
        {
            pieces,
            // clocks: { ...clockSnapshot },
        },
    ];
    for (const [i, moveSnapshot] of gameState.moveHistory.entries()) {
        clockSnapshot = {
            whiteClock:
                i % 2 === 0 ? moveSnapshot.timeLeft : clockSnapshot.whiteClock,
            blackClock:
                i % 2 !== 0 ? moveSnapshot.timeLeft : clockSnapshot.blackClock,
        };

        const move = decodeMovePath(moveSnapshot.path, constants.BOARD_WIDTH);
        const { newPieces } = simulateMove(pieces, move);

        const position: Position = {
            move,
            san: moveSnapshot.san,
            pieces: newPieces,
            // clocks: { ...clockSnapshot },
        };
        positionHistory.push(position);
        pieces = newPieces;
    }
    return positionHistory;
}

export async function refetchGame(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const {
        gameToken,
        viewer: { userId },
    } = liveChessStore.getState();

    const { error, data: gameState } = await getGame({
        path: { gameToken },
    });
    if (error || gameState === undefined) {
        console.error(error);
        return;
    }

    const { live, board } = createStoreProps(gameToken, userId, gameState);
    liveChessStore.getState().resetState(live);
    chessboardStore.getState().resetState(board);
}
