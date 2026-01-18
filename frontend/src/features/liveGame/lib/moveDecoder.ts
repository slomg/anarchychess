import brotliDecompress from "brotli/decompress";

import {
    IntermediateSquare,
    MoveKey,
    MoveSideEffect,
    Piece,
} from "@/features/chessboard/lib/types";
import { Move } from "@/features/chessboard/lib/types";
import {
    ForcedMovePriority,
    IntermediateSquarePath,
    MovePath,
    MoveSideEffectPath,
    PieceSpawnPath,
    SpecialMoveType,
} from "@/lib/apiClient";
import { idxToLogicalPoint, pointToStr } from "@/features/point/pointUtils";
import { createPieceId } from "@/features/chessboard/lib/pieceUtils";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { LogicalPoint, StrPoint } from "@/features/point/types";

export function decodeMovePathIntoLegalMoves({
    paths,
    boardWidth,
}: {
    paths: MovePath[];
    boardWidth: number;
}): LegalMoves {
    const moves = new Map<StrPoint, Move[]>();
    const highlightSquares: LogicalPoint[] = [];
    let hasForcedMoves = false;
    for (const path of paths) {
        const move = decodeMovePath(path, boardWidth);
        if (move.forcedPriority != ForcedMovePriority.NONE) {
            hasForcedMoves = true;
        }
        if (move.highlightSquare) {
            highlightSquares.push(move.from);
        }

        const fromString = pointToStr(move.from);
        const movesFromPoint = moves.get(fromString) ?? [];
        movesFromPoint.push(move);

        moves.set(pointToStr(move.from), movesFromPoint);
    }

    return new LegalMoves(moves, hasForcedMoves, highlightSquares);
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
        moveKey: path.moveKey as MoveKey,
        triggers,
        captures,
        intermediates,
        sideEffects,
        pieceSpawns,
        specialType: path.specialType ?? SpecialMoveType.NONE,
        forcedPriority: path.forcedPriority ?? ForcedMovePriority.NONE,
        promotesTo: path.promotesTo ?? null,
        highlightSquare: path.highlightSquare ?? false,
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

export function decodeLegalMoves({
    encoded,
    boardWidth,
}: {
    encoded: string;
    boardWidth: number;
}): LegalMoves {
    if (encoded.length === 0) {
        return new LegalMoves();
    }

    const buffer = Buffer.from(encoded, "base64");
    const decompressed = brotliDecompress(buffer);
    const decoded = new TextDecoder().decode(decompressed);
    const moves = decodeMovePathIntoLegalMoves({
        paths: JSON.parse(decoded),
        boardWidth,
    });
    return moves;
}
