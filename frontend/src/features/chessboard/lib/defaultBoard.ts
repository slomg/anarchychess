import { logicalPoint } from "@/features/point/pointUtils";
import { GameColor, PieceType } from "@/lib/apiClient";
import BoardPieces from "./boardPieces";

export default function createDefaultChessboard() {
    // prettier-ignore
    return BoardPieces.fromPieces(
        { position: logicalPoint({ x: 0, y: 0 }), type: PieceType.ROOK, color: GameColor.WHITE, stunnedForTurns: 0, id: "0", },
        { position: logicalPoint({ x: 1, y: 0 }), type: PieceType.HORSEY, color: GameColor.WHITE, stunnedForTurns: 0, id: "1", },
        { position: logicalPoint({ x: 2, y: 0 }), type: PieceType.KNOOK, color: GameColor.WHITE, stunnedForTurns: 0, id: "2", },
        { position: logicalPoint({ x: 3, y: 0 }), type: PieceType.BISHOP, color: GameColor.WHITE, stunnedForTurns: 0, id: "3", },
        { position: logicalPoint({ x: 4, y: 0 }), type: PieceType.QUEEN, color: GameColor.WHITE, stunnedForTurns: 0, id: "4", },
        { position: logicalPoint({ x: 5, y: 0 }), type: PieceType.KING, color: GameColor.WHITE, stunnedForTurns: 0, id: "5", },
        { position: logicalPoint({ x: 6, y: 0 }), type: PieceType.BISHOP, color: GameColor.WHITE, stunnedForTurns: 0, id: "6", },
        { position: logicalPoint({ x: 7, y: 0 }), type: PieceType.CHECKER, color: GameColor.WHITE, stunnedForTurns: 0, id: "7", },
        { position: logicalPoint({ x: 8, y: 0 }), type: PieceType.ANTIQUEEN, color: GameColor.WHITE, stunnedForTurns: 0, id: "8", },
        { position: logicalPoint({ x: 9, y: 0 }), type: PieceType.ROOK, color: GameColor.WHITE, stunnedForTurns: 0, id: "9", },

        { position: logicalPoint({ x: 0, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, id: "10", },
        { position: logicalPoint({ x: 1, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, id: "11", },
        { position: logicalPoint({ x: 2, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, id: "12", },
        { position: logicalPoint({ x: 3, y: 1 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.WHITE, stunnedForTurns: 0, id: "13", },
        { position: logicalPoint({ x: 4, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, id: "14", },
        { position: logicalPoint({ x: 5, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, id: "15", },
        { position: logicalPoint({ x: 6, y: 1 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.WHITE, stunnedForTurns: 0, id: "16", },
        { position: logicalPoint({ x: 7, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, id: "17", },
        { position: logicalPoint({ x: 8, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, id: "18", },
        { position: logicalPoint({ x: 9, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, id: "19", },

        { position: logicalPoint({ x: 9, y: 3 }), type: PieceType.TRAITOR_ROOK, color: null, stunnedForTurns: 0, id: "20", },
        { position: logicalPoint({ x: 0, y: 6 }), type: PieceType.TRAITOR_ROOK, color: null, stunnedForTurns: 0, id: "21", },

        { position: logicalPoint({ x: 0, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, id: "22", },
        { position: logicalPoint({ x: 1, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, id: "23", },
        { position: logicalPoint({ x: 2, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, id: "24", },
        { position: logicalPoint({ x: 3, y: 8 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.BLACK, stunnedForTurns: 0, id: "25", },
        { position: logicalPoint({ x: 4, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, id: "26", },
        { position: logicalPoint({ x: 5, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, id: "27", },
        { position: logicalPoint({ x: 6, y: 8 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.BLACK, stunnedForTurns: 0, id: "28", },
        { position: logicalPoint({ x: 7, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, id: "29", },
        { position: logicalPoint({ x: 8, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, id: "30", },
        { position: logicalPoint({ x: 9, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, id: "31", },

        { position: logicalPoint({ x: 0, y: 9 }), type: PieceType.ROOK, color: GameColor.BLACK, stunnedForTurns: 0, id: "32", },
        { position: logicalPoint({ x: 1, y: 9 }), type: PieceType.HORSEY, color: GameColor.BLACK, stunnedForTurns: 0, id: "33", },
        { position: logicalPoint({ x: 2, y: 9 }), type: PieceType.KNOOK, color: GameColor.BLACK, stunnedForTurns: 0, id: "34", },
        { position: logicalPoint({ x: 3, y: 9 }), type: PieceType.BISHOP, color: GameColor.BLACK, stunnedForTurns: 0, id: "35", },
        { position: logicalPoint({ x: 4, y: 9 }), type: PieceType.QUEEN, color: GameColor.BLACK, stunnedForTurns: 0, id: "36", },
        { position: logicalPoint({ x: 5, y: 9 }), type: PieceType.KING, color: GameColor.BLACK, stunnedForTurns: 0, id: "37", },
        { position: logicalPoint({ x: 6, y: 9 }), type: PieceType.BISHOP, color: GameColor.BLACK, stunnedForTurns: 0, id: "38", },
        { position: logicalPoint({ x: 7, y: 9 }), type: PieceType.CHECKER, color: GameColor.BLACK, stunnedForTurns: 0, id: "39", },
        { position: logicalPoint({ x: 8, y: 9 }), type: PieceType.ANTIQUEEN, color: GameColor.BLACK, stunnedForTurns: 0, id: "40", },
        { position: logicalPoint({ x: 9, y: 9 }), type: PieceType.ROOK, color: GameColor.BLACK, stunnedForTurns: 0, id: "41", },
    );
}
