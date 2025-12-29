import { StoreApi } from "zustand";
import createLiveChessStore, {
    LiveChessStore,
} from "../../stores/liveChessStore";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import useLiveChessEvents from "../useLiveChessEvents";
import {
    Clocks,
    DrawState,
    GameColor,
    GameResult,
    GameResultData,
    MovePath,
    MoveSnapshot,
} from "@/lib/apiClient";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import { renderHook } from "@testing-library/react";
import { createFakeMoveSnapshot } from "@/lib/testUtils/fakers/moveSnapshotFaker";
import { createFakeClock } from "@/lib/testUtils/fakers/clockFaker";
import { act } from "react";
import { refetchGame } from "../../lib/gameStateProcessor";
import { createFakePosition } from "@/lib/testUtils/fakers/positionFaker";
import {
    createFakeLegalMoves,
    createFakePiece,
    createRandomPoint,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { logicalPoint } from "@/features/point/pointUtils";
import { brotliCompressSync } from "zlib";
import {
    decodeMovePath,
    decodeMovePathIntoLegalMoves,
} from "../../lib/moveDecoder";
import { GameClientEvents, useGameEvent } from "../useGameHub";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { Position } from "@/features/chessboard/lib/types";

vi.mock("@/features/liveGame/hooks/useGameHub");
vi.mock("@/features/liveGame/lib/gameStateProcessor");

describe("useLiveChessEvents", () => {
    let liveChessStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;

    const useGameEventMock = vi.mocked(useGameEvent);
    const gameEventHandlers: EventHandlers<GameClientEvents> = {};

    beforeEach(() => {
        liveChessStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();

        useGameEventMock.mockImplementation((_, event, handler) => {
            gameEventHandlers[event] = handler;
        });
    });

    function renderLiveChessEvents() {
        return renderHook(() =>
            useLiveChessEvents(liveChessStore, chessboardStore),
        );
    }

    function setupStandardStoresForMove() {
        const piece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        chessboardStore.setState({
            viewingPlyIdx: 0,
            positionHistory: [createFakePosition()],
            pieces: BoardPieces.fromPieces(piece),
        });
        liveChessStore.setState({
            viewer: { userId: "test id", playerColor: GameColor.WHITE },
        });
        return piece;
    }

    async function triggerMoveMade({
        sideToMove,
        clocks,
        plyIdx,
    }: {
        sideToMove: GameColor;
        clocks?: Clocks;
        plyIdx?: number;
    }): Promise<MoveSnapshot> {
        const move = createFakeMoveSnapshot({
            san: "test san",
            path: { fromIdx: 11, toIdx: 12, moveKey: "0" },
        });
        clocks ??= createFakeClock();
        plyIdx ??= chessboardStore.getState().positionHistory.length - 1;

        await act(async () => {
            await gameEventHandlers.MoveMadeAsync?.(
                move,
                sideToMove,
                plyIdx,
                clocks,
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

            expect(refetchGame).toHaveBeenCalledWith(
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
        it("should trigger a refetch when plyIdx is out of sync", async () => {
            chessboardStore.setState({
                positionHistory: [createFakePosition()],
            });

            renderLiveChessEvents();

            await triggerMoveMade({ sideToMove: GameColor.WHITE, plyIdx: 23 });

            expect(refetchGame).toHaveBeenCalled();
            expect(chessboardStore.getState().positionHistory.length).toBe(1);
        });

        it.each([true, false])(
            "should only play and store the move if we are not awaiting move ack",
            async (awaitingAck) => {
                setupStandardStoresForMove();
                renderLiveChessEvents();

                const piecesBefore = chessboardStore.getState().pieces;
                const positionHistoryBefore =
                    chessboardStore.getState().positionHistory;
                if (awaitingAck) liveChessStore.getState().markPendingMoveAck();

                const move = await triggerMoveMade({
                    sideToMove: GameColor.WHITE,
                });

                expect(chessboardStore.getState().viewingPlyIdx).toBe(
                    positionHistoryBefore.length,
                );
                expect(chessboardStore.getState().positionHistory.length).toBe(
                    positionHistoryBefore.length + 1,
                );

                const piecesAfter = chessboardStore.getState().pieces;
                if (!awaitingAck) {
                    expect(piecesAfter).not.toEqual(piecesBefore);
                } else {
                    expect(piecesAfter).toEqual(piecesBefore);
                }

                expect(
                    chessboardStore.getState().positionHistory[1],
                ).toEqual<Position>({
                    san: move.san,
                    move: decodeMovePath(move.path, 10),
                    pieces: piecesAfter,
                });
            },
        );

        it.each([
            [GameColor.WHITE, GameColor.BLACK],
            [GameColor.WHITE, GameColor.WHITE],
        ])(
            "should only disable movement if the side to move !== us",
            async (ourColor, newSideToMove) => {
                liveChessStore.setState({
                    viewer: { userId: "test id", playerColor: ourColor },
                });
                const disableMovementMock = vi.fn();
                chessboardStore.setState({
                    disableMovement: disableMovementMock,
                });

                setupStandardStoresForMove();
                renderLiveChessEvents();

                await triggerMoveMade({ sideToMove: newSideToMove });

                if (ourColor !== newSideToMove) {
                    expect(disableMovementMock).toHaveBeenCalledOnce();
                } else {
                    expect(disableMovementMock).not.toHaveBeenCalledOnce();
                }
            },
        );
    });

    describe("LegalMovesChangedAsync", () => {
        function encodeMoves(moves: MovePath[]): string {
            const json = JSON.stringify(moves);
            const compressed = brotliCompressSync(Buffer.from(json));
            return compressed.toString("base64");
        }

        it("should decode legal moves and update both stores", async () => {
            const addLegalMovesMock = vi.fn();
            chessboardStore.setState({ addLegalMoves: addLegalMovesMock });

            renderLiveChessEvents();
            const fakeMoves: MovePath[] = [
                {
                    fromIdx: 0,
                    toIdx: 1,
                    moveKey: "1",
                    triggerIdxs: [2],
                    capturedIdxs: [3],
                    sideEffects: [{ fromIdx: 4, toIdx: 5 }],
                    promotesTo: null,
                },
                {
                    fromIdx: 10,
                    toIdx: 11,
                    moveKey: "2",
                },
            ];
            const encodedMoves = encodeMoves(fakeMoves);
            const hasForcedMoves = true;
            const plyIdx = 68;

            await act(async () =>
                gameEventHandlers.LegalMovesChangedAsync?.(
                    encodedMoves,
                    hasForcedMoves,
                    plyIdx,
                ),
            );

            const expectedLegalMoves = decodeMovePathIntoLegalMoves({
                paths: fakeMoves,
                boardWidth: 10,
                hasForcedMoves,
            });
            expect(addLegalMovesMock).toHaveBeenCalledExactlyOnceWith(
                expectedLegalMoves,
                plyIdx + 1,
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

    describe("GameEndedAsync", () => {
        it("should update liveChessStore, disable chessboard movement, and set final clocks", async () => {
            liveChessStore.setState({
                resultData: null,
            });
            chessboardStore.setState({
                highlightedLegalMoves: [
                    createRandomPoint(),
                    createRandomPoint(),
                ],
                selectedPieceId: "123",
            });
            chessboardStore
                .getState()
                .setLatestLegalMoves(createFakeLegalMoves());

            renderLiveChessEvents();

            const gameResult: GameResultData = {
                whiteRatingChange: 10,
                blackRatingChange: -10,
                result: GameResult.WHITE_WIN,
                resultDescription: "test",
            };
            const finalClocks: Clocks = {
                whiteClock: 6,
                blackClock: 9,
                lastUpdated: 1234,
                isFrozen: true,
            };

            await act(async () => {
                gameEventHandlers.GameEndedAsync?.(gameResult, finalClocks);
            });

            const liveState = liveChessStore.getState();
            expect(liveState.resultData).toEqual(gameResult);
            expect(liveState.clocks).toEqual(finalClocks);

            const chessboardState = chessboardStore.getState();
            expect(chessboardState.highlightedLegalMoves).toHaveLength(0);
            expect(chessboardState.selectedPieceId).toBeNull();
            expect(chessboardState.getLegalMoves().size).toBe(0);
        });
    });
});
