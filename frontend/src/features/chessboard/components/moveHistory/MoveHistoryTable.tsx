import { ArrowsUpDownIcon } from "@heroicons/react/24/solid";
import React, { useEffect, useRef } from "react";
import clsx from "clsx";

import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import useAutoScroll from "@/hooks/useAutoScroll";
import Card from "@/components/ui/Card";
import { Position } from "../../lib/positionHistory";

const MoveHistoryTable = () => {
    useChessboardStore((x) => x.positionHistory.totalPlyCount);
    const positionHistory = useChessboardStore((x) => x.positionHistory);

    const {
        stepPositionForward,
        stepPositionBackward,
        goToStartPosition,
        goToLatestPosition,
        flipBoard,
    } = useChessboardStore((x) => ({
        stepPositionForward: x.stepPositionForward,
        stepPositionBackward: x.stepPositionBackward,
        goToStartPosition: x.goToStartPosition,
        goToLatestPosition: x.goToLatestPosition,
        flipBoard: x.flipBoard,
    }));

    const tableRef = useRef<HTMLDivElement | null>(null);
    useAutoScroll(tableRef, [positionHistory]);

    let rowIndex = 1;
    const moveRows: React.ReactElement[] = [];

    const iterator = positionHistory[Symbol.iterator]();
    while (true) {
        const white = iterator.next();
        const black = iterator.next();

        if (white.done) break;

        moveRows.push(
            <MoveRow
                key={rowIndex}
                index={rowIndex}
                whitePosition={white.value}
                blackPosition={black.done ? undefined : black.value}
            />,
        );

        rowIndex++;
    }

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

    return (
        <Card className="relative block max-h-96 w-full p-0 lg:max-h-full">
            <div className="h-full w-full overflow-x-auto" ref={tableRef}>
                {moveRows}

            <div className="absolute right-0 bottom-0 flex w-fit gap-3 p-3">
                <ArrowsUpDownIcon
                    className="text-secondary h-6 w-6 cursor-pointer"
                    title="Flip Board"
                    onClick={flipBoard}
                />
            </div>
        </Card>
    );
};
export default MoveHistoryTable;

const MoveRow = ({
    whitePosition,
    blackPosition,
    index,
}: {
    whitePosition?: Position;
    blackPosition?: Position;
    index: number;
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

    const color = index % 2 === 0 ? "bg-white/10" : "";
    const selectedClass = "bg-blue-300/30";
    return (
        <div className={clsx("relative flex", color)}>
            <div className="bg-card w-10 p-3">{index}.</div>
            <div
                className={clsx(
                    "flex-1 cursor-pointer overflow-x-auto p-3",
                    isViewingWhite && selectedClass,
                )}
                onClick={() =>
                    whitePosition && goToPosition(whitePosition.positionId)
                }
            >
                <div className="overflow-x-auto">{whitePosition?.san}</div>
            </div>
            <div
                className={clsx(
                    "flex-1 cursor-pointer overflow-x-auto p-3",
                    isViewingBlack && selectedClass,
                )}
                onClick={() =>
                    blackPosition && goToPosition(blackPosition.positionId)
                }
            >
                <div className="overflow-x-auto">{blackPosition?.san}</div>
            </div>
        </div>
    );
};
