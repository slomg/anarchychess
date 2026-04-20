import BoardPieces from "./boardPieces";
import { Piece } from "./types";
import { GameColor, PieceType } from "@/lib/apiClient";
import { logicalPoint } from "@/features/point/pointUtils";

type PieceColorResolver = (
    piece: Piece,
    pieces: BoardPieces,
) => GameColor | null;

const pieceColorResolvers: Partial<Record<PieceType, PieceColorResolver>> = {
    [PieceType.TRAITOR_ROOK]: getTraitorRookOwnership,
};

export default function getEffectivePieceColor(
    piece: Piece,
    pieces: BoardPieces,
): GameColor | null {
    const resolver = pieceColorResolvers[piece.type];
    return resolver?.(piece, pieces) ?? piece.color;
}

function getTraitorRookOwnership(
    piece: Piece,
    pieces: BoardPieces,
): GameColor | null {
    let whitePieces = 0;
    let blackPieces = 0;
    for (let x = -1; x <= 1; x++) {
        for (let y = -1; y <= 1; y++) {
            if (x == 0 && y == 0) {
                continue;
            }

            const targetPosition = logicalPoint({
                x: piece.position.x + x,
                y: piece.position.y + y,
            });
            const targetPiece = pieces.getByPosition(targetPosition);
            if (!targetPiece || targetPiece.stunnedForTurns > 0) {
                continue;
            }

            if (targetPiece.color === GameColor.WHITE) {
                whitePieces++;
            } else if (targetPiece.color === GameColor.BLACK) {
                blackPieces++;
            }
        }
    }

    if (whitePieces > blackPieces) {
        return GameColor.WHITE;
    } else if (blackPieces > whitePieces) {
        return GameColor.BLACK;
    } else {
        return null;
    }
}
