import { createFakeClockPlayer } from "@/lib/testUtils/fakers/createFakeClockPlayer";
import { createFakeMoveSnapshot } from "@/lib/testUtils/fakers/moveSnapshotFaker";
import { decodeMovePath, decodeMovePathIntoLegalMoves } from "../moveDecoder";
import createDefaultChessboard from "@/features/chessboard/lib/defaultBoard";
import { createFakeGameState } from "@/lib/testUtils/fakers/gameStateFaker";
import { createFakeMove } from "@/lib/testUtils/fakers/chessboardFakers";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import { createFakeClocks } from "@/lib/testUtils/fakers/clocksFaker";
import { simulateMove } from "@/features/chessboard/lib/simulateMove";
import { LiveChessStoreProps } from "../../stores/liveChessStore";
import mockSequentialUUID from "@/lib/testUtils/mocks/mockUuids";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { LiveChessViewer } from "../../stores/gamePlaySlice";
import { MoveBounds } from "@/features/chessboard/lib/types";
import { logicalPoint } from "@/features/point/pointUtils";
import { processGameState } from "../gameStateProcessor";
import { GameColor, GameResult } from "@/lib/apiClient";

describe("processGameState", () => {
    it("should create correct live store props", () => {
        const gameState = createFakeGameState();
        const viewerUserId = gameState.blackPlayer.userId;

        const { live } = processGameState(
            "game-token",
            viewerUserId,
            gameState,
        );

        expect(live).toEqual<LiveChessStoreProps>({
            gameToken: "game-token",
            sourceRevision: gameState.revision,

            whitePlayer: gameState.whitePlayer,
            blackPlayer: gameState.blackPlayer,
            sideToMove: gameState.sideToMove,

            pool: gameState.pool,
            viewer: {
                userId: viewerUserId,
                playerColor: GameColor.BLACK,
            },

            drawState: gameState.drawState,
            liveClocks: gameState.clocks,
            clockSnapshotByPly: expect.anything(),
            resultData: null,
        });
    });

    it("should create board props with correct orientation", () => {
        const gameState = createFakeGameState();
        const viewerUserId = gameState.blackPlayer.userId;

        const { board } = processGameState(
            "game-token",
            viewerUserId,
            gameState,
        );

        expect(board.viewingFrom).toBe(GameColor.BLACK);
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

        const { board } = processGameState(
            "game-token",
            gameState.blackPlayer.userId,
            gameState,
        );

        // moves and clocks from the test setup
        // start position history ids after piece ids
        const defaultChessboard = createDefaultChessboard();
        mockSequentialUUID({ startAt: defaultChessboard.size });
        const baseMs = gameState.pool.timeControl.baseSeconds * 1000;
        let pieces = new BoardPieces(defaultChessboard);
        const positionHistory = new PositionHistory({
            pieces: new BoardPieces(pieces),
            fen: gameState.initialFen,
        });
        const moves = [
            {
                from: logicalPoint({ x: 5, y: 1 }),
                to: logicalPoint({ x: 5, y: 4 }),
                decoded: decodeMovePath(gameState.moveHistory[0].path),
                clocks: { whiteClock: 100, blackClock: baseMs },
                fen: "fake-fen-1",
                nextSideToMove: GameColor.BLACK,
                san: "f5",
            },
            {
                from: logicalPoint({ x: 5, y: 8 }),
                to: logicalPoint({ x: 5, y: 5 }),
                decoded: decodeMovePath(gameState.moveHistory[1].path),
                clocks: { whiteClock: 100, blackClock: 100 },
                fen: "fake-fen-2",
                nextSideToMove: GameColor.WHITE,
                san: "f6",
            },
            {
                from: logicalPoint({ x: 8, y: 0 }),
                to: logicalPoint({ x: 7, y: 2 }),
                decoded: decodeMovePath(gameState.moveHistory[2].path),
                clocks: { whiteClock: 50, blackClock: 100 },
                fen: "fake-fen-3",
                nextSideToMove: GameColor.BLACK,
                san: "Hh3",
            },
            {
                from: logicalPoint({ x: 1, y: 9 }),
                to: logicalPoint({ x: 2, y: 7 }),
                decoded: decodeMovePath(gameState.moveHistory[3].path),
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
                san: move.san,
            });
        }

        const lastPosition = positionHistory.currentNode!;
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
        const { board } = processGameState(
            "game-token",
            gameState.blackPlayer.userId,
            gameState,
        );

        const positionId = board.positionHistory!.currentPosition.positionId;
        expect(board.legalMovesByPosition.get(positionId)).toEqual(
            decodeMovePathIntoLegalMoves(gameState.legalMoves),
        );
    });

    it("should return the right viewer for spectator", () => {
        const gameState = createFakeGameState();
        const userId = "random user id";

        const result = processGameState("game-token", userId, gameState);

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

        const result = processGameState(
            "game-token",
            gameState.blackPlayer.userId,
            gameState,
        );

        expect(result.board.allowHistoryChanges).toBe(true);
    });

    it("should initialize clock snapshots correctly for each ply", () => {
        const gameState = createFakeGameState({
            moveHistory: [
                createFakeMoveSnapshot({ timeLeft: 295_000 }),
                createFakeMoveSnapshot({ timeLeft: 290_000 }),
                createFakeMoveSnapshot({ timeLeft: 285_000 }),
                createFakeMoveSnapshot({ timeLeft: 270_000 }),
            ],
            clocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({ timeLeftMs: 280_000 }),
                blackClock: createFakeClockPlayer({ timeLeftMs: 270_000 }),
            }),
        });
        gameState.pool.timeControl.baseSeconds = 300; // 5 min base

        const { live } = processGameState(
            "game-token",
            gameState.whitePlayer.userId,
            gameState,
        );

        const snapshots = live.clockSnapshotByPly;

        expect(snapshots.size).toBe(5);

        expect(snapshots.get(0)).toEqual({
            whiteClock: 300_000,
            blackClock: 300_000,
        });
        expect(snapshots.get(1)).toEqual({
            whiteClock: 295_000,
            blackClock: 300_000,
        });
        expect(snapshots.get(2)).toEqual({
            whiteClock: 295_000,
            blackClock: 290_000,
        });
        expect(snapshots.get(3)).toEqual({
            whiteClock: 285_000,
            blackClock: 290_000,
        });
        // final clocks
        expect(snapshots.get(4)).toEqual({
            whiteClock: 280_000,
            blackClock: 270_000,
        });
    });
});
