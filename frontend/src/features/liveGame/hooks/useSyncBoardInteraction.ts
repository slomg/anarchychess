import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { useEffect } from "react";
import { StoreApi } from "zustand";
import { LiveChessStore } from "../stores/liveChessStore";

export default function useSyncBoardInteraction(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
): void {
    useEffect(() => {
        let prev: boolean | undefined;
        const unsub = liveChessStore.subscribe((state) => {
            const isInteractionAllowed = state.isInteractionAllowed();
            if (isInteractionAllowed === prev) return;
            prev = isInteractionAllowed;

            chessboardStore.getState().setHideLegalMoves(!isInteractionAllowed);
        });
        return unsub;
    }, [liveChessStore, chessboardStore]);
}
