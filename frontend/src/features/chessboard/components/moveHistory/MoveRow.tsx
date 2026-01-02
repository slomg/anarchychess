import clsx from "clsx";

import { useChessboardStore } from "../../hooks/useChessboard";
import { Position } from "../../lib/position";

const MoveRow = ({
    whitePosition,
    blackPosition,
}: {
    whitePosition: Position;
    blackPosition?: Position;
}) => {
    const { goToPosition, isViewingWhite, isViewingBlack } = useChessboardStore(
        (x) => ({
            goToPosition: x.goToPosition,
            isViewingWhite:
                whitePosition &&
                x.positionHistory.viewingPosition?.positionId ===
                    whitePosition.positionId,
            isViewingBlack:
                blackPosition &&
                x.positionHistory.viewingPosition?.positionId ===
                    blackPosition.positionId,
        }),
    );

    const moveNumber = whitePosition.ply / 2 + 1;
    const color = moveNumber % 2 === 0 ? "bg-white/10" : "";
    const selectedClass = "bg-blue-300/30";
    return (
        <div className={clsx("flex", color)} data-testid="moveRow">
            <div className="bg-card w-10 p-3">{moveNumber}.</div>
            <button
                className={clsx(
                    "flex-1 cursor-pointer overflow-x-auto p-3 text-start",
                    isViewingWhite && selectedClass,
                )}
                onClick={() =>
                    whitePosition && goToPosition(whitePosition.positionId)
                }
            >
                {whitePosition?.san}
            </button>

            <button
                className={clsx(
                    "flex-1 cursor-pointer overflow-x-auto p-3 text-start",
                    isViewingBlack && selectedClass,
                )}
                onClick={() =>
                    blackPosition && goToPosition(blackPosition.positionId)
                }
            >
                {blackPosition?.san}
            </button>
        </div>
    );
};
export default MoveRow;
