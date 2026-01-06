import { StoreApi } from "zustand";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { getNextAnalysisPosition } from "@/lib/apiClient";
import { Move } from "@/features/chessboard/lib/types";
import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import { PositionProps } from "@/features/chessboard/lib/position";
import LegalMoves from "@/features/chessboard/lib/legalMoves";

export async function addAnalysisMove(
    chessboardStore: StoreApi<ChessboardStore>,
    rootFen: string,
    move: Move,
): Promise<void> {
    const result = await fetchNextPosition(chessboardStore, rootFen, move);
    if (result === null) return;

    const { addPosition, addLegalMoves } = chessboardStore.getState();
    const position = addPosition(result.positionProps);
    addLegalMoves(result.legalMoves, position.positionId);
}

export async function addSidelineAnalysisMove(
    chessboardStore: StoreApi<ChessboardStore>,
    rootFen: string,
    move: Move,
) {
    const result = await fetchNextPosition(chessboardStore, rootFen, move);
    if (result === null) return;

    const { addSidelinePosition, addLegalMoves } = chessboardStore.getState();
    const position = addSidelinePosition(result.positionProps);
    addLegalMoves(result.legalMoves, position.positionId);
}

async function fetchNextPosition(
    chessboardStore: StoreApi<ChessboardStore>,
    rootFen: string,
    move: Move,
): Promise<{ positionProps: PositionProps; legalMoves: LegalMoves } | null> {
    const { pieces, boardDimensions, positionHistory, goToPosition } =
        chessboardStore.getState();

    const nextPosition = positionHistory.getNextPositionWithKey(move.moveKey);
    if (nextPosition) {
        await goToPosition(nextPosition.positionId);
        return null;
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
        return null;
    }

    const positionProps: PositionProps = {
        pieces,
        move,
        sideToMove: data.sideToMove,
        fen: data.fen,
        san: data.san,
    };
    const legalMoves = decodeMovePathIntoLegalMoves({
        paths: data.moveOptions.legalMoves,
        boardWidth: boardDimensions.width,
        hasForcedMoves: data.moveOptions.hasForcedMoves,
    });

    return { positionProps, legalMoves };
}
