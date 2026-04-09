import brotliDecompress from "brotli/decompress";

import {
    IntermediateSquare,
    Move,
    MoveKey,
    MoveSideEffect,
    MoveStun,
    Piece,
} from "@/features/chessboard/lib/types";
import {
    ForcedMovePriority,
    IntermediateSquarePath,
    MovePath,
    MoveSideEffectPath,
    MoveStunPath,
    PieceSpawnPath,
    SpecialMoveType,
} from "@/lib/apiClient";

import { idxToLogicalPoint } from "@/features/point/pointUtils";
import { createPieceId } from "@/features/chessboard/lib/pieceUtils";
import LegalMoves from "@/features/chessboard/lib/legalMoves";

export function decodeMovePathIntoLegalMoves({
    paths,
    boardWidth,
}: {
    paths: MovePath[];
    boardWidth: number;
}): LegalMoves {
    const legalMoves = new LegalMoves();
    for (const path of paths) {
        const move = decodeMovePath(path, boardWidth);
        legalMoves.addMove(move);
    }

    return legalMoves;
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
    const stuns = path.stuns?.map((x) => parseStuns(x, boardWidth)) ?? [];

    const overtimeRemovals =
        path.overtimeRemovalIdxs?.map((idx) =>
            idxToLogicalPoint(idx, boardWidth),
        ) ?? [];

    return {
        from,
        to,
        moveKey: path.moveKey as MoveKey,
        triggers,
        captures,
        intermediates,
        sideEffects,
        pieceSpawns,
        stuns,
        specialType: path.specialType ?? SpecialMoveType.NONE,
        forcedPriority: path.forcedPriority ?? ForcedMovePriority.NONE,
        promotesTo: path.promotesTo ?? null,
        emphasizeSquare: path.emphasizeSquare ?? false,
        overtimeRemovals,
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
        stunnedForTurns: 0,
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

function parseStuns(path: MoveStunPath, boardWidth: number): MoveStun {
    const position = idxToLogicalPoint(path.posIdx, boardWidth);
    return {
        position,
        stunForTurns: path.stunForTurns,
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
