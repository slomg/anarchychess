import { StateCreator } from "zustand";

import type { ChessboardStore } from "./chessboardStore";
import { GameColor } from "@/lib/apiClient";
import { LogicalPoint } from "@/features/point/types";
import { ViewPoint } from "@/features/point/types";
import { ScreenPoint } from "@/features/point/types";
import { Point } from "@/features/point/types";
import { logicalPoint, viewPoint } from "@/features/point/pointUtils";
import { invertColor } from "@/lib/utils/chessUtils";
import constants from "@/lib/constants";

export interface BoardSliceProps {
    viewingFrom: GameColor;
}

interface ImmerDOMRect {
    readonly bottom: number;
    readonly height: number;
    readonly left: number;
    readonly right: number;
    readonly top: number;
    readonly width: number;
    readonly x: number;
    readonly y: number;
}

export interface BoardSlice extends BoardSliceProps {
    viewingFrom: GameColor;
    boardRect?: ImmerDOMRect;

    screenToLogicalPoint(screenPoint: ScreenPoint): LogicalPoint | undefined;
    screenToViewPoint(screenPoint: ScreenPoint): ViewPoint | undefined;
    logicalPointToScreenPoint(
        logicalPoint: LogicalPoint,
    ): ScreenPoint | undefined;

    viewPointToLogicalPoint(viewPoint: ViewPoint): LogicalPoint;
    logicalPointToViewPoint(logicalPoint: LogicalPoint): ViewPoint;

    flipBoard(): void;
    setBoardRect(rect: DOMRect): void;
}

export function createBoardSlice(
    initState: BoardSliceProps,
): StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    BoardSlice
> {
    return (set, get) => ({
        ...initState,

        screenToLogicalPoint(screenPoint) {
            const { screenToViewPoint, viewPointToLogicalPoint } = get();

            const viewPoint = screenToViewPoint(screenPoint);
            if (!viewPoint) return;

            return viewPointToLogicalPoint(viewPoint);
        },
        screenToViewPoint(screenPoint) {
            const { boardRect } = get();
            if (!boardRect) return;

            const relX = screenPoint.x - boardRect.left;
            const relY = screenPoint.y - boardRect.top;
            if (relX < 0 || relY < 0) {
                return;
            }

            const x = Math.floor(
                (relX / boardRect.width) * constants.BOARD_WIDTH,
            );
            const y = Math.floor(
                (relY / boardRect.height) * constants.BOARD_HEIGHT,
            );

            if (x >= constants.BOARD_WIDTH) {
                return;
            }
            if (y >= constants.BOARD_WIDTH) {
                return;
            }

            return viewPoint({ x, y });
        },
        logicalPointToScreenPoint(logicalPoint) {
            const { logicalPointToViewPoint, boardRect } = get();
            if (!boardRect) return;

            const viewPoint = logicalPointToViewPoint(logicalPoint);
            const screenX =
                boardRect.left +
                ((viewPoint.x + 0.5) / constants.BOARD_WIDTH) * boardRect.width;
            const screenY =
                boardRect.top +
                ((viewPoint.y + 0.5) / constants.BOARD_HEIGHT) *
                    boardRect.height;

            return { x: screenX, y: screenY } as ScreenPoint;
        },

        // both perform the same coordinate transformation
        // we have both for clarity
        viewPointToLogicalPoint(viewPoint) {
            const { viewingFrom } = get();
            return logicalPoint(
                flipPointForPerspective(viewPoint, viewingFrom),
            );
        },
        logicalPointToViewPoint(logicalPoint) {
            const { viewingFrom } = get();
            return viewPoint(
                flipPointForPerspective(logicalPoint, viewingFrom),
            );
        },

        flipBoard() {
            set((state) => {
                state.viewingFrom = invertColor(state.viewingFrom);
            });
        },
        setBoardRect(rect) {
            set((state) => {
                state.boardRect = {
                    bottom: rect.bottom,
                    height: rect.height,
                    left: rect.left,
                    right: rect.right,
                    top: rect.top,
                    width: rect.width,
                    x: rect.x,
                    y: rect.y,
                };
            });
        },
    });
}

function flipPointForPerspective(point: Point, viewingFrom: GameColor) {
    let { x, y } = point;
    if (viewingFrom === GameColor.WHITE) {
        y = constants.BOARD_HEIGHT - y - 1;
    } else {
        x = constants.BOARD_WIDTH - x - 1;
    }
    return { x, y };
}
