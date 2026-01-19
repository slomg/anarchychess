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
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import useLiveMoveEmitter from "../useLiveMoveEmitter";
import { useGameEmitter } from "../useGameHub";
import { GameResult } from "@/lib/apiClient";

vi.mock("@/features/liveGame/hooks/useGameHub");
vi.mock("@/features/analysis/lib/handleAnalysisMove");

describe("useLiveMoveEmitter", () => {
    let liveChessStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;
    let prevPieces: BoardPieces;

    const sendGameEventMock = vi.fn();
    const addSidelineAnalysisMoveMock = vi.mocked(addSidelineAnalysisMove);

    beforeEach(() => {
        liveChessStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();
        prevPieces = createFakeBoardPieces();

        vi.mocked(useGameEmitter).mockReturnValue(sendGameEventMock);
    });

    it("should emit move events when a piece is moved", async () => {
        renderHook(() => useLiveMoveEmitter(liveChessStore, chessboardStore));

        const move = createFakeMove();
        await act(() =>
            chessboardStore
                .getState()
                .pieceMovementEvent.emit(move, prevPieces),
        );

        expect(sendGameEventMock).toHaveBeenCalledExactlyOnceWith(
            "MovePieceAsync",
            liveChessStore.getState().gameToken,
            move.moveKey,
        );
        expect(addSidelineAnalysisMoveMock).not.toHaveBeenCalled();
    });

    it("should treat move as analysis when the game is over", async () => {
        renderHook(() => useLiveMoveEmitter(liveChessStore, chessboardStore));
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
        expect(sendGameEventMock).not.toHaveBeenCalled();
    });
});
