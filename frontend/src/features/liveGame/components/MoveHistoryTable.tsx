import React, { useEffect, useMemo, useRef } from "react";
import clsx from "clsx";

import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import useAutoScroll from "@/hooks/useAutoScroll";
import Card from "@/components/ui/Card";
import GameActions from "./GameActions";

const MoveHistoryTable = () => {
    const positionHistory = useChessboardStore((x) => x.positionHistory);
    const { shiftMoveViewBy, teleportToPosition, teleportToLatestPosition } =
        useChessboardStore((x) => ({
            shiftMoveViewBy: x.shiftMoveViewBy,
            teleportToPosition: x.teleportToPosition,
            teleportToLatestPosition: x.teleportToLatestPosition,
        }));

    const tableRef = useRef<HTMLDivElement | null>(null);
    useAutoScroll(tableRef, [positionHistory]);

    const moveRows: React.ReactElement[] = useMemo(() => {
        let rowIndex = 1;
        const moveRows: React.ReactElement[] = [];
        for (let i = 1; i < positionHistory.length; i += 2) {
            const currentMove = positionHistory[i].san;
            const nextMove = positionHistory[i + 1]?.san;

            moveRows.push(
                <MoveRow
                    key={i}
                    index={rowIndex}
                    moveWhite={currentMove}
                    moveBlack={nextMove}
                />,
            );

            rowIndex++;
        }
        return moveRows;
    }, [positionHistory]);

    useEffect(() => {
        async function onKeyDown(event: KeyboardEvent): Promise<void> {
            switch (event.key) {
                case "ArrowLeft":
                    await shiftMoveViewBy(-1);
                    break;
                case "ArrowRight":
                    await shiftMoveViewBy(1);
                    break;
                case "ArrowUp":
                    await teleportToPosition(0);
                    break;
                case "ArrowDown":
                    await teleportToLatestPosition();
                    break;
            }
        }

        window.addEventListener("keydown", onKeyDown);
        return () => window.removeEventListener("keydown", onKeyDown);
    }, [shiftMoveViewBy, teleportToPosition, teleportToLatestPosition]);

    return (
        <Card className="relative block max-h-96 p-0 lg:max-h-full">
            <div className="max-h-full overflow-x-auto" ref={tableRef}>
                <table className="w-full table-fixed">
                    <tbody>{moveRows}</tbody>
                </table>
            </div>

            <GameActions />
        </Card>
    );
};
export default MoveHistoryTable;

const MoveRow = ({
    moveWhite,
    moveBlack,
    index,
}: {
    moveWhite?: string;
    moveBlack?: string;
    index: number;
}) => {
    const whiteMoveIdx = index * 2 - 1;
    const blackMoveIdx = whiteMoveIdx + 1;

    const { teleportToPosition, isViewingWhite, isViewingBlack } =
        useChessboardStore((x) => ({
            teleportToPosition: x.teleportToPosition,
            isViewingWhite: x.viewingPlyIdx === whiteMoveIdx,
            isViewingBlack: x.viewingPlyIdx === blackMoveIdx,
        }));

    const color = index % 2 === 0 ? "bg-white/10" : "";
    const selectedClass = "bg-blue-300/30";
    return (
        <tr className={color}>
            <td className="bg-card w-10 p-3">{index}.</td>
            <td
                className={clsx(
                    "cursor-pointer overflow-x-auto p-3",
                    isViewingWhite && selectedClass,
                )}
                onClick={() => teleportToPosition(whiteMoveIdx)}
            >
                <div className="overflow-x-auto">{moveWhite}</div>
            </td>
            <td
                className={clsx(
                    "cursor-pointer overflow-x-auto p-3",
                    isViewingBlack && selectedClass,
                )}
                onClick={() => teleportToPosition(blackMoveIdx)}
            >
                <div className="overflow-x-auto">{moveBlack}</div>
            </td>
        </tr>
    );
};
