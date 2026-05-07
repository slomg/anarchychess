import { useEffect, useRef } from "react";

import { useChessboardStore } from "../../hooks/useChessboard";
import useAutoScroll from "@/hooks/useAutoScroll";
import MoveVariation from "./MoveVariation";
import MoveRow from "./MoveRow";
import { twMerge } from "tailwind-merge";

const MoveHistoryRows = ({ className }: { className?: string }) => {
    const { totalPlyCount, positionHistory } = useChessboardStore((x) => ({
        totalPlyCount: x.positionHistory.totalPlyCount,
        positionHistory: x.positionHistory,
    }));

    const {
        stepPositionForward,
        stepPositionBackward,
        goToStartPosition,
        goToLatestPosition,
    } = useChessboardStore((x) => ({
        stepPositionForward: x.stepPositionForward,
        stepPositionBackward: x.stepPositionBackward,
        goToStartPosition: x.goToStartPosition,
        goToLatestPosition: x.goToLatestPosition,
        flipBoard: x.flipBoard,
    }));

    const tableRef = useRef<HTMLDivElement | null>(null);
    useAutoScroll(tableRef, [totalPlyCount]);

    useEffect(() => {
        async function onKeyDown(event: KeyboardEvent): Promise<void> {
            switch (event.key) {
                case "ArrowLeft":
                    await stepPositionBackward();
                    break;
                case "ArrowRight":
                    await stepPositionForward();
                    break;
                case "ArrowUp":
                    await goToStartPosition();
                    break;
                case "ArrowDown":
                    await goToLatestPosition();
                    break;
            }
        }

        window.addEventListener("keydown", onKeyDown);
        return () => window.removeEventListener("keydown", onKeyDown);
    }, [
        stepPositionBackward,
        stepPositionForward,
        goToStartPosition,
        goToLatestPosition,
    ]);

    let pendingWhiteMoveVariation: React.ReactElement | null =
        positionHistory.rootSubVariationByKey.size > 0 ? (
            <MoveVariation
                key="rootVariation"
                variations={[...positionHistory.rootSubVariationByKey.values()]}
            />
        ) : null;

    const moveRows: React.ReactElement[] = [];
    const iterator = positionHistory[Symbol.iterator]();
    while (true) {
        const white = iterator.next();
        const black = iterator.next();

        if (white.done) break;

        const whitePosition = white.value;
        const blackPosition = black.done ? undefined : black.value;
        moveRows.push(
            <MoveRow
                key={whitePosition.ply}
                whitePosition={whitePosition}
                blackPosition={blackPosition}
            />,
        );

        if (pendingWhiteMoveVariation) {
            moveRows.push(pendingWhiteMoveVariation);
            pendingWhiteMoveVariation = null;
        }

        if (whitePosition.subVariationByKey.size > 0) {
            moveRows.push(
                <MoveVariation
                    key={"variation:" + whitePosition.positionId}
                    variations={[...whitePosition.subVariationByKey.values()]}
                />,
            );
        }

        if (blackPosition && blackPosition.subVariationByKey.size > 0) {
            pendingWhiteMoveVariation = (
                <MoveVariation
                    key={blackPosition.positionId}
                    variations={[...blackPosition.subVariationByKey.values()]}
                />
            );
        }
    }
    if (pendingWhiteMoveVariation) {
        moveRows.push(pendingWhiteMoveVariation);
    }

    return (
        <div
            className={twMerge("h-full flex-1 overflow-x-auto", className)}
            ref={tableRef}
            data-testid="moveHistoryRows"
        >
            {moveRows}
        </div>
    );
};
export default MoveHistoryRows;
