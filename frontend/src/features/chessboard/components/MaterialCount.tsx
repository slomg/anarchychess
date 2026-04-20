import Image from "next/image";

import getEffectivePieceColor from "../lib/effectivePieceColor";
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
    [PieceType.TRAITOR_ROOK]: 2,
    [PieceType.UNDERAGE_PAWN]: 1,
    [PieceType.PAWN]: 1,
    [PieceType.STERILE_PAWN]: 0.8,
};

// piexel height of each piece without things that are hard to see
const MATERIAL_HEIGHT: Record<PieceType, number> = {
    [PieceType.HORSEY]: 123,
    [PieceType.KNOOK]: 120,
    [PieceType.ANTIQUEEN]: 120,
    [PieceType.ROOK]: 117,
    [PieceType.TRAITOR_ROOK]: 117,
    [PieceType.BISHOP]: 116,
    [PieceType.KING]: 115,
    [PieceType.QUEEN]: 110,
    [PieceType.CHECKER]: 110,
    [PieceType.STERILE_PAWN]: 67,
    [PieceType.PAWN]: 67,
    [PieceType.UNDERAGE_PAWN]: 24,
};

interface PieceBalance {
    balance: number;
    color: GameColor | null;
}

const MaterialCount = ({ playerColor }: { playerColor: GameColor }) => {
    const opponentColor = invertColor(playerColor);

    const pieces = useChessboardStore((x) => x.pieces);

    let totalValue = 0;
    const pieceBalance = new Map<PieceType, PieceBalance>();
    for (const piece of pieces) {
        const value = MATERIAL_VALUE[piece.type];
        const effectiveColor = getEffectivePieceColor(piece, pieces);

        const prevBalance: PieceBalance = pieceBalance.get(piece.type) ?? {
            balance: 0,
            color: piece.color === null ? null : opponentColor,
        };
        if (effectiveColor === playerColor) {
            totalValue += value;
            prevBalance.balance++;
        } else if (effectiveColor === opponentColor) {
            totalValue -= value;
            prevBalance.balance--;
        }

        pieceBalance.set(piece.type, prevBalance);
    }
    const sortedPieceBalance = [...pieceBalance.entries()].sort(
        ([a], [b]) => MATERIAL_HEIGHT[a] - MATERIAL_HEIGHT[b],
    );

    return (
        <div className="text-text/70 flex h-5 items-center">
            {sortedPieceBalance.map(([piece, pieceBalance]) => {
                if (pieceBalance.balance <= 0) {
                    return null;
                }

                return Array.from({ length: pieceBalance.balance }, (_, i) => (
                    <Image
                        key={`${piece}-${i}`}
                        alt="Material Piece"
                        width={64}
                        height={64}
                        className="mr-0.5 h-5 w-auto bg-contain bg-no-repeat"
                        src={getMaterialPieceImage(piece, pieceBalance.color)}
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
