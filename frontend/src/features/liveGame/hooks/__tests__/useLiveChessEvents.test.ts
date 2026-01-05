import { renderHook } from "@testing-library/react";
import { brotliCompressSync } from "zlib";
import { StoreApi } from "zustand";
import { act } from "react";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import {
    createFakeLegalMoves,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";
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
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import { createFakeClock } from "@/lib/testUtils/fakers/clockFaker";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { GameClientEvents, useGameEvent } from "../useGameHub";
import { refetchGame } from "../../lib/gameStateProcessor";
import { logicalPoint } from "@/features/point/pointUtils";
import useLiveChessEvents from "../useLiveChessEvents";

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
            positionHistory: createNFakePositionHistory(3),
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
        plyNumber,
    }: {
        sideToMove: GameColor;
        clocks?: Clocks;
        plyNumber?: number;
    }): Promise<MoveSnapshot> {
        const move = createFakeMoveSnapshot({
            san: "test san",
            path: { fromIdx: 11, toIdx: 12, moveKey: "0" },
            nextSideToMove: sideToMove,
        });
        clocks ??= createFakeClock();
        plyNumber ??=
            chessboardStore.getState().positionHistory.mainPlyCount + 1;

        await act(async () => {
            await gameEventHandlers.MoveMadeAsync?.(move, plyNumber, clocks);
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
        it("should trigger a refetch when plyNumber is out of sync", async () => {
            chessboardStore.setState({
                positionHistory: createNFakePositionHistory(1),
            });

            renderLiveChessEvents();

            await triggerMoveMade({
                sideToMove: GameColor.BLACK,
                plyNumber: 23,
            });

            expect(refetchGame).toHaveBeenCalled();
            expect(
                chessboardStore.getState().positionHistory.totalPlyCount,
            ).toBe(1);
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
                    sideToMove: GameColor.BLACK,
                });

                expect(
                    chessboardStore.getState().positionHistory.totalPlyCount,
                ).toBe(positionHistoryBefore.totalPlyCount + 1);

                const piecesAfter = chessboardStore.getState().pieces;
                if (!awaitingAck) {
                    expect(piecesAfter).not.toEqual(piecesBefore);
                } else {
                    expect(piecesAfter).toEqual(piecesBefore);
                }

                expect(
                    chessboardStore.getState().positionHistory.viewingPosition,
                ).toEqual(
                    expect.objectContaining({
                        san: move.san,
                        move: decodeMovePath(move.path, 10),
                        pieces: piecesAfter,
                    }),
                );
            },
        );

        it("should go to the last position before playing the move", async () => {
            setupStandardStoresForMove();
            renderLiveChessEvents();

            const { goToStartPosition } = chessboardStore.getState();
            await goToStartPosition();

            await triggerMoveMade({
                sideToMove: GameColor.BLACK,
            });

            const {
                positionHistory: updatedPositionHistory,
                pieces: updatedPieces,
            } = chessboardStore.getState();

            expect(updatedPositionHistory.isViewingLatestPosition).toBe(true);
            expect(updatedPositionHistory.viewingPosition?.pieces).toEqual(
                updatedPieces,
            );
        });

        it("should do nothing if it's our turn next", async () => {
            const addPositionMock = vi.fn();
            chessboardStore.setState({ addPosition: addPositionMock });
            liveChessStore.setState({
                viewer: { userId: "test-id", playerColor: GameColor.WHITE },
            });

            setupStandardStoresForMove();
            renderLiveChessEvents();
            await triggerMoveMade({ sideToMove: GameColor.WHITE });

            expect(addPositionMock).not.toHaveBeenCalled();
        });
    });

    describe("OpponentMoveMadeAsync", () => {
        function encodeMoves(moves: MovePath[]): string {
            const json = JSON.stringify(moves);
            const compressed = brotliCompressSync(Buffer.from(json));
            return compressed.toString("base64");
        }

        it("should apply move and set legal moves for the next player", async () => {
            setupStandardStoresForMove();
            renderLiveChessEvents();

            const fakeMoves: MovePath[] = [
                { fromIdx: 0, toIdx: 1, moveKey: "1" },
                { fromIdx: 10, toIdx: 11, moveKey: "2" },
            ];
            const encodedMoves = encodeMoves(fakeMoves);
            const hasForcedMoves = true;

            await act(async () => {
                await gameEventHandlers.OpponentMoveMadeAsync?.(
                    createFakeMoveSnapshot({
                        san: "test san",
                        path: { fromIdx: 11, toIdx: 12, moveKey: "0" },
                    }),
                    chessboardStore.getState().positionHistory.mainPlyCount + 1,
                    encodedMoves,
                    hasForcedMoves,
                    createFakeClock(),
                );
            });

            const position =
                chessboardStore.getState().positionHistory.viewingPosition!;
            const expectedLegalMoves = decodeMovePathIntoLegalMoves({
                paths: fakeMoves,
                boardWidth: 10,
                hasForcedMoves,
            });
            expect(position).toBeDefined();
            expect(chessboardStore.getState().getLegalMoves()).toEqual(
                expectedLegalMoves,
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
            liveChessStore.setState({ resultData: null });
            chessboardStore.setState({ allowHistoryChanges: false });
            chessboardStore
                .getState()
                .setLatestLegalMoves(createFakeLegalMoves());

            renderLiveChessEvents();

            const gameResult = createFakeGameResultData();
            const finalClocks = createFakeClock({ isFrozen: true });

            await act(async () => {
                gameEventHandlers.GameEndedAsync?.(gameResult, finalClocks);
            });

            const liveState = liveChessStore.getState();
            expect(liveState.resultData).toEqual(gameResult);
            expect(liveState.clocks).toEqual(finalClocks);

            const chessboardState = chessboardStore.getState();
            expect(chessboardState.allowHistoryChanges).toBe(true);
        });
    });
});
