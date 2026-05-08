import { StoreApi } from "zustand";

import {
    AnalysisPosition,
    ApiProblemDetails,
    getNextAnalysisPosition,
} from "@/lib/apiClient";

import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { PositionProps } from "@/features/chessboard/lib/position";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { Move } from "@/features/chessboard/lib/types";

export interface AnalysisMoveArgs {
    chessboardStore: StoreApi<ChessboardStore>;
    prevPieces: BoardPieces;
    move: Move;
}

export async function addAnalysisMove(args: AnalysisMoveArgs): Promise<void> {
    const result = await fetchNextPosition(args);
    if (result === null) return;

    const { addPosition } = args.chessboardStore.getState();
    addPosition(result.positionProps, result.legalMoves);
}

export async function addSidelineAnalysisMove(args: AnalysisMoveArgs) {
    const result = await fetchNextPosition(args);
    if (result === null) return;

    const { addSidelinePosition } = args.chessboardStore.getState();
    addSidelinePosition(result.positionProps, result.legalMoves);
}

async function fetchNextPosition({
    chessboardStore,
    move,
    prevPieces,
}: AnalysisMoveArgs): Promise<{
    positionProps: PositionProps;
    legalMoves: LegalMoves;
} | null> {
    const {
        pieces,
        positionHistory,
        hideLegalMoves: initialHideLegalMoves,
        setImmediatePieces,
        setPosition,
        setHideLegalMoves,
    } = chessboardStore.getState();

    const nextPosition = positionHistory.getNextPositionWithKey(move.moveKey);
    if (nextPosition) {
        setPosition(nextPosition.positionId);
        return null;
    }

    setHideLegalMoves(true);
    let data: AnalysisPosition | undefined;
    let error: ApiProblemDetails | undefined;
    try {
        ({ error, data } = await getNextAnalysisPosition({
            body: {
                fen:
                    positionHistory.viewingPosition?.fen ??
                    positionHistory.root.fen,
                piecePosition: move.from,
                moveKey: move.moveKey,
            },
        }));
    } catch (err) {
        setImmediatePieces(prevPieces);
        throw err;
    } finally {
        setHideLegalMoves(initialHideLegalMoves);
    }

    if (error || data === undefined) {
        console.error(
            "handleAnalyisisMove fetchNextPosition getNextAnalysisPosition",
            error,
        );
        setImmediatePieces(prevPieces);
        return null;
    }

    const positionProps: PositionProps = {
        pieces,
        move,
        sideToMove: data.sideToMove,
        fen: data.fen,
        san: data.san,
    };
    const legalMoves = decodeMovePathIntoLegalMoves(data.legalMoves);

    return { positionProps, legalMoves };
}
