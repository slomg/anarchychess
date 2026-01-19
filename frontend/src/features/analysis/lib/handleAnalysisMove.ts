import { StoreApi } from "zustand";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import {
    AnalysisPosition,
    ApiProblemDetails,
    getNextAnalysisPosition,
} from "@/lib/apiClient";
import { Move } from "@/features/chessboard/lib/types";
import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import { PositionProps } from "@/features/chessboard/lib/position";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import BoardPieces from "@/features/chessboard/lib/boardPieces";

export interface AnalysisMoveArgs {
    chessboardStore: StoreApi<ChessboardStore>;
    prevPieces: BoardPieces;
    rootFen: string;
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
    rootFen,
    move,
    prevPieces,
}: AnalysisMoveArgs): Promise<{
    positionProps: PositionProps;
    legalMoves: LegalMoves;
} | null> {
    const {
        pieces,
        boardDimensions,
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
                fen: positionHistory.viewingPosition?.fen ?? rootFen,
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
    const legalMoves = decodeMovePathIntoLegalMoves({
        paths: data.legalMoves,
        boardWidth: boardDimensions.width,
    });

    return { positionProps, legalMoves };
}
