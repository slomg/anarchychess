import { logicalPoint } from "@/features/point/pointUtils";
import { GameColor, PieceType } from "@/lib/apiClient";
import BoardPieces from "./boardPieces";

export default function createDefaultChessboard() {
    // prettier-ignore
    return BoardPieces.fromPieces(
        { position: logicalPoint({ x: 0, y: 0 }), type: PieceType.ROOK, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "0", },
        { position: logicalPoint({ x: 1, y: 0 }), type: PieceType.HORSEY, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "1", },
        { position: logicalPoint({ x: 2, y: 0 }), type: PieceType.KNOOK, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "2", },
        { position: logicalPoint({ x: 3, y: 0 }), type: PieceType.BISHOP, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "3", },
        { position: logicalPoint({ x: 4, y: 0 }), type: PieceType.QUEEN, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "4", },
        { position: logicalPoint({ x: 5, y: 0 }), type: PieceType.KING, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "5", },
        { position: logicalPoint({ x: 6, y: 0 }), type: PieceType.BISHOP, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "6", },
        { position: logicalPoint({ x: 7, y: 0 }), type: PieceType.CHECKER, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "7", },
        { position: logicalPoint({ x: 8, y: 0 }), type: PieceType.ANTIQUEEN, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "8", },
        { position: logicalPoint({ x: 9, y: 0 }), type: PieceType.ROOK, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "9", },

        { position: logicalPoint({ x: 0, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "10", },
        { position: logicalPoint({ x: 1, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "11", },
        { position: logicalPoint({ x: 2, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "12", },
        { position: logicalPoint({ x: 3, y: 1 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "13", },
        { position: logicalPoint({ x: 4, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "14", },
        { position: logicalPoint({ x: 5, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "15", },
        { position: logicalPoint({ x: 6, y: 1 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "16", },
        { position: logicalPoint({ x: 7, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "17", },
        { position: logicalPoint({ x: 8, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "18", },
        { position: logicalPoint({ x: 9, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, stunnedForTurns: 0, hasMoved: false, id: "19", },

        { position: logicalPoint({ x: 9, y: 3 }), type: PieceType.TRAITOR_ROOK, color: null, stunnedForTurns: 0, hasMoved: false, id: "20", },
        { position: logicalPoint({ x: 0, y: 6 }), type: PieceType.TRAITOR_ROOK, color: null, stunnedForTurns: 0, hasMoved: false, id: "21", },

        { position: logicalPoint({ x: 0, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "22", },
        { position: logicalPoint({ x: 1, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "23", },
        { position: logicalPoint({ x: 2, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "24", },
        { position: logicalPoint({ x: 3, y: 8 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "25", },
        { position: logicalPoint({ x: 4, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "26", },
        { position: logicalPoint({ x: 5, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "27", },
        { position: logicalPoint({ x: 6, y: 8 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "28", },
        { position: logicalPoint({ x: 7, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "29", },
        { position: logicalPoint({ x: 8, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "30", },
        { position: logicalPoint({ x: 9, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "31", },

        { position: logicalPoint({ x: 0, y: 9 }), type: PieceType.ROOK, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "32", },
        { position: logicalPoint({ x: 1, y: 9 }), type: PieceType.HORSEY, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "33", },
        { position: logicalPoint({ x: 2, y: 9 }), type: PieceType.KNOOK, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "34", },
        { position: logicalPoint({ x: 3, y: 9 }), type: PieceType.BISHOP, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "35", },
        { position: logicalPoint({ x: 4, y: 9 }), type: PieceType.QUEEN, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "36", },
        { position: logicalPoint({ x: 5, y: 9 }), type: PieceType.KING, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "37", },
        { position: logicalPoint({ x: 6, y: 9 }), type: PieceType.BISHOP, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "38", },
        { position: logicalPoint({ x: 7, y: 9 }), type: PieceType.CHECKER, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "39", },
        { position: logicalPoint({ x: 8, y: 9 }), type: PieceType.ANTIQUEEN, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "40", },
        { position: logicalPoint({ x: 9, y: 9 }), type: PieceType.ROOK, color: GameColor.BLACK, stunnedForTurns: 0, hasMoved: false, id: "41", },
    );
}
