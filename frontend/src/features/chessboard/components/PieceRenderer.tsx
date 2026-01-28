import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import ChessPiece from "./ChessPiece";
import RemovingChessPiece from "./RemovingChessPiece";

const PieceRenderer = () => {
    const pieceIds = useChessboardStore((x) =>
        Array.from(x.animatingPieces?.keys() ?? x.pieces.keys()),
    );
    const removingPiecesIds = useChessboardStore((x) =>
        Array.from(x.removingPieces.keys()),
    );

    return (
        <>
            {pieceIds.map((pieceId) => (
                <ChessPiece id={pieceId} key={pieceId} />
            ))}
            {removingPiecesIds.map((pieceId) => (
                <RemovingChessPiece id={pieceId} key={pieceId} />
            ))}
        </>
    );
};
export default PieceRenderer;
