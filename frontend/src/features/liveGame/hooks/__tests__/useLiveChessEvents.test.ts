import { renderHook } from "@testing-library/react";
import { brotliCompressSync } from "zlib";
import { StoreApi } from "zustand";
import { act } from "react";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import createLiveChessStore, {
    LiveChessStore,
} from "../../stores/liveChessStore";
import {
    decodeMovePath,
    decodeMovePathIntoLegalMoves,
} from "../../lib/moveDecoder";
import {
    Clocks,
    DrawState,
    GameColor,
    MovePath,
    MoveSnapshot,
} from "@/lib/apiClient";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { createNFakePositionHistory } from "@/lib/testUtils/fakers/positionHistoryFaker";
import { createFakeGameResultData } from "@/lib/testUtils/fakers/gameResultDataFaker";
import { createFakeMoveSnapshot } from "@/lib/testUtils/fakers/moveSnapshotFaker";
import { createFakeLegalMoves } from "@/lib/testUtils/fakers/chessboardFakers";
import { createFakeMovePath } from "@/lib/testUtils/fakers/movePathFaker";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import { createFakeClocks } from "@/lib/testUtils/fakers/clocksFaker";
import { GameClientEvents, useGameEvent } from "../useGameHub";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import gameStartRedirect from "../../lib/gameStartRedirect";
import { refetchGame } from "../../lib/gameStateProcessor";
import { logicalPoint } from "@/features/point/pointUtils";
import handleMoveUpdate from "../../lib/handleMoveUpdate";
import useLiveChessEvents from "../useLiveChessEvents";

vi.mock("@/features/liveGame/hooks/useGameHub");
vi.mock("@/features/liveGame/lib/gameStateProcessor");
vi.mock("@/features/liveGame/lib/handleMoveUpdate");
vi.mock("@/features/liveGame/lib/gameStartRedirect");

