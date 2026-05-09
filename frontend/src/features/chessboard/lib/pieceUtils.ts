import { GameColor, PieceType } from "@/lib/apiClient";
import { PieceID } from "./types";

export function createPieceId(): PieceID {
    return crypto.randomUUID();
}

export function pieceTypeToStr(pieceType: PieceType): string {
    const name = PieceType[pieceType].toLowerCase();
    const words = name.split("_");
    return words.map((x) => x[0].toUpperCase() + x.slice(1)).join(" ");
}

export function getPieceImage(
    type: PieceType,
    color: GameColor | null,
): string {
    const pieceName = PieceType[type].toLowerCase();
    const pieceColor =
        color === null ? "neutral" : GameColor[color].toLowerCase();
    return `${process.env.NEXT_PUBLIC_ASSETS_URL}/pieces/${pieceName}_${pieceColor}.png`;
}

export function getMaterialPieceImage(
    type: PieceType,
    color: GameColor | null,
): string {
    const pieceName = PieceType[type].toLowerCase();
    const pieceColor =
        color === null ? "neutral" : GameColor[color].toLowerCase();
    return `${process.env.NEXT_PUBLIC_ASSETS_URL}/material-pieces/${pieceName}_${pieceColor}.png`;
}
