import { StoreApi } from "zustand";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { getNextAnalysisPosition } from "@/lib/apiClient";
import { Move } from "@/features/chessboard/lib/types";
import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";

export default async function handleAnalysisMove(
    chessboardStore: StoreApi<ChessboardStore>,
    rootFen: string,
    move: Move,
): Promise<void> {
    const {
        pieces,
        boardDimensions,
        positionHistory,
        goToPosition,
        addPosition,
        addLegalMoves,
    } = chessboardStore.getState();

    const nextPosition = positionHistory.getNextPositionWithKey(move.moveKey);
    if (nextPosition) {
        await goToPosition(nextPosition.positionId);
        return;
    }

    const { error, data } = await getNextAnalysisPosition({
        body: {
            fen: positionHistory.viewingPosition?.fen ?? rootFen,
            piecePosition: move.from,
            moveKey: move.moveKey,
        },
    });
    if (error || data === undefined) {
        console.error(error);
        return;
    }

    const position = addPosition({
        pieces,
        move,
        sideToMove: data.sideToMove,
        fen: data.fen,
        san: data.san,
    });

    const decodedLegalMoves = decodeMovePathIntoLegalMoves({
        paths: data.moveOptions.legalMoves,
        boardWidth: boardDimensions.width,
        hasForcedMoves: data.moveOptions.hasForcedMoves,
    });
    addLegalMoves(decodedLegalMoves, position.positionId);
}
