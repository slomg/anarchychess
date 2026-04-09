import { logicalPoint } from "@/features/point/pointUtils";
import { GameColor, PieceType } from "@/lib/apiClient";
import BoardPieces from "./boardPieces";

export default function createDefaultChessboard() {
    // prettier-ignore
    return BoardPieces.fromPieces(
        { position: logicalPoint({ x: 0, y: 0 }), type: PieceType.ROOK, color: GameColor.WHITE, id: "0", },
        { position: logicalPoint({ x: 1, y: 0 }), type: PieceType.HORSEY, color: GameColor.WHITE, id: "1", },
        { position: logicalPoint({ x: 2, y: 0 }), type: PieceType.KNOOK, color: GameColor.WHITE, id: "2", },
        { position: logicalPoint({ x: 3, y: 0 }), type: PieceType.BISHOP, color: GameColor.WHITE, id: "3", },
        { position: logicalPoint({ x: 4, y: 0 }), type: PieceType.QUEEN, color: GameColor.WHITE, id: "4", },
        { position: logicalPoint({ x: 5, y: 0 }), type: PieceType.KING, color: GameColor.WHITE, id: "5", },
        { position: logicalPoint({ x: 6, y: 0 }), type: PieceType.BISHOP, color: GameColor.WHITE, id: "6", },
        { position: logicalPoint({ x: 7, y: 0 }), type: PieceType.CHECKER, color: GameColor.WHITE, id: "7", },
        { position: logicalPoint({ x: 8, y: 0 }), type: PieceType.ANTIQUEEN, color: GameColor.WHITE, id: "8", },
        { position: logicalPoint({ x: 9, y: 0 }), type: PieceType.ROOK, color: GameColor.WHITE, id: "9", },

        { position: logicalPoint({ x: 0, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "10", },
        { position: logicalPoint({ x: 1, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "11", },
        { position: logicalPoint({ x: 2, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "12", },
        { position: logicalPoint({ x: 3, y: 1 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.WHITE, id: "13", },
        { position: logicalPoint({ x: 4, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "14", },
        { position: logicalPoint({ x: 5, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "15", },
        { position: logicalPoint({ x: 6, y: 1 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.WHITE, id: "16", },
        { position: logicalPoint({ x: 7, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "17", },
        { position: logicalPoint({ x: 8, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "18", },
        { position: logicalPoint({ x: 9, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "19", },

        { position: logicalPoint({ x: 9, y: 3 }), type: PieceType.TRAITOR_ROOK, color: null, id: "20", },
        { position: logicalPoint({ x: 0, y: 6 }), type: PieceType.TRAITOR_ROOK, color: null, id: "21", },

        { position: logicalPoint({ x: 0, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "22", },
        { position: logicalPoint({ x: 1, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "23", },
        { position: logicalPoint({ x: 2, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "24", },
        { position: logicalPoint({ x: 3, y: 8 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.BLACK, id: "25", },
        { position: logicalPoint({ x: 4, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "26", },
        { position: logicalPoint({ x: 5, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "27", },
        { position: logicalPoint({ x: 6, y: 8 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.BLACK, id: "28", },
        { position: logicalPoint({ x: 7, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "29", },
        { position: logicalPoint({ x: 8, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "30", },
        { position: logicalPoint({ x: 9, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "31", },

        { position: logicalPoint({ x: 0, y: 9 }), type: PieceType.ROOK, color: GameColor.BLACK, id: "32", },
        { position: logicalPoint({ x: 1, y: 9 }), type: PieceType.HORSEY, color: GameColor.BLACK, id: "33", },
        { position: logicalPoint({ x: 2, y: 9 }), type: PieceType.KNOOK, color: GameColor.BLACK, id: "34", },
        { position: logicalPoint({ x: 3, y: 9 }), type: PieceType.BISHOP, color: GameColor.BLACK, id: "35", },
        { position: logicalPoint({ x: 4, y: 9 }), type: PieceType.QUEEN, color: GameColor.BLACK, id: "36", },
        { position: logicalPoint({ x: 5, y: 9 }), type: PieceType.KING, color: GameColor.BLACK, id: "37", },
        { position: logicalPoint({ x: 6, y: 9 }), type: PieceType.BISHOP, color: GameColor.BLACK, id: "38", },
        { position: logicalPoint({ x: 7, y: 9 }), type: PieceType.CHECKER, color: GameColor.BLACK, id: "39", },
        { position: logicalPoint({ x: 8, y: 9 }), type: PieceType.ANTIQUEEN, color: GameColor.BLACK, id: "40", },
        { position: logicalPoint({ x: 9, y: 9 }), type: PieceType.ROOK, color: GameColor.BLACK, id: "41", },
    );
}
