import { useChessboardStore } from "../hooks/useChessboard";
import { pointToStr } from "@/features/point/pointUtils";
import { LogicalPoint } from "@/features/point/types";
import ChessSquare from "./ChessSquare";

const EmphasizedSquaresRenderer = () => {
    const emphasizedSquares = useChessboardStore(
        (x) => x.getViewedPositionLegalMoves().emphasizedSquares,
    );

    return emphasizedSquares.map((point) => (
        <EmphasizedSquare position={point} key={pointToStr(point)} />
    ));
};
export default EmphasizedSquaresRenderer;

const EmphasizedSquare = ({ position }: { position: LogicalPoint }) => {
    return (
        <ChessSquare
            data-testid="emphasizedSquare"
            className="animate-fade-in before:animate-subtle-ping z-20
                before:absolute before:inset-0 before:border-4
                before:border-red-500 sm:before:border-6"
            position={position}
        />
    );
};
