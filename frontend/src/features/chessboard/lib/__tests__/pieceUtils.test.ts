import { GameColor, PieceType } from "@/lib/apiClient";
import {
    getMaterialPieceImage,
    getPieceImage,
    pieceTypeToStr,
} from "../pieceUtils";

describe("pieceTypeToStr", () => {
    it.each([
        [PieceType.PAWN, "Pawn"],
        [PieceType.HORSEY, "Horsey"],
        [PieceType.TRAITOR_ROOK, "Traitor Rook"],
        [PieceType.UNDERAGE_PAWN, "Underage Pawn"],
    ])("should convert piece type to display string", (pieceType, expected) => {
        expect(pieceTypeToStr(pieceType)).toBe(expected);
    });
});

describe("getPieceImage", () => {
    it.each([
        [
            PieceType.PAWN,
            GameColor.WHITE,
            `${process.env.NEXT_PUBLIC_ASSETS_URL}/pieces/pawn_white.png`,
        ],
        [
            PieceType.KING,
            GameColor.BLACK,
            `${process.env.NEXT_PUBLIC_ASSETS_URL}/pieces/king_black.png`,
        ],
        [
            PieceType.TRAITOR_ROOK,
            null,
            `${process.env.NEXT_PUBLIC_ASSETS_URL}/pieces/traitor_rook_neutral.png`,
        ],
    ])("should return correct piece image path", (type, color, expected) => {
        expect(getPieceImage(type, color)).toBe(expected);
    });
});

describe("getMaterialPieceImage", () => {
    it.each([
        [
            PieceType.PAWN,
            GameColor.WHITE,
            `${process.env.NEXT_PUBLIC_ASSETS_URL}/material-pieces/pawn_white.png`,
        ],
        [
            PieceType.KING,
            GameColor.BLACK,
            `${process.env.NEXT_PUBLIC_ASSETS_URL}/material-pieces/king_black.png`,
        ],
        [
            PieceType.TRAITOR_ROOK,
            null,
            `${process.env.NEXT_PUBLIC_ASSETS_URL}/material-pieces/traitor_rook_neutral.png`,
        ],
    ])(
        "should return correct material piece image path",
        (type, color, expected) => {
            expect(getMaterialPieceImage(type, color)).toBe(expected);
        },
    );
});
