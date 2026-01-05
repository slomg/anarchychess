import { renderHook } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import createLiveChessStore, {
    LiveChessStore,
} from "../../stores/liveChessStore";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import handleAnalysisMove from "@/features/analysis/lib/handleAnalysisMove";
import { createFakeMove } from "@/lib/testUtils/fakers/chessboardFakers";
import useLiveMoveEmitter from "../useLiveMoveEmitter";
import { useGameEmitter } from "../useGameHub";
import { GameResult } from "@/lib/apiClient";

vi.mock("@/features/liveGame/hooks/useGameHub");
vi.mock("@/features/analysis/lib/handleAnalysisMove");

describe("useLiveMoveEmitter", () => {
    let liveChessStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;

    const sendGameEventMock = vi.fn();
    const handleAnalysisMoveMock = vi.mocked(handleAnalysisMove);

    beforeEach(() => {
        liveChessStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();

        vi.mocked(useGameEmitter).mockReturnValue(sendGameEventMock);
    });

    it("should emit move events when a piece is moved", async () => {
        renderHook(() => useLiveMoveEmitter(liveChessStore, chessboardStore));

        const move = createFakeMove();
        chessboardStore.getState().pieceMovementEvent.emit(move);

        expect(sendGameEventMock).toHaveBeenCalledExactlyOnceWith(
            "MovePieceAsync",
            liveChessStore.getState().gameToken,
            move.moveKey,
        );
        expect(handleAnalysisMoveMock).not.toHaveBeenCalled();
    });

    it("should treat move as analysis when the game is over", () => {
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
        chessboardStore.getState().pieceMovementEvent.emit(move);

        expect(handleAnalysisMoveMock).toHaveBeenCalledExactlyOnceWith(
            chessboardStore,
            initialFen,
            move,
        );
        expect(sendGameEventMock).not.toHaveBeenCalled();
    });
});
