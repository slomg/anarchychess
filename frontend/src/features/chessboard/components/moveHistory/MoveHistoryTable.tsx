import {
    ArrowsUpDownIcon,
    ChevronRightIcon,
    ChevronDoubleLeftIcon,
    ChevronDoubleRightIcon,
    ChevronLeftIcon,
} from "@heroicons/react/24/solid";

import React, { useEffect, useRef } from "react";

import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import useAutoScroll from "@/hooks/useAutoScroll";
import MoveVariation from "./MoveVariation";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import MoveRow from "./MoveRow";

const MoveHistoryTable = ({ title }: { title?: React.ReactNode }) => {
    const { totalPlyCount, positionHistory } = useChessboardStore((x) => ({
        totalPlyCount: x.positionHistory.totalPlyCount,
        positionHistory: x.positionHistory,
    }));

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
    if (pendingWhiteMoveVariation) moveRows.push(pendingWhiteMoveVariation);

    return (
        <Card
            className="relative max-h-96 w-full gap-0 overflow-hidden p-0
                lg:max-h-full"
        >
            {title && (
                <div
                    className="bg-primary flex items-center justify-center gap-1
                        rounded-t-md p-1 text-2xl"
                    data-testid="moveHistoryTitle"
                >
                    {title}
                </div>
            )}

            <div
                className="border-primary order-1 flex gap-3 border-b p-3
                    lg:order-2 lg:border-t lg:border-b-0"
            >
                <Button onClick={goToStartPosition} title="Go to Start">
                    <ChevronDoubleLeftIcon className="h-8 w-8" />
                </Button>
                <Button onClick={stepPositionBackward} title="Previous Move">
                    <ChevronLeftIcon className="h-8 w-8" />
                </Button>
                <Button onClick={stepPositionForward} title="Next Move">
                    <ChevronRightIcon className="h-8 w-8" />
                </Button>
                <Button onClick={goToLatestPosition} title="Go to End">
                    <ChevronDoubleRightIcon className="h-8 w-8" />
                </Button>

                <Button
                    className="ml-auto"
                    onClick={flipBoard}
                    title="Flip Board"
                >
                    <ArrowsUpDownIcon className="h-8 w-8" />
                </Button>
            </div>

            <div
                className="order-2 h-full flex-1 overflow-x-auto lg:order-1"
                ref={tableRef}
                data-testid="moveHistoryContents"
            >
                {moveRows}
            </div>
        </Card>
    );
};
export default MoveHistoryTable;
