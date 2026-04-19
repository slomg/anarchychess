import Image from "next/image";

import { useChessboardStore } from "../hooks/useChessboard";
import { getMaterialPieceImage } from "../lib/pieceImage";
import { GameColor, PieceType } from "@/lib/apiClient";
import { invertColor } from "@/lib/utils/chessUtils";

const MATERIAL_VALUE: Record<PieceType, number> = {
    [PieceType.QUEEN]: 10,
    [PieceType.ROOK]: 5,
    [PieceType.KNOOK]: 4,
    [PieceType.KING]: 3,
    [PieceType.CHECKER]: 3,
    [PieceType.BISHOP]: 3,
    [PieceType.HORSEY]: 3,
    [PieceType.ANTIQUEEN]: 3,
    [PieceType.UNDERAGE_PAWN]: 1,
    [PieceType.PAWN]: 1,
    [PieceType.STERILE_PAWN]: 0.8,
    [PieceType.TRAITOR_ROOK]: 0,
};

// piexel height of each piece without things that are hard to see
const MATERIAL_HEIGHT: Record<PieceType, number> = {
    [PieceType.KNOOK]: 123,
    [PieceType.HORSEY]: 120,
    [PieceType.KING]: 115,
    [PieceType.ROOK]: 116,
    [PieceType.CHECKER]: 110,
    [PieceType.BISHOP]: 97,
    [PieceType.STERILE_PAWN]: 67,
    [PieceType.PAWN]: 67,
    [PieceType.ANTIQUEEN]: 64,
    [PieceType.QUEEN]: 63,
    [PieceType.UNDERAGE_PAWN]: 24,
    [PieceType.TRAITOR_ROOK]: 0,
};

const MaterialCount = ({ playerColor }: { playerColor: GameColor }) => {
    const opponentColor = invertColor(playerColor);

    const pieces = useChessboardStore((x) => x.pieces);

    let totalValue = 0;
    const pieceBalance = new Map<PieceType, number>();
    for (const piece of pieces) {
        const prevCount = pieceBalance.get(piece.type) ?? 0;
        const value = MATERIAL_VALUE[piece.type];
        if (piece.color === playerColor) {
            totalValue += value;
            pieceBalance.set(piece.type, prevCount + 1);
        } else if (piece.color === opponentColor) {
            totalValue -= value;
            pieceBalance.set(piece.type, prevCount - 1);
        }
    }
    const sortedPieceBalance = [...pieceBalance.entries()].sort(
        ([a], [b]) => MATERIAL_HEIGHT[a] - MATERIAL_HEIGHT[b],
    );

    return (
        <div className="text-text/70 flex h-5 items-center overflow-auto">
            {sortedPieceBalance.map(([piece, balance]) => {
                if (balance <= 0) {
                    return null;
                }

                return Array.from({ length: balance }, (_, i) => (
                    <Image
                        key={`${piece}-${i}`}
                        alt="Material Piece"
                        width={2}
                        height={2}
                        className="mr-0.5 h-5 w-auto bg-contain bg-no-repeat"
                        src={getMaterialPieceImage(piece, opponentColor)}
                        data-testid={`materialCount-${piece}`}
                    />
                ));
            })}

            {totalValue > 0 && (
                // mt-1 because I have no idea why items-center doesn't center this text
                <span
                    className="mt-1 ml-1 text-sm"
                    data-testid="materialCountTotalValue"
                >
                    +{totalValue}
                </span>
            )}
        </div>
    );
};
export default MaterialCount;
