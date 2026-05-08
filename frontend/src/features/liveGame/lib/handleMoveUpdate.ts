import { StoreApi } from "zustand";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { LiveChessStore } from "../stores/liveChessStore";
import { Clocks, MoveSnapshot } from "@/lib/apiClient";
import { Move } from "@/features/chessboard/lib/types";
import { decodeMovePath } from "./moveDecoder";
import { simulateMove } from "@/features/chessboard/lib/simulateMove";

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
        addLatestPosition,
        applyMoveAnimated,
        goToLatestPosition,
    } = chessboardStore.getState();
    const { isPendingMoveAck, receiveLiveMove } = liveChessStore.getState();

    if (positionHistory.mainPlyCount !== plyNumber - 1) {
        return false;
    }

    await goToLatestPosition();

    decodedMove ??= decodeMovePath(move.path);

    let pieces = positionHistory.tail?.pieces ?? positionHistory.rootPieces;
    pieces = simulateMove(pieces, decodedMove).newPieces;
    if (!isPendingMoveAck) {
        await applyMoveAnimated(decodedMove);
    }

    addLatestPosition(
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
