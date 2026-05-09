import clsx from "clsx";

import { useChessboardStore } from "../../hooks/useChessboard";
import { Position } from "../../lib/position";
import { GameColor } from "@/lib/apiClient";

const MoveVariation = ({ variations }: { variations: readonly Position[] }) => {
    const nodes: React.ReactElement[] = [];

    for (const variation of variations) {
        const untilMultiVariation: Position[] = [];
        let withMultiVariation: Position | undefined;
        for (const nextPosition of variation) {
            untilMultiVariation.push(nextPosition);
            if (nextPosition.variations.length > 1) {
                withMultiVariation = nextPosition;
                break;
            }
        }

        nodes.push(
            <Line
                key={"node:" + variation.positionId}
                positions={untilMultiVariation}
            />,
        );

        if (withMultiVariation) {
            nodes.push(
                <MoveVariation
                    key={"variation:" + variation.positionId}
                    variations={withMultiVariation.variations}
                />,
            );
        }
    }

    return (
        <div
            className="before:bg-text/30 relative ml-3 flex flex-col pl-4
                select-none before:absolute before:top-0 before:bottom-5
                before:left-0 before:w-0.5"
            data-testid="moveVariations"
        >
            {nodes}
        </div>
    );
};
export default MoveVariation;

const Line = ({ positions }: { positions: Position[] }) => {
    function getPositionFormattedMoveNumber(
        position: Position,
        index: number,
    ): string {
        // compare to the opposite color because position.sideToMove refers to the side to move after the move was played
        const isWhiteMove = position.sideToMove === GameColor.BLACK;

        const startedWithBlack =
            (isWhiteMove && position.ply % 2 === 0) ||
            (!isWhiteMove && position.ply % 2 !== 0);

        const moveNumber = startedWithBlack
            ? Math.ceil((position.ply + 1) / 2)
            : Math.ceil(position.ply / 2);

        if (index !== 0 && !isWhiteMove) return "";

        const dots = isWhiteMove ? "." : "...";
        return moveNumber + dots;
    }

    return (
        <div
            className="text-text/60 before:border-text/30 relative p-1
                before:absolute before:top-4.5 before:-left-3.5 before:w-3
                before:border-t-2"
            data-testid="lineVariation"
        >
            {positions.map((position, index) => (
                <LineMove
                    key={position.positionId}
                    position={position}
                    dots={getPositionFormattedMoveNumber(position, index)}
                />
            ))}
        </div>
    );
};

const LineMove = ({ position, dots }: { position: Position; dots: string }) => {
    const { goToPosition, isSelected } = useChessboardStore((x) => ({
        goToPosition: x.goToPosition,
        isSelected:
            x.positionHistory.viewingPosition?.positionId ===
            position.positionId,
    }));

    return (
        <button
            className={clsx(
                "cursor-pointer p-1 text-nowrap",
                isSelected && "rounded-md bg-blue-300/30",
            )}
            onClick={() => goToPosition(position.positionId)}
        >
            {dots}
            {position.san}
        </button>
    );
};
