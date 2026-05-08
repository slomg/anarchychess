import {
    algebraicToLogical,
    logicalPoint,
    pointToStr,
} from "@/features/point/pointUtils";

import { StrPoint } from "@/features/point/types";
import { createPieceId } from "./pieceUtils";
import { GameColor } from "@/lib/apiClient";
import constants from "@/lib/constants";
import BoardPieces from "./boardPieces";
import { FenParts } from "./types";

export function decodeFen(fen: string): {
    pieces: BoardPieces;
    sideToMove: GameColor;
} {
    const pieces = new BoardPieces();
    const { piecesPart, fenParts } = parseParts(fen);

    let movedPieces: Set<StrPoint> | null = null;
    let stunnedPieces: Map<StrPoint, number> | null = null;
    if (fenParts !== null) {
        movedPieces = parseMovedPieces(fenParts);
        stunnedPieces = parseStunnedPieces(fenParts);
    }

    const ranks = piecesPart.split("/").reverse();
    for (const [y, rank] of ranks.entries()) {
        // split the rank into numbers and pieces.
        // this regex makes sure multiple digits are grouped together
        const squares = rank.match(/\d+|[^0-9]/g);
        if (!squares) continue;

        let x = 0;
        for (const square of squares) {
            // if the square is a digit, skip that amount of squares
            const numSquare = Number(square);
            if (numSquare) {
                x += numSquare;
                continue;
            }

            const pieceId = createPieceId();
            const color = getColorFromLetter(square);
            const pieceLetter = square.toLowerCase();
            const pieceType = constants.LETTER_TO_PIECE[pieceLetter];

            const position = logicalPoint({ x, y });
            const positionStr = pointToStr(position);

            pieces.add({
                id: pieceId,
                position,
                type: pieceType,
                color,
                stunnedForTurns: stunnedPieces?.get(positionStr) ?? 0,
                hasMoved: movedPieces?.has(positionStr) ?? false,
            });
            x++;
        }
    }
    return { pieces, sideToMove: fenParts?.sideToMove ?? GameColor.WHITE };
}

function parseParts(fen: string): {
    piecesPart: string;
    fenParts: FenParts | null;
} {
    const [piecesPartStr, fenPartsStr] = fen.split(" ");
    let fenParts: FenParts | null;
    try {
        fenParts = JSON.parse(fenPartsStr);
    } catch {
        fenParts = null;
    }

    return {
        piecesPart: piecesPartStr,
        fenParts,
    };
}

function parseMovedPieces(fenParts: FenParts): Set<StrPoint> | null {
    if (!fenParts.movedPieces) {
        return null;
    }

    const result = new Set<StrPoint>();
    for (const algebraic of fenParts.movedPieces) {
        const point = algebraicToLogical(algebraic);
        result.add(pointToStr(point));
    }
    return result;
}

function parseStunnedPieces(fenParts: FenParts): Map<StrPoint, number> | null {
    if (!fenParts.stunnedPieces) {
        return null;
    }

    const result = new Map<StrPoint, number>();
    for (const [algebraic, forTurns] of Object.entries(
        fenParts.stunnedPieces,
    )) {
        const point = algebraicToLogical(algebraic);
        result.set(pointToStr(point), forTurns);
    }
    return result;
}

function getColorFromLetter(letter: string): GameColor | null {
    if (!isLetter(letter)) return null;
    return letter === letter.toUpperCase() ? GameColor.WHITE : GameColor.BLACK;
}

function isLetter(char: string): boolean {
    const code = char.charCodeAt(0);
    return (code >= 65 && code <= 90) || (code >= 97 && code <= 122);
}
