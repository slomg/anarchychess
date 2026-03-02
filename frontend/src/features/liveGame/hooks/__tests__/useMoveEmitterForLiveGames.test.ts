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
import useMoveEmitterForLiveGames from "../useMoveEmitterForLiveGames";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { GameResult } from "@/lib/apiClient";

vi.mock("@/features/analysis/lib/handleAnalysisMove");

describe("useMoveEmitterForLiveGames", () => {
    let liveChessStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;
    let prevPieces: BoardPieces;

    const sendMoveEventMock = vi.fn();
    const addSidelineAnalysisMoveMock = vi.mocked(addSidelineAnalysisMove);

    beforeEach(() => {
        liveChessStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();
        prevPieces = createFakeBoardPieces();
    });

    it("should emit move events when a piece is moved", async () => {
        renderHook(() =>
            useMoveEmitterForLiveGames(
                liveChessStore,
                chessboardStore,
                sendMoveEventMock,
            ),
        );

        const move = createFakeMove();
        await act(() =>
            chessboardStore
                .getState()
                .pieceMovementEvent.emit(move, prevPieces),
        );

        expect(sendMoveEventMock).toHaveBeenCalledExactlyOnceWith(move.moveKey);
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
        const initialFen = "test initial fen";
        liveChessStore.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "desc",
            },
            initialFen,
        });

        const move = createFakeMove();
        await act(() =>
            chessboardStore
                .getState()
                .pieceMovementEvent.emit(move, prevPieces),
        );

        expect(addSidelineAnalysisMoveMock).toHaveBeenCalledExactlyOnceWith<
            [AnalysisMoveArgs]
        >({
            chessboardStore,
            prevPieces,
            rootFen: initialFen,
            move,
        });
        expect(sendMoveEventMock).not.toHaveBeenCalled();
    });
});
