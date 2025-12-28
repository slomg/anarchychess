import brotliDecompress from "brotli/decompress";

import {
    IntermediateSquare,
    MoveSideEffect,
    Piece,
} from "@/features/chessboard/lib/types";
import { Move } from "@/features/chessboard/lib/types";
import {
    IntermediateSquarePath,
    MovePath,
    MoveSideEffectPath,
    PieceSpawnPath,
} from "@/lib/apiClient";
import { idxToLogicalPoint, pointToStr } from "@/features/point/pointUtils";
import { createPieceId } from "@/features/chessboard/lib/pieceUtils";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { StrPoint } from "@/features/point/types";

export function decodeMovePathIntoLegalMoves({
    paths,
    boardWidth,
    hasForcedMoves,
}: {
    paths: MovePath[];
    boardWidth: number;
    hasForcedMoves: boolean;
}): LegalMoves {
    const moves = new Map<StrPoint, Move[]>();
    for (const path of paths) {
        const move = decodeMovePath(path, boardWidth);
        const fromString = pointToStr(move.from);
        const movesFromPoint = moves.get(fromString) ?? [];
        movesFromPoint.push(move);

        moves.set(pointToStr(move.from), movesFromPoint);
    }

    return new LegalMoves(moves, hasForcedMoves);
}

export function decodeMovePath(path: MovePath, boardWidth: number): Move {
    const from = idxToLogicalPoint(path.fromIdx, boardWidth);
    const to = idxToLogicalPoint(path.toIdx, boardWidth);
    const triggers =
        path.triggerIdxs?.map((idx) => idxToLogicalPoint(idx, boardWidth)) ??
        [];
    const captures =
        path.capturedIdxs?.map((idx) => idxToLogicalPoint(idx, boardWidth)) ??
        [];
    const intermediates =
        path.intermediateSquares?.map((x) =>
            parseIntermediateSquares(x, boardWidth),
        ) ?? [];
    const sideEffects =
        path.sideEffects?.map((x) => parseSideEffect(x, boardWidth)) ?? [];
    const pieceSpawns =
        path.pieceSpawns?.map((x) => parsePieceSpawns(x, boardWidth)) ?? [];

    return {
        from,
        to,
        moveKey: path.moveKey,
        triggers,
        captures,
        intermediates,
        sideEffects,
        pieceSpawns,
        promotesTo: path.promotesTo ?? null,
        specialType: path.specialType ?? null,
    };
}

function parseSideEffect(
    path: MoveSideEffectPath,
    boardWidth: number,
): MoveSideEffect {
    const from = idxToLogicalPoint(path.fromIdx, boardWidth);
    const to = idxToLogicalPoint(path.toIdx, boardWidth);
    return {
        from,
        to,
    };
}

function parsePieceSpawns(path: PieceSpawnPath, boardWidth: number): Piece {
    const position = idxToLogicalPoint(path.posIdx, boardWidth);
    return {
        id: createPieceId(),
        type: path.type,
        color: path.color ?? null,
        position,
    };
}

function parseIntermediateSquares(
    path: IntermediateSquarePath,
    boardWidth: number,
): IntermediateSquare {
    const position = idxToLogicalPoint(path.posIdx, boardWidth);
    return {
        position,
        isCapture: path.isCapture,
    };
}

export function decodeEncodedMovesIntoMap({
    encoded,
    boardWidth,
    hasForcedMoves,
}: {
    encoded: string;
    boardWidth: number;
    hasForcedMoves: boolean;
}): LegalMoves {
    const buffer = Buffer.from(encoded, "base64");
    const decompressed = brotliDecompress(buffer);
    const decoded = new TextDecoder().decode(decompressed);
    const moves = decodeMovePathIntoLegalMoves({
        paths: JSON.parse(decoded),
        boardWidth,
        hasForcedMoves,
    });
    return moves;
}
