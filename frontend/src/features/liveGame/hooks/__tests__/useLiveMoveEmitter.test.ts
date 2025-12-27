import { StoreApi } from "zustand";
import createLiveChessStore, {
    LiveChessStore,
} from "../../stores/liveChessStore";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { useGameEmitter } from "../useGameHub";
import { renderHook } from "@testing-library/react";
import useLiveMoveEmitter from "../useLiveMoveEmitter";
import { createFakeMove } from "@/lib/testUtils/fakers/chessboardFakers";

vi.mock("@/features/liveGame/hooks/useGameHub");

describe("useLiveMoveEmitter", () => {
    let liveChessStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;
    const mockSendGameEvent = vi.fn();

    beforeEach(() => {
        liveChessStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();

        vi.mocked(useGameEmitter).mockReturnValue(mockSendGameEvent);
    });

    it("should emit move events when a piece is moved", async () => {
        renderHook(() => useLiveMoveEmitter(liveChessStore, chessboardStore));

        const move = createFakeMove();
        chessboardStore.getState().pieceMovementEvent.emit(move);

        expect(mockSendGameEvent).toHaveBeenCalledWith(
            "MovePieceAsync",
            liveChessStore.getState().gameToken,
            move.moveKey,
        );
    });
});
