import { StoreApi } from "zustand";
import createLiveChessStore, {
    LiveChessStore,
} from "../../stores/liveChessStore";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { act, renderHook } from "@testing-library/react";
import useSyncBoardInteraction from "../useSyncBoardInteraction";
import { GameColor } from "@/lib/apiClient";

describe("useSyncBoardInteraction", () => {
    let liveChessStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;

    beforeEach(() => {
        liveChessStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();
    });

    it("should hide legal moves when interaction is not allowed and show when allowed", () => {
        chessboardStore.setState({ hideLegalMoves: true });
        // our turn
        liveChessStore.setState({
            viewer: {
                userId: "user id",
                playerColor: GameColor.WHITE,
            },
            sideToMove: GameColor.WHITE,
        });

        renderHook(() =>
            useSyncBoardInteraction(liveChessStore, chessboardStore),
        );

        expect(chessboardStore.getState().hideLegalMoves).toBe(false);

        // opponent side to move
        act(() => {
            liveChessStore.setState({ sideToMove: GameColor.BLACK });
        });

        expect(chessboardStore.getState().hideLegalMoves).toBe(true);
    });

    it("should reselect piece", () => {
        const reselectPieceMock = vi.fn();
        chessboardStore.setState({
            hideLegalMoves: true,
            reselectPiece: reselectPieceMock,
        });
        liveChessStore.setState({
            viewer: {
                userId: "user id",
                playerColor: GameColor.WHITE,
            },
            sideToMove: GameColor.BLACK,
        });

        renderHook(() =>
            useSyncBoardInteraction(liveChessStore, chessboardStore),
        );

        act(() => {
            liveChessStore.setState({ sideToMove: GameColor.WHITE });
        });

        expect(reselectPieceMock).toHaveBeenCalledOnce();
    });
});
