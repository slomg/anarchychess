import { StoreApi } from "zustand";

import {
    ChessboardProps,
    ChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import {
    GameColor,
    GamePlayer,
    getGame,
    MoveSnapshot,
    type GameState,
} from "@/lib/apiClient";

import { LiveChessStore, LiveChessStoreProps } from "../stores/liveChessStore";
import { decodeMovePath, decodeMovePathIntoLegalMoves } from "./moveDecoder";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import { simulateMove } from "@/features/chessboard/lib/simulateMove";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { decodeFen } from "../../chessboard/lib/fenDecoder";
import { LiveChessViewer } from "../stores/gamePlaySlice";
import constants from "@/lib/constants";
import { ClockSnapshot } from "./types";

export interface ProcessedGameState {
    live: LiveChessStoreProps;
    board: ChessboardProps;
}

export function processGameState(
    gameToken: string,
    viewerUserId: string,
    gameState: GameState,
): ProcessedGameState {
    const positionHistory = getPositionHistory(
        gameState.initialFen,
        gameState.moveHistory,
    );
    const lastPosition = positionHistory.viewingPosition;
    const boardWidth = constants.BOARD_WIDTH;
    const boardHeight = constants.BOARD_HEIGHT;
    const legalMoves = decodeMovePathIntoLegalMoves({
        paths: gameState.legalMoves,
        boardWidth,
    });

    const viewer = getViewer(
        gameState.whitePlayer,
        gameState.blackPlayer,
        viewerUserId,
    );

    const clockSnapshotByPly = getClockSnapshots(gameState);

    const live: LiveChessStoreProps = {
        gameToken,
        initialFen: gameState.initialFen,

        whitePlayer: gameState.whitePlayer,
        blackPlayer: gameState.blackPlayer,
        sideToMove: gameState.sideToMove,

        sourceRevision: gameState.revision,
        pool: gameState.pool,
        viewer,

        drawState: gameState.drawState,
        liveClocks: gameState.clocks,
        clockSnapshotByPly,
        resultData: gameState.resultData ?? null,
    };
    const board: ChessboardProps = {
        pieces: new BoardPieces(
            lastPosition?.pieces ?? positionHistory.rootPieces,
        ),
        positionHistory,

        legalMovesByPosition: new Map([
            [positionHistory.viewingPosition?.positionId, legalMoves],
        ]),
        lastMove: lastPosition?.move && {
            from: lastPosition.move.from,
            to: lastPosition.move.to,
        },

        boardDimensions: { width: boardWidth, height: boardHeight },
        viewingFrom: viewer.playerColor ?? GameColor.WHITE,
        allowHistoryChanges:
            gameState.resultData !== null && gameState.resultData !== undefined,
    };

    return { live, board };
}

export function getViewer(
    whitePlayer: GamePlayer,
    blackPlayer: GamePlayer,
    userId: string,
): LiveChessViewer {
    const viewer: LiveChessViewer = {
        userId: userId,
        playerColor: getViewerColor(whitePlayer, blackPlayer, userId),
    };
    return viewer;
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
export function getPositionHistory(
    initialFen: string,
    moveHistory: MoveSnapshot[],
): PositionHistory {
    let pieces = decodeFen(initialFen);

    const positionHistory = new PositionHistory(pieces);
    for (const moveSnapshot of moveHistory) {
        const move = decodeMovePath(moveSnapshot.path, constants.BOARD_WIDTH);
        const { newPieces } = simulateMove(pieces, move);

        positionHistory.addNextPosition({
            pieces: newPieces,
            move,
            sideToMove: moveSnapshot.nextSideToMove,
            fen: moveSnapshot.fen,
            san: moveSnapshot.san,
        });
        pieces = newPieces;
    }
    return positionHistory;
}

function getClockSnapshots(gameState: GameState): Map<number, ClockSnapshot> {
    const baseClock = gameState.pool.timeControl.baseSeconds * 1000;
    let clockSnapshot: ClockSnapshot = {
        whiteClock: baseClock,
        blackClock: baseClock,
    };

    const clocksByPly: Map<number, ClockSnapshot> = new Map([
        [0, clockSnapshot],
    ]);
    for (const [i, moveSnapshot] of gameState.moveHistory.entries()) {
        clockSnapshot = {
            whiteClock:
                i % 2 === 0 ? moveSnapshot.timeLeft : clockSnapshot.whiteClock,
            blackClock:
                i % 2 !== 0 ? moveSnapshot.timeLeft : clockSnapshot.blackClock,
        };
        clocksByPly.set(i + 1, { ...clockSnapshot });
    }
    clocksByPly.set(gameState.moveHistory.length, {
        whiteClock: gameState.clocks.whiteClock.timeLeftMs,
        blackClock: gameState.clocks.blackClock.timeLeftMs,
    });

    return clocksByPly;
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
        console.error("refetchGame getGame", error);
        return;
    }

    const { live, board } = processGameState(gameToken, userId, gameState);
    liveChessStore.getState().resetState(live);
    chessboardStore.getState().resetState(board);
}
