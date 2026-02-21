import { StoreApi } from "zustand";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { LiveChessStore } from "../stores/liveChessStore";
import { Clocks, MoveSnapshot } from "@/lib/apiClient";
import { Move } from "@/features/chessboard/lib/types";
import { decodeMovePath } from "./moveDecoder";

export default async function handleMoveUpdate(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
    {
        move,
        decodedMove,
        plyNumber,
        legalMoves,
        clocks,
    }: {
        move: MoveSnapshot;
        decodedMove?: Move;
        plyNumber: number;
        legalMoves?: LegalMoves;
        clocks?: Clocks;
    },
): Promise<boolean> {
    const {
        positionHistory,
        addPosition,
        applyMoveAnimated,
        goToLatestPosition,
    } = chessboardStore.getState();
    const { isPendingMoveAck, receiveLiveMove } = liveChessStore.getState();

    if (plyNumber - 1 !== positionHistory.mainPlyCount) {
        return false;
    }
    await goToLatestPosition();

    decodedMove ??= decodeMovePath(
        move.path,
        chessboardStore.getState().boardDimensions.width,
    );
    if (!isPendingMoveAck) {
        await applyMoveAnimated(decodedMove);
    }

    const pieces = chessboardStore.getState().pieces;
    addPosition(
        {
            pieces,
            fen: move.fen,
            sideToMove: move.nextSideToMove,
            san: move.san,
            move: decodedMove,
        },
        legalMoves,
    );
    receiveLiveMove(plyNumber, move.nextSideToMove, clocks);
    return true;
}
