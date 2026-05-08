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

import { createPieceId } from "@/features/chessboard/lib/pieceUtils";
import { idxToLogicalPoint } from "@/features/point/pointUtils";
import LegalMoves from "@/features/chessboard/lib/legalMoves";

export function decodeMovePathIntoLegalMoves(paths: MovePath[]): LegalMoves {
    const legalMoves = new LegalMoves();
    for (const path of paths) {
        const move = decodeMovePath(path);
        legalMoves.addMove(move);
    }

    return legalMoves;
}

export function decodeMovePath(path: MovePath): Move {
    const from = idxToLogicalPoint(path.fromIdx);
    const to = idxToLogicalPoint(path.toIdx);
    const triggers =
        path.triggerIdxs?.map((idx) => idxToLogicalPoint(idx)) ?? [];
    const captures =
        path.capturedIdxs?.map((idx) => idxToLogicalPoint(idx)) ?? [];
    const intermediates =
        path.intermediateSquares?.map((x) => parseIntermediateSquares(x)) ?? [];
    const sideEffects = path.sideEffects?.map((x) => parseSideEffect(x)) ?? [];
    const pieceSpawns = path.pieceSpawns?.map((x) => parsePieceSpawns(x)) ?? [];
    const stuns = path.stuns?.map((x) => parseStuns(x)) ?? [];

    const overtimeRemovals =
        path.overtimeRemovalIdxs?.map((idx) => idxToLogicalPoint(idx)) ?? [];

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

function parseSideEffect(path: MoveSideEffectPath): MoveSideEffect {
    const from = idxToLogicalPoint(path.fromIdx);
    const to = idxToLogicalPoint(path.toIdx);
    return {
        from,
        to,
    };
}

function parsePieceSpawns(path: PieceSpawnPath): Piece {
    const position = idxToLogicalPoint(path.posIdx);
    return {
        id: createPieceId(),
        type: path.type,
        color: path.color ?? null,
        position,
        stunnedForTurns: 0,
        hasMoved: false,
    };
}

function parseIntermediateSquares(
    path: IntermediateSquarePath,
): IntermediateSquare {
    const position = idxToLogicalPoint(path.posIdx);
    return {
        position,
        isCapture: path.isCapture,
    };
}

function parseStuns(path: MoveStunPath): MoveStun {
    const position = idxToLogicalPoint(path.posIdx);
    return {
        position,
        stunForTurns: path.stunForTurns,
    };
}

export function decodeLegalMoves(encoded: string): LegalMoves {
    if (encoded.length === 0) {
        return new LegalMoves();
    }

    const buffer = Buffer.from(encoded, "base64");
    const decompressed = brotliDecompress(buffer);
    const decoded = new TextDecoder().decode(decompressed);
    const moves = decodeMovePathIntoLegalMoves(JSON.parse(decoded));
    return moves;
}
