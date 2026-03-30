import { LogicalPoint, StrPoint } from "@/features/point/types";
import { useChessboardStore } from "../hooks/useChessboard";
import { pointToStr } from "@/features/point/pointUtils";
import ChessSquare from "./ChessSquare";

const HighlightedLegalMovesRenderer = () => {
    const legalMoves = useChessboardStore((x) =>
        x.getViewedPositionLegalMoves(),
    );
    const pieces = useChessboardStore((x) => x.pieces);
    const selectedPieceId = useChessboardStore((x) => x.selectedPieceId);
    if (!selectedPieceId) {
        return null;
    }

    const selectedPiece = pieces.getById(selectedPieceId);
    if (!selectedPiece) {
        return null;
    }

    const moveNodes = legalMoves.getFromOrigin(selectedPiece.position);

    const toHighlightPoints = new Map<StrPoint, LogicalPoint>();
    for (const moveNode of moveNodes) {
        let allTriggers = moveNode.terminalMoves.length > 0;
        for (const move of moveNode.terminalMoves) {
            if (move.triggers.length === 0) {
                allTriggers = false;
            }

            for (const trigger of move.triggers) {
                toHighlightPoints.set(pointToStr(trigger), trigger);
            }

            if (move.intermediates.length != 0) {
                toHighlightPoints.set(
                    pointToStr(move.intermediates[0].position),
                    move.intermediates[0].position,
                );
            }
        }

        if (!allTriggers) {
            toHighlightPoints.set(pointToStr(moveNode.at), moveNode.at);
        }
    }

    return [...toHighlightPoints.values()].map((point) => (
        <HighlightedLegalMove position={point} key={pointToStr(point)} />
    ));
};
export default HighlightedLegalMovesRenderer;

const HighlightedLegalMove = ({ position }: { position: LogicalPoint }) => {
    return (
        <ChessSquare
            data-testid="highlightedLegalMove"
            className="z-20
                bg-[radial-gradient(rgba(0,0,0,0.25)_20%,rgba(0,0,0,0)_23%)]
                bg-size-[100%_100%] bg-center bg-no-repeat transition-all
                duration-100 ease-out hover:border-5 hover:border-white/50
                hover:bg-[rgba(105,105,105,0.2)]"
            position={position}
        />
    );
};
