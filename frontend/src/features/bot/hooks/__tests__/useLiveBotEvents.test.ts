import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import { BotClientEvents, useBotEvent } from "../useBotHub";
import { StoreApi } from "zustand";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import {
    createFakeBoardPieces,
    createFakeLegalMoves,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { act, renderHook } from "@testing-library/react";
import useLiveBotEvents from "../useLiveBotEvents";
import { createFakeGameResultData } from "@/lib/testUtils/fakers/gameResultDataFaker";
import { refetchBotGame } from "../../lib/botStateProcessor";
import { createFakeMoveSnapshot } from "@/lib/testUtils/fakers/moveSnapshotFaker";
import handleMoveUpdate from "@/features/liveGame/lib/handleMoveUpdate";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";

vi.mock("@/features/bot/lib/botStateProcessor");
vi.mock("@/features/bot/hooks/useBotHub");
vi.mock("@/features/liveGame/lib/handleMoveUpdate");

describe("useLiveBotEvents", () => {
    let liveChessStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;

    const useBotEventMock = vi.mocked(useBotEvent);
    const refetchBotGameMock = vi.mocked(refetchBotGame);
    const handleMoveUpdateMock = vi.mocked(handleMoveUpdate);

    const botEventHandlers: EventHandlers<BotClientEvents> = {};

    beforeEach(() => {
        liveChessStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();

        handleMoveUpdateMock.mockResolvedValue(true);
        useBotEventMock.mockImplementation((_, event, handler) => {
            botEventHandlers[event] = handler;
        });
    });

    describe("SyncPlyNumberAsync", () => {
        it("should refetch the game if ply number out of sync", async () => {
            const positionHistory = new PositionHistory(
                createFakeBoardPieces(),
            );
            positionHistory.addNextPosition(createFakePositionProps());
            positionHistory.addNextPosition(createFakePositionProps());
            positionHistory.addNextPosition(createFakePositionProps());
            chessboardStore.setState({ positionHistory });

            renderHook(() => useLiveBotEvents(liveChessStore, chessboardStore));

            await act(async () => {
                await botEventHandlers.SyncPlyNumberAsync?.(4);
            });

            expect(refetchBotGame).toHaveBeenCalledExactlyOnceWith(
                liveChessStore,
                chessboardStore,
            );
        });

        it("should not refetch if revision matches", async () => {
            const positionHistory = new PositionHistory(
                createFakeBoardPieces(),
            );
            positionHistory.addNextPosition(createFakePositionProps());
            positionHistory.addNextPosition(createFakePositionProps());
            positionHistory.addNextPosition(createFakePositionProps());
            chessboardStore.setState({ positionHistory });

            renderHook(() => useLiveBotEvents(liveChessStore, chessboardStore));

            await act(async () => {
                await botEventHandlers.SyncPlyNumberAsync?.(3);
            });

            expect(refetchBotGame).not.toHaveBeenCalled();
        });
    });

    describe("PlayerMadeMoveAsync", () => {
        it("should trigger a refetch when move doesn't succeed", async () => {
            handleMoveUpdateMock.mockResolvedValue(false);

            renderHook(() => useLiveBotEvents(liveChessStore, chessboardStore));

            await act(() =>
                botEventHandlers.PlayerMadeMoveAsync?.(
                    createFakeMoveSnapshot(),
                    5,
                    false,
                ),
            );

            expect(refetchBotGameMock).toHaveBeenCalledOnce();
        });

        it("should play move", async () => {
            renderHook(() => useLiveBotEvents(liveChessStore, chessboardStore));

            const move = createFakeMoveSnapshot();
            const plyNumber = 2;
            const didMoveEndGame = false;
            await act(() =>
                botEventHandlers.PlayerMadeMoveAsync?.(
                    move,
                    plyNumber,
                    didMoveEndGame,
                ),
            );

            expect(handleMoveUpdateMock).toHaveBeenCalledExactlyOnceWith(
                liveChessStore,
                chessboardStore,
                {
                    move,
                    plyNumber,
                    legalMoves: undefined,
                },
            );
            expect(refetchBotGameMock).not.toHaveBeenCalled();
        });

        it("should set legal moves to empty if move ended game", async () => {
            renderHook(() => useLiveBotEvents(liveChessStore, chessboardStore));

            const move = createFakeMoveSnapshot();
            const plyNumber = 2;
            const didMoveEndGame = true;
            await act(() =>
                botEventHandlers.PlayerMadeMoveAsync?.(
                    move,
                    plyNumber,
                    didMoveEndGame,
                ),
            );

            expect(handleMoveUpdateMock).toHaveBeenCalledExactlyOnceWith(
                liveChessStore,
                chessboardStore,
                {
                    move,
                    plyNumber,
                    legalMoves: LegalMoves.StableEmpty,
                },
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

            renderHook(() => useLiveBotEvents(liveChessStore, chessboardStore));

            const gameResult = createFakeGameResultData();

            await act(async () => {
                botEventHandlers.GameEndedAsync?.(gameResult);
            });

            const liveState = liveChessStore.getState();
            expect(liveState.resultData).toEqual(gameResult);

            const chessboardState = chessboardStore.getState();
            expect(chessboardState.allowHistoryChanges).toBe(true);
        });
    });
});
