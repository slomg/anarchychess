import { decodeMovePath, decodeMovePathIntoLegalMoves } from "../moveDecoder";
import { createFakeGameState } from "@/lib/testUtils/fakers/gameStateFaker";
import { createFakeMovePath } from "@/lib/testUtils/fakers/movePathFaker";
import { createFakeMove } from "@/lib/testUtils/fakers/chessboardFakers";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import { simulateMove } from "@/features/chessboard/lib/simulateMove";
import { LiveChessStoreProps } from "../../stores/liveChessStore";
import mockSequentialUUID from "@/lib/testUtils/mocks/mockUuids";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { LiveChessViewer } from "../../stores/gamePlaySlice";
import { MoveBounds } from "@/features/chessboard/lib/types";
import { logicalPoint } from "@/features/point/pointUtils";
import { createStoreProps } from "../gameStateProcessor";
import { GameColor, GameResult } from "@/lib/apiClient";
import constants from "@/lib/constants";

describe("createStoreProps", () => {
    it("should create correct live store props", () => {
        const gameState = createFakeGameState();
        const viewerUserId = gameState.blackPlayer.userId;

        const { live } = createStoreProps(
            "game-token",
            viewerUserId,
            gameState,
        );

        expect(live).toEqual<LiveChessStoreProps>({
            gameToken: "game-token",
            sourceRevision: gameState.revision,
            initialFen: gameState.initialFen,

            whitePlayer: gameState.whitePlayer,
            blackPlayer: gameState.blackPlayer,
            sideToMove: gameState.sideToMove,

            pool: gameState.pool,
            viewer: {
                userId: viewerUserId,
                playerColor: GameColor.BLACK,
            },

            drawState: gameState.drawState,
            clocks: gameState.clocks,
            overtimeTurnStartedAt: gameState.overtime.overtimeTurnStartedAt,
            whiteOvertime: null,
            blackOvertime: null,
            resultData: null,
        });
    });

    it("should create board props with correct orientation and dimensions", () => {
        const gameState = createFakeGameState();
        const viewerUserId = gameState.blackPlayer.userId;

        const { board } = createStoreProps(
            "game-token",
            viewerUserId,
            gameState,
        );

        expect(board.viewingFrom).toBe(GameColor.BLACK);
        expect(board.boardDimensions).toEqual({
            width: constants.BOARD_WIDTH,
            height: constants.BOARD_HEIGHT,
        });
    });

    it("should build position history and last move from move history", () => {
        mockSequentialUUID();

        const gameState = createFakeGameState({
            // f5 f6 Nh3 Nc8
            moveHistory: [
                {
                    path: {
                        fromIdx: 15,
                        toIdx: 45,
                        moveKey: "0",
                    },
                    nextSideToMove: GameColor.BLACK,
                    fen: "fake-fen-1",
                    san: "f5",
                    timeLeft: 100,
                },
                {
                    path: {
                        fromIdx: 85,
                        toIdx: 55,
                        moveKey: "1",
                    },
                    nextSideToMove: GameColor.WHITE,
                    fen: "fake-fen-2",
                    san: "f6",
                    timeLeft: 100,
                },
                {
                    path: {
                        fromIdx: 8,
                        toIdx: 27,
                        moveKey: "2",
                    },
                    nextSideToMove: GameColor.BLACK,
                    fen: "fake-fen-3",
                    san: "Hh3",
                    timeLeft: 50,
                },
                {
                    path: {
                        fromIdx: 91,
                        toIdx: 72,
                        moveKey: "3",
                    },
                    nextSideToMove: GameColor.WHITE,
                    fen: "fake-fen-4",
                    san: "Hc8",
                    timeLeft: 50,
                },
            ],
        });

        const { board } = createStoreProps(
            "game-token",
            gameState.blackPlayer.userId,
            gameState,
        );

        // moves and clocks from the test setup
        // start position history ids after piece ids
        mockSequentialUUID({ startAt: constants.DEFAULT_CHESS_BOARD.size });
        const baseMs = gameState.pool.timeControl.baseSeconds * 1000;
        let pieces = new BoardPieces(constants.DEFAULT_CHESS_BOARD);
        const positionHistory = new PositionHistory(new BoardPieces(pieces));
        const moves = [
            {
                from: logicalPoint({ x: 5, y: 1 }),
                to: logicalPoint({ x: 5, y: 4 }),
                decoded: decodeMovePath(gameState.moveHistory[0].path, 10),
                clocks: { whiteClock: 100, blackClock: baseMs },
                fen: "fake-fen-1",
                nextSideToMove: GameColor.BLACK,
                san: "f5",
            },
            {
                from: logicalPoint({ x: 5, y: 8 }),
                to: logicalPoint({ x: 5, y: 5 }),
                decoded: decodeMovePath(gameState.moveHistory[1].path, 10),
                clocks: { whiteClock: 100, blackClock: 100 },
                fen: "fake-fen-2",
                nextSideToMove: GameColor.WHITE,
                san: "f6",
            },
            {
                from: logicalPoint({ x: 8, y: 0 }),
                to: logicalPoint({ x: 7, y: 2 }),
                decoded: decodeMovePath(gameState.moveHistory[2].path, 10),
                clocks: { whiteClock: 50, blackClock: 100 },
                fen: "fake-fen-3",
                nextSideToMove: GameColor.BLACK,
                san: "Hh3",
            },
            {
                from: logicalPoint({ x: 1, y: 9 }),
                to: logicalPoint({ x: 2, y: 7 }),
                decoded: decodeMovePath(gameState.moveHistory[3].path, 10),
                clocks: { whiteClock: 50, blackClock: 50 },
                fen: "fake-fen-4",
                nextSideToMove: GameColor.WHITE,
                san: "Hc8",
            },
        ];

        for (const move of moves) {
            const { newPieces } = simulateMove(
                pieces,
                createFakeMove({ from: move.from, to: move.to }),
            );
            pieces = newPieces;
            positionHistory.addNextPosition({
                pieces,
                move: move.decoded,
                sideToMove: move.nextSideToMove,
                fen: move.fen,
                // clocks: move.clocks,
                san: move.san,
            });
        }

        const lastPosition = positionHistory.viewingPosition!;
        const lastMove: MoveBounds = {
            from: lastPosition.move.from,
            to: lastPosition.move.to,
        };

        expect(board.lastMove).toEqual(lastMove);
        expect(board.positionHistory).toEqual(positionHistory);
        expect(board.pieces).toEqual(lastPosition.pieces);
    });

    it("should map legal moves to the current position", () => {
        const gameState = createFakeGameState();
        const { board } = createStoreProps(
            "game-token",
            gameState.blackPlayer.userId,
            gameState,
        );

        const positionId = board.positionHistory!.viewingPosition?.positionId;
        expect(board.legalMovesByPosition.get(positionId)).toEqual(
            decodeMovePathIntoLegalMoves({
                paths: gameState.legalMoves,
                boardWidth: constants.BOARD_WIDTH,
            }),
        );
    });

    it("should decode white player overtime correctly", () => {
        const gameState = createFakeGameState({
            overtime: {
                whiteOvertime: {
                    secondRemainder: 0.123,
                    pendingRemoval: [
                        {
                            legalMoves: [createFakeMovePath()],
                            removedPiece: { x: 1, y: 2 },
                        },
                    ],
                },
                blackOvertime: null,
                overtimeTurnStartedAt: 1234,
            },
        });

        const { live } = createStoreProps(
            "game-token",
            gameState.blackPlayer.userId,
            gameState,
        );

        expect(live.whiteOvertime).not.toBeNull();
        expect(live.overtimeTurnStartedAt).toBe(1234);
        expect(live.whiteOvertime!.secondRemainder).toBe(0.123);
        expect(live.whiteOvertime!.pendingRemoval[0].removedPieceAt).toEqual(
            logicalPoint({ x: 1, y: 2 }),
        );
        expect(live.blackOvertime).toBeNull();
    });

    it("should decode black player overtime correctly", () => {
        const gameState = createFakeGameState({
            overtime: {
                whiteOvertime: null,
                blackOvertime: {
                    secondRemainder: 0.456,
                    pendingRemoval: [
                        {
                            legalMoves: [createFakeMovePath()],
                            removedPiece: { x: 2, y: 3 },
                        },
                    ],
                },
                overtimeTurnStartedAt: 5678,
            },
        });

        const { live } = createStoreProps(
            "game-token",
            gameState.blackPlayer.userId,
            gameState,
        );

        expect(live.whiteOvertime).toBeNull();
        expect(live.overtimeTurnStartedAt).toBe(5678);
        expect(live.blackOvertime).not.toBeNull();
        expect(live.blackOvertime!.secondRemainder).toBe(0.456);
        expect(live.blackOvertime!.pendingRemoval[0].removedPieceAt).toEqual(
            logicalPoint({ x: 2, y: 3 }),
        );
    });

    it("should return the right viewer for spectator", () => {
        const gameState = createFakeGameState();
        const userId = "random user id";

        const result = createStoreProps("game-token", userId, gameState);

        expect(result.live.viewer).toEqual<LiveChessViewer>({
            userId,
            playerColor: null,
        });
    });

    it("should enable history changes if the game is over", () => {
        const gameState = createFakeGameState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "desc",
            },
        });

        const result = createStoreProps(
            "game-token",
            gameState.blackPlayer.userId,
            gameState,
        );

        expect(result.board.allowHistoryChanges).toBe(true);
    });
});