describe("useLiveChessEvents", () => {
    let liveChessStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;

    const useGameEventMock = vi.mocked(useGameEvent);
    const handleMoveUpdateMock = vi.mocked(handleMoveUpdate);

    const gameEventHandlers: EventHandlers<GameClientEvents> = {};

    beforeEach(() => {
        liveChessStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();

        handleMoveUpdateMock.mockResolvedValue(true);
        useGameEventMock.mockImplementation((_, event, handler) => {
            gameEventHandlers[event] = handler;
        });
    });

    function renderLiveChessEvents() {
        return renderHook(() =>
            useLiveChessEvents(liveChessStore, chessboardStore),
        );
    }

    function encodeMoves(moves: MovePath[]): string {
        const json = JSON.stringify(moves);
        const compressed = brotliCompressSync(Buffer.from(json));
        return compressed.toString("base64");
    }

    async function triggerMoveMade({
        sideToMove,
        clocks,
        plyNumber,
        didMoveEndGame,
    }: {
        sideToMove: GameColor;
        clocks?: Clocks;
        plyNumber?: number;
        didMoveEndGame?: boolean;
    }): Promise<MoveSnapshot> {
        const move = createFakeMoveSnapshot({
            san: "test san",
            path: { fromIdx: 11, toIdx: 12, moveKey: "0" },
            nextSideToMove: sideToMove,
        });
        clocks ??= createFakeClocks();
        plyNumber ??=
            chessboardStore.getState().positionHistory.mainPlyCount + 1;

        await act(async () => {
            await gameEventHandlers.MoveMadeAsync?.(
                move,
                plyNumber,
                clocks,
                didMoveEndGame ?? false,
            );
        });
        return move;
    }

    describe("SyncRevisionAsync", () => {
        it("should refetch the game if revision is out of sync", async () => {
            const newRevision = 2;

            liveChessStore.setState({
                sourceRevision: 1,
            });

            renderLiveChessEvents();

            await act(async () => {
                await gameEventHandlers.SyncRevisionAsync?.(newRevision);
            });

            expect(refetchGame).toHaveBeenCalledExactlyOnceWith(
                liveChessStore,
                chessboardStore,
            );
        });

        it("should not refetch if revision matches", async () => {
            const newRevision = 1;

            liveChessStore.setState({
                sourceRevision: newRevision,
            });

            renderLiveChessEvents();

            await act(async () => {
                await gameEventHandlers.SyncRevisionAsync?.(newRevision);
            });

            expect(refetchGame).not.toHaveBeenCalled();
        });
    });

    describe("MoveMadeAsync", () => {
        it("should trigger a refetch when move doesn't succeed", async () => {
            handleMoveUpdateMock.mockResolvedValue(false);

            renderLiveChessEvents();

            await triggerMoveMade({
                sideToMove: GameColor.BLACK,
                plyNumber: 23,
            });

            expect(refetchGame).toHaveBeenCalled();
        });

        it("should do nothing if it's our turn next", async () => {
            liveChessStore.setState({
                viewer: { userId: "test-id", playerColor: GameColor.WHITE },
            });

            renderLiveChessEvents();
            await triggerMoveMade({ sideToMove: GameColor.WHITE });

            expect(handleMoveUpdateMock).not.toHaveBeenCalled();
        });

        it("should apply move", async () => {
            renderLiveChessEvents();

            const clocks = createFakeClocks();
            const move = await triggerMoveMade({
                sideToMove: GameColor.BLACK,
                plyNumber: 5,
                clocks,
            });

            expect(handleMoveUpdateMock).toHaveBeenCalledExactlyOnceWith(
                liveChessStore,
                chessboardStore,
                {
                    move,
                    decodedMove: decodeMovePath(move.path),
                    plyNumber: 5,
                    legalMoves: undefined,
                    clocks,
                },
            );
        });

        it("should set legal moves to empty if the move ended the game", async () => {
            const addPositionMock = vi.fn();
            chessboardStore.setState({ addPosition: addPositionMock });

            renderLiveChessEvents();
            const clocks = createFakeClocks();
            const move = await triggerMoveMade({
                sideToMove: GameColor.BLACK,
                plyNumber: 5,
                clocks,
                didMoveEndGame: true,
            });

            expect(handleMoveUpdateMock).toHaveBeenCalledExactlyOnceWith(
                liveChessStore,
                chessboardStore,
                {
                    move,
                    decodedMove: decodeMovePath(move.path),
                    plyNumber: 5,
                    legalMoves: LegalMoves.StableEmpty,
                    clocks,
                },
            );
        });
    });

    describe("OpponentMoveMadeAsync", () => {
        it("should apply move", async () => {
            renderLiveChessEvents();

            const fakeMoves: MovePath[] = [
                { fromIdx: 0, toIdx: 1, moveKey: "1" },
                { fromIdx: 10, toIdx: 11, moveKey: "2" },
            ];
            const encodedMoves = encodeMoves(fakeMoves);

            const move = createFakeMoveSnapshot({
                san: "test san",
                path: { fromIdx: 11, toIdx: 12, moveKey: "0" },
            });
            const plyNumber =
                chessboardStore.getState().positionHistory.mainPlyCount + 1;
            const clocks = createFakeClocks();
            await act(async () => {
                await gameEventHandlers.OpponentMoveMadeAsync?.(
                    move,
                    plyNumber,
                    encodedMoves,
                    clocks,
                );
            });

            const expectedLegalMoves = decodeMovePathIntoLegalMoves(fakeMoves);
            expect(handleMoveUpdateMock).toHaveBeenCalledExactlyOnceWith(
                liveChessStore,
                chessboardStore,
                {
                    move,
                    decodedMove: decodeMovePath(move.path),
                    plyNumber,
                    legalMoves: expectedLegalMoves,
                    clocks,
                },
            );
        });
    });

    describe("DrawStateChangeAsync", () => {
        it("should update the drawState in liveChessStore", () => {
            const initialDrawState = {
                activeRequester: null,
                whiteCooldown: 0,
                blackCooldown: 0,
            };

            liveChessStore.setState({
                drawState: initialDrawState,
            });

            renderLiveChessEvents();

            const newDrawState: DrawState = {
                activeRequester: GameColor.WHITE,
                whiteCooldown: 5,
                blackCooldown: 3,
            };

            act(() => {
                gameEventHandlers.DrawStateChangeAsync?.(newDrawState);
            });

            expect(liveChessStore.getState().drawState).toEqual(newDrawState);
        });
    });

    describe("ReceiveOvertimeAsync", () => {
        function setupOvertimeTest() {
            chessboardStore.setState({
                positionHistory: createNFakePositionHistory(3),
            });
        }

        it("should refetch the game if plyNumber is ahead by more than 1", async () => {
            setupOvertimeTest();
            renderLiveChessEvents();

            const mainPly =
                chessboardStore.getState().positionHistory.mainPlyCount;
            const removedFrom = logicalPoint({ x: 0, y: 0 });
            const encodedLegalMoves = encodeMoves([]);

            await act(async () => {
                gameEventHandlers.ReceiveOvertimeAsync?.(
                    mainPly + 2,
                    removedFrom,
                    encodedLegalMoves,
                );
            });

            expect(refetchGame).toHaveBeenCalledExactlyOnceWith(
                liveChessStore,
                chessboardStore,
            );
        });

        it("should apply overtime immediately if plyNumber matches current main ply", async () => {
            setupOvertimeTest();
            renderLiveChessEvents();

            const mainPly =
                chessboardStore.getState().positionHistory.mainPlyCount;
            const removedFrom = logicalPoint({ x: 1, y: 2 });

            const movePath = [createFakeMovePath()];
            const encodedLegalMoves = encodeMoves(movePath);
            const decodedLegalMoves = decodeMovePathIntoLegalMoves(movePath);

            const addLegalMovesSpy = vi.spyOn(
                chessboardStore.getState(),
                "addLegalMovesForPosition",
            );
            const removePieceSpy = vi.spyOn(
                chessboardStore.getState(),
                "removePieceAt",
            );

            const targetPosition = chessboardStore
                .getState()
                .positionHistory.getPositionWithPly(mainPly)!;

            await act(async () => {
                await gameEventHandlers.ReceiveOvertimeAsync?.(
                    mainPly,
                    removedFrom,
                    encodedLegalMoves,
                );
            });

            expect(addLegalMovesSpy).toHaveBeenCalledExactlyOnceWith(
                decodedLegalMoves,
                targetPosition.positionId,
            );
            expect(removePieceSpy).toHaveBeenCalledExactlyOnceWith(removedFrom);
        });

        it("should immediately remove a piece if ReceiveOvertimeAsync arrives for a past ply", async () => {
            setupOvertimeTest();
            renderLiveChessEvents();

            const mainPly =
                chessboardStore.getState().positionHistory.mainPlyCount;
            const removedFrom = logicalPoint({ x: 0, y: 0 });

            const removePieceSpy = vi.spyOn(
                chessboardStore.getState(),
                "removePieceAt",
            );

            await act(async () => {
                await gameEventHandlers.ReceiveOvertimeAsync?.(
                    mainPly - 1,
                    removedFrom,
                    encodeMoves([]),
                );
            });

            expect(removePieceSpy).toHaveBeenCalledExactlyOnceWith(removedFrom);
            const pastPosition = chessboardStore
                .getState()
                .positionHistory.getPositionWithPly(mainPly - 1)!;
            expect(pastPosition.move.overtimeRemovals).toContain(removedFrom);
        });

        it("should queue overtime for the next ply and apply it when the next move arrives", async () => {
            setupOvertimeTest();
            renderLiveChessEvents();

            const mainPly =
                chessboardStore.getState().positionHistory.mainPlyCount;
            const removedFrom = logicalPoint({ x: 1, y: 1 });

            const movePath = [createFakeMovePath()];
            const encodedLegalMoves = encodeMoves(movePath);

            await act(async () => {
                await gameEventHandlers.ReceiveOvertimeAsync?.(
                    mainPly + 1,
                    removedFrom,
                    encodedLegalMoves,
                );
            });
            const latestPosition =
                chessboardStore.getState().positionHistory.currentNode!;
            expect(latestPosition.move.overtimeRemovals.length).toBe(0);

            const move = await triggerMoveMade({ sideToMove: GameColor.BLACK });

            const expectedDecodedMove = decodeMovePath(move.path);
            expectedDecodedMove.overtimeRemovals = [removedFrom];
            expect(handleMoveUpdateMock).toHaveBeenCalledExactlyOnceWith(
                liveChessStore,
                chessboardStore,
                expect.objectContaining({ decodedMove: expectedDecodedMove }),
            );
        });

        it("should accumulate multiple overtime removals for the same ply and apply them when the next move arrives", async () => {
            setupOvertimeTest();
            renderLiveChessEvents();

            const mainPly =
                chessboardStore.getState().positionHistory.mainPlyCount;

            const removed1 = logicalPoint({ x: 1, y: 1 });
            const removed2 = logicalPoint({ x: 2, y: 2 });
            const removed3 = logicalPoint({ x: 3, y: 3 });

            const movePath = [createFakeMovePath()];
            const encodedLegalMoves = encodeMoves(movePath);

            await act(async () => {
                await gameEventHandlers.ReceiveOvertimeAsync?.(
                    mainPly + 1,
                    removed1,
                    encodedLegalMoves,
                );
                await gameEventHandlers.ReceiveOvertimeAsync?.(
                    mainPly + 1,
                    removed2,
                    encodedLegalMoves,
                );
                await gameEventHandlers.ReceiveOvertimeAsync?.(
                    mainPly + 1,
                    removed3,
                    encodedLegalMoves,
                );
            });

            const move = await triggerMoveMade({ sideToMove: GameColor.BLACK });

            const expectedDecodedMove = decodeMovePath(move.path);
            expectedDecodedMove.overtimeRemovals = [
                removed1,
                removed2,
                removed3,
            ];
            expect(handleMoveUpdateMock).toHaveBeenCalledExactlyOnceWith(
                liveChessStore,
                chessboardStore,
                expect.objectContaining({ decodedMove: expectedDecodedMove }),
            );
        });
    });

    describe("GameEndedAsync", () => {
        it("should update liveChessStore, disable chessboard movement, and set final clocks", async () => {
            liveChessStore.setState({ resultData: null });
            chessboardStore.setState({ allowHistoryChanges: false });
            chessboardStore
                .getState()
                .setLatestLegalMoves(createFakeLegalMoves());

            renderLiveChessEvents();

            const gameResult = createFakeGameResultData();
            const finalClocks = createFakeClocks({ isFrozen: true });

            await act(async () => {
                gameEventHandlers.GameEndedAsync?.(gameResult, finalClocks);
            });

            const liveState = liveChessStore.getState();
            expect(liveState.resultData).toEqual(gameResult);
            expect(liveState.liveClocks).toEqual(finalClocks);

            const chessboardState = chessboardStore.getState();
            expect(chessboardState.allowHistoryChanges).toBe(true);
        });
    });

    describe("ReceiveErrorAsync", () => {
        it("should refetch the game", async () => {
            renderLiveChessEvents();

            await act(async () => {
                gameEventHandlers.ReceiveErrorAsync?.([]);
            });

            expect(refetchGame).toHaveBeenCalledExactlyOnceWith(
                liveChessStore,
                chessboardStore,
            );
        });
    });

    describe("RematchAcceptedAsync", () => {
        it("should navigate when RematchAccepted fires", async () => {
            renderLiveChessEvents();
            const newGameToken = "new-game-token-999";

            await act(() =>
                gameEventHandlers.RematchAcceptedAsync?.(newGameToken),
            );

            expect(gameStartRedirect).toHaveBeenCalledExactlyOnceWith(
                newGameToken,
                expect.anything(),
            );
        });
    });
});
