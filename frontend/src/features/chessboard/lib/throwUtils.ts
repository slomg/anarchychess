import {
    logicalPoint,
    offset,
    sortPointsByDistanceSquared,
} from "@/features/point/pointUtils";

import { LogicalPoint, Offset } from "@/features/point/types";
import { PendingThrow } from "../stores/throwSlice";
import { GameColor } from "@/lib/apiClient";

export interface ThrowLane {
    origin: LogicalPoint;
    points: LogicalPoint[];
}

export interface ThrowData {
    direction: Offset;
    lanes: ThrowLane[];
}

export function getThrowData(
    pendingThrow: PendingThrow | null,
): ThrowData | null {
    if (pendingThrow === null) {
        return null;
    }

    const forwardY = pendingThrow.piece.color === GameColor.WHITE ? 1 : -1;

    if (pendingThrow.throwerOrigin.x - pendingThrow.piece.position.x === 0) {
        // forward
        return buildThrowData(pendingThrow, offset({ x: 0, y: forwardY }), {
            leftOrigin: logicalPoint({
                x: pendingThrow.piece.position.x - 1,
                y: pendingThrow.piece.position.y,
            }),
            centerOrigin: pendingThrow.piece.position,
            rightOrigin: logicalPoint({
                x: pendingThrow.piece.position.x + 1,
                y: pendingThrow.piece.position.y,
            }),
        });
    } else if (
        pendingThrow.throwerOrigin.x - pendingThrow.piece.position.x >
        0
    ) {
        // left
        return buildThrowData(pendingThrow, offset({ x: -1, y: forwardY }), {
            leftOrigin: logicalPoint({
                x: pendingThrow.piece.position.x,
                y: pendingThrow.piece.position.y - forwardY,
            }),
            centerOrigin: pendingThrow.piece.position,
            rightOrigin: logicalPoint({
                x: pendingThrow.piece.position.x,
                y: pendingThrow.piece.position.y + forwardY,
            }),
        });
    } else {
        // right
        return buildThrowData(pendingThrow, offset({ x: 1, y: forwardY }), {
            leftOrigin: logicalPoint({
                x: pendingThrow.piece.position.x,
                y: pendingThrow.piece.position.y + forwardY,
            }),
            centerOrigin: pendingThrow.piece.position,
            rightOrigin: logicalPoint({
                x: pendingThrow.piece.position.x,
                y: pendingThrow.piece.position.y - forwardY,
            }),
        });
    }
}

export function isOnLane(
    point: LogicalPoint,
    sideIdx: number,
    throwData: ThrowData,
): boolean {
    const origin = throwData.lanes[sideIdx]?.origin;
    if (!origin) {
        return false;
    }
    return isCollinearWithDirection(point, origin, throwData.direction);
}

export function clampPointIdx(pointIdx: number, throwLane: ThrowLane): number {
    return Math.max(Math.min(pointIdx, throwLane.points.length - 1), 0);
}

export function clampLaneIdx(sideIdx: number, throwData: ThrowData): number {
    return Math.max(Math.min(sideIdx, throwData.lanes.length - 1), 0);
}

function isCollinearWithDirection(
    point: LogicalPoint,
    origin: LogicalPoint,
    direction: Offset,
) {
    return (
        (point.x - origin.x) * direction.y ===
        (point.y - origin.y) * direction.x
    );
}

function buildThrowData(
    pendingThrow: PendingThrow,
    direction: Offset,
    {
        leftOrigin,
        centerOrigin,
        rightOrigin,
    }: {
        leftOrigin: LogicalPoint;
        centerOrigin: LogicalPoint;
        rightOrigin: LogicalPoint;
    },
): ThrowData {
    const leftPoints: LogicalPoint[] = [];
    const centerPoints: LogicalPoint[] = [];
    const rightPoints: LogicalPoint[] = [];
    for (const point of pendingThrow.points) {
        if (isCollinearWithDirection(point, leftOrigin, direction)) {
            leftPoints.push(point);
        } else if (isCollinearWithDirection(point, centerOrigin, direction)) {
            centerPoints.push(point);
        } else if (isCollinearWithDirection(point, rightOrigin, direction)) {
            rightPoints.push(point);
        }
    }

    const lanes: ThrowLane[] = [];
    if (leftPoints.length > 0) {
        lanes.push({
            origin: leftOrigin,
            points: sortPointsByDistanceSquared(leftOrigin, leftPoints),
        });
    }
    if (centerPoints.length > 0) {
        lanes.push({
            origin: centerOrigin,
            points: sortPointsByDistanceSquared(centerOrigin, centerPoints),
        });
    }
    if (rightPoints.length > 0) {
        lanes.push({
            origin: rightOrigin,
            points: sortPointsByDistanceSquared(rightOrigin, rightPoints),
        });
    }

    // this needs to be ordered from the left, from the player non flipped perspective
    const orderedLanes =
        pendingThrow.piece.color === GameColor.WHITE ? lanes : lanes.reverse();

    return {
        direction,
        lanes: orderedLanes,
    };
}
