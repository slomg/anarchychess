import { GameColor, PieceType } from "@/lib/apiClient";

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
