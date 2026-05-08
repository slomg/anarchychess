import {
    forwardRef,
    ForwardRefRenderFunction,
    HTMLAttributes,
    memo,
    ReactNode,
    useImperativeHandle,
    useRef,
} from "react";

import { twMerge } from "tailwind-merge";

import { Point, ViewPoint } from "@/features/point/types";
import { pointToStr } from "@/features/point/pointUtils";
import constants from "@/lib/constants";

export type ChessCoordProps = {
    position: ViewPoint;
    rotation?: number;
    children?: ReactNode;
} & HTMLAttributes<HTMLDivElement>;

export interface ChessSquareRef {
    updateDraggingOffset: (offset: Point) => void;
    getBoundingClientRect: () => DOMRect | null;
}

/**
 * Render an element in a specific location on the chess board
 */
const CoordSquare: ForwardRefRenderFunction<ChessSquareRef, ChessCoordProps> = (
    { position, rotation = 0, children, className, style, ...divProps },
    ref,
) => {
    const squareDivRef = useRef<HTMLDivElement>(null);

    const { x, y } = position;
    const tileWidth = 100 / constants.BOARD_WIDTH;
    const tileHeight = 100 / constants.BOARD_HEIGHT;

    const physicalX = x * 100;
    const physicalY = y * 100;

    const maxX = (constants.BOARD_WIDTH - 1) * 100;
    const maxY = (constants.BOARD_HEIGHT - 1) * 100;

    function calculateTransform(offset: Point): string {
        const translate = `translate(
            clamp(0%, calc(${physicalX}% + ${offset.x}px), ${maxX}%),
            clamp(0%, calc(${physicalY}% + ${offset.y}px), ${maxY}%))`;
        const rotate = rotation != 0 ? `rotate(${rotation}deg)` : "";
        return translate + " " + rotate;
    }

    useImperativeHandle(ref, () => ({
        updateDraggingOffset(offset: Point) {
            if (squareDivRef.current) {
                squareDivRef.current.style.transform =
                    calculateTransform(offset);
            }
        },
        getBoundingClientRect: () =>
            squareDivRef.current?.getBoundingClientRect() ?? null,
    }));

    return (
        <div
            data-position={pointToStr(position)}
            data-testid="coordSquare"
            className={twMerge(
                "absolute transform will-change-transform",
                className,
            )}
            style={{
                width: `${tileWidth}%`,
                height: `${tileHeight}%`,
                transform: calculateTransform({ x: 0, y: 0 }),
                ...style,
            }}
            ref={squareDivRef}
            {...divProps}
        >
            {children}
        </div>
    );
};
export default memo(forwardRef(CoordSquare));
