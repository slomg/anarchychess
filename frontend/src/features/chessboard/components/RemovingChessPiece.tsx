import { useChessboardStore } from "../hooks/useChessboard";
import getPieceImage from "../lib/pieceImage";
import { PieceID } from "../lib/types";
import ChessSquare from "./ChessSquare";

const RemovingChessPiece = ({ id }: { id: PieceID }) => {
    const piece = useChessboardStore((x) => x.removingPieces.get(id));
    if (!piece) return null;

    return (
        <ChessSquare
            data-testid="removingPiece"
            position={piece.position}
            className="bg-size-[100%] bg-no-repeat opacity-50
                select-none"
            style={{
                backgroundImage: `url("${getPieceImage(piece.type, piece.color)}")`,
            }}
        />
    );
};
export default RemovingChessPiece;
