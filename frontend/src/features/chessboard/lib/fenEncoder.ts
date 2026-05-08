import { logicalPoint, logicalToAlgebraic } from "@/features/point/pointUtils";
import { AlgebraicString, LogicalPoint } from "@/features/point/types";
import { GameColor } from "@/lib/apiClient";
import BoardPieces from "./boardPieces";
import constants from "@/lib/constants";
import { FenParts } from "./types";

export function encodeFen({
    pieces,
    sideToMove,
}: {
    pieces: BoardPieces;
    sideToMove: GameColor;
}): string {
    let fen = encodePieces(pieces);

    const fenParts = getFenParts({ pieces, sideToMove });
    if (fenParts !== null) {
        fen += " " + fenParts;
    }

    return fen;
}

function encodePieces(board: BoardPieces): string {
    let result = "";
    for (let y = constants.BOARD_HEIGHT - 1; y >= 0; y--) {
        let emptyCount = 0;
        for (let x = 0; x < constants.BOARD_WIDTH; x++) {
            const point: LogicalPoint = logicalPoint({ x, y });
            const piece = board.getByPosition(point);
            if (!piece) {
                emptyCount++;
                continue;
            }

            if (emptyCount > 0) {
                result += emptyCount;
                emptyCount = 0;
            }

            const pieceLetter = getLetterForColor(
                constants.PIECE_TO_LETTER[piece.type],
                piece.color,
            );

            result += pieceLetter;
        }

        if (emptyCount > 0) {
            result += emptyCount;
        }
        if (y > 0) {
            result += "/";
        }
    }

    return result;
}

function getLetterForColor(letter: string, color: GameColor | null): string {
    if (color === GameColor.WHITE) {
        return letter.toUpperCase();
    } else if (color === GameColor.BLACK) {
        return letter.toLowerCase();
    } else {
        return letter;
    }
}

function getFenParts({
    pieces,
    sideToMove,
}: {
    pieces: BoardPieces;
    sideToMove: GameColor;
}): string | null {
    const movedPieces: AlgebraicString[] = [];
    const stunnedPieces: Record<AlgebraicString, number> = {};
    for (const piece of pieces) {
        if (piece.hasMoved) {
            movedPieces.push(logicalToAlgebraic(piece.position));
        }
        if (piece.stunnedForTurns > 0) {
            stunnedPieces[logicalToAlgebraic(piece.position)] =
                piece.stunnedForTurns;
        }
    }

    const fenParts: FenParts = {
        sideToMove: sideToMove !== GameColor.WHITE ? sideToMove : undefined,
        movedPieces: movedPieces.length > 0 ? movedPieces : undefined,
        stunnedPieces:
            Object.keys(stunnedPieces).length > 0 ? stunnedPieces : undefined,
    };
    const serialized = JSON.stringify(fenParts);
    if (serialized === "{}") {
        return null;
    }

    return serialized;
}
