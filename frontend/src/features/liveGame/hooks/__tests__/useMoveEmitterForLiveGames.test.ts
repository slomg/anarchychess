import { act, renderHook } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import {
    addSidelineAnalysisMove,
    AnalysisMoveArgs,
} from "@/features/analysis/lib/handleAnalysisMove";
import {
    createFakeBoardPieces,
    createFakeMove,
} from "@/lib/testUtils/fakers/chessboardFakers";
import createLiveChessStore, {
    LiveChessStore,
} from "../../stores/liveChessStore";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { PieceMovementEvent } from "@/features/chessboard/stores/piecesSlice";
import useMoveEmitterForLiveGames from "../useMoveEmitterForLiveGames";
import { GameResult } from "@/lib/apiClient";

vi.mock("@/features/analysis/lib/handleAnalysisMove");

describe("useMoveEmitterForLiveGames", () => {
    let liveChessStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;
    let event: PieceMovementEvent;

    const sendMoveEventMock = vi.fn();
    const addSidelineAnalysisMoveMock = vi.mocked(addSidelineAnalysisMove);

    beforeEach(() => {
        liveChessStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();

        event = {
            move: createFakeMove(),
            prevPieces: createFakeBoardPieces(),
            animationPromise: new Promise<void>(() => {}),
        };
    });

    it("should emit move events when a piece is moved", async () => {
        renderHook(() =>
            useMoveEmitterForLiveGames(
                liveChessStore,
                chessboardStore,
                sendMoveEventMock,
            ),
        );

        await act(() =>
            chessboardStore.getState().pieceMovementEvent.emit(event),
        );

        expect(sendMoveEventMock).toHaveBeenCalledExactlyOnceWith(event);
        expect(addSidelineAnalysisMoveMock).not.toHaveBeenCalled();
    });

    it("should treat move as analysis when the game is over", async () => {
        renderHook(() =>
            useMoveEmitterForLiveGames(
                liveChessStore,
                chessboardStore,
                sendMoveEventMock,
            ),
        );
        liveChessStore.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "desc",
            },
        });

        await act(() =>
            chessboardStore.getState().pieceMovementEvent.emit(event),
        );

        expect(addSidelineAnalysisMoveMock).toHaveBeenCalledExactlyOnceWith<
            [AnalysisMoveArgs]
        >({
            chessboardStore,
            prevPieces: event.prevPieces,
            move: event.move,
        });
        expect(sendMoveEventMock).not.toHaveBeenCalled();
    });
});
