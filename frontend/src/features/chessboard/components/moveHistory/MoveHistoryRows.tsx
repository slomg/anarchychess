import { useEffect, useRef } from "react";
import { twMerge } from "tailwind-merge";

import { useChessboardStore } from "../../hooks/useChessboard";
import { ChildPosition } from "../../lib/position";
import useAutoScroll from "@/hooks/useAutoScroll";
import MoveVariation from "./MoveVariation";
import { GameColor } from "@/lib/apiClient";
import MoveRow from "./MoveRow";

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
        const pos1 = iterator.next();
        if (pos1.done) {
            break;
        }

        let whitePosition: ChildPosition | undefined;
        let blackPosition: ChildPosition | undefined;

        if (pos1.value.sideToMove === GameColor.WHITE) {
            blackPosition = pos1.value;
            moveRows.push(
                <MoveRow
                    ply={pos1.value.ply}
                    blackPosition={pos1.value}
                    key={pos1.value.ply}
                />,
            );
        } else {
            whitePosition = pos1.value;

            const pos2 = iterator.next();
            blackPosition = pos2.done ? undefined : pos2.value;

            const plyOffset =
                positionHistory.root.sideToMove === GameColor.BLACK ? 1 : 0;
            moveRows.push(
                <MoveRow
                    ply={pos1.value.ply + plyOffset}
                    whitePosition={pos1.value}
                    blackPosition={blackPosition}
                    key={pos1.value.ply}
                />,
            );
        }

        if (pendingWhiteMoveVariation) {
            moveRows.push(pendingWhiteMoveVariation);
            pendingWhiteMoveVariation = null;
        }

        if (whitePosition && whitePosition.subVariationByKey.size > 0) {
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
