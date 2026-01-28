import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { useEffect } from "react";
import { StoreApi } from "zustand";
import { LiveChessStore } from "../stores/liveChessStore";

export default function useSyncBoardInteraction(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
): void {
    useEffect(() => {
        const { isInteractionAllowed } = liveChessStore.getState();
        const { setHideLegalMoves } = chessboardStore.getState();

        const initialInteractionAllowed = isInteractionAllowed();
        setHideLegalMoves(!initialInteractionAllowed);

        let prevInteractionAllowed = initialInteractionAllowed;
        const unsub = liveChessStore.subscribe((state) => {
            const updatedInteractionAllowed = state.isInteractionAllowed();
            if (updatedInteractionAllowed === prevInteractionAllowed) return;
            prevInteractionAllowed = updatedInteractionAllowed;

            setHideLegalMoves(!updatedInteractionAllowed);
        });
        return unsub;
    }, [liveChessStore, chessboardStore]);
}
