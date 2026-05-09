import { useRef } from "react";
import clsx from "clsx";

import { useChessboardStore } from "../../hooks/useChessboard";
import useHorizontalScroll from "@/hooks/useHorizontalScroll";
import { Position } from "../../lib/position";

const MoveRow = ({
    ply,
    whitePosition,
    blackPosition,
}: {
    ply: number;
    whitePosition?: Position;
    blackPosition?: Position;
}) => {
    const whiteMoveRef = useRef<HTMLButtonElement>(null);
    const blackMoveRef = useRef<HTMLButtonElement>(null);

    useHorizontalScroll(whiteMoveRef);
    useHorizontalScroll(blackMoveRef);

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

    const moveNumber = Math.ceil(ply / 2);
    const color = moveNumber % 2 === 0 ? "bg-white/10" : "";
    const selectedClass = "bg-blue-300/30";
    return (
        <div className={clsx("flex text-nowrap", color)} data-testid="moveRow">
            <div className="bg-card w-10 p-3">{moveNumber}.</div>
            <button
                ref={whiteMoveRef}
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
                ref={blackMoveRef}
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
