import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import ChessPiece from "./ChessPiece";

const PieceRenderer = () => {
    const pieceIds = useChessboardStore((x) =>
        Array.from(x.animatingPieces?.keys() ?? x.pieces.keys()),
    );

    return pieceIds.map((pieceId) => <ChessPiece id={pieceId} key={pieceId} />);
};
export default PieceRenderer;
