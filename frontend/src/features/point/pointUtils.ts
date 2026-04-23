import { Vector3 } from "three";

import {
    ViewPoint,
    ScreenPoint,
    Point,
    LogicalPoint,
    Offset,
    StrPoint,
} from "@/features/point/types";

export function pointToStr(point: Point): StrPoint {
    return `${point.x},${point.y}`;
}

export function idxToLogicalPoint(
    index: number,
    boardWidth: number,
): LogicalPoint {
    return logicalPoint({
        x: index % boardWidth,
        y: Math.floor(index / boardWidth),
    });
}

export function logicalPoint(point: Point): LogicalPoint {
    return point as LogicalPoint;
}

export function viewPoint(point: Point): ViewPoint {
    return point as ViewPoint;
}

export function screenPoint(point: Point): ScreenPoint {
    return point as ScreenPoint;
}

export function offset(point: Point): Offset {
    return point as Offset;
}

export function viewToWorld(point: ViewPoint): Vector3 {
    const squareSize = 0.775; // we don't talk about how I got this number
    const boardSize = 10;

    const offset = (boardSize * squareSize) / 2;

    const vx = point.x * squareSize - offset + squareSize / 2;
    const vy = offset - point.y * squareSize - squareSize / 2;
    return new Vector3(vx, vy);
}

export function pointEquals(a?: Point | null, b?: Point | null): boolean {
    if (a == null && b == null) return true;
    else if (a == null || b == null) return false;

    return a.x === b.x && a.y === b.y;
}

export function pointArraysEqual(a: Point[], b: Point[]): boolean {
    if (a.length !== b.length) return false;
    for (let i = 0; i < a.length; i++) {
        if (!pointEquals(a[i], b[i])) return false;
    }
    return true;
}

export function sortPoints<T extends Point>(points: T[]): T[] {
    return [...points].sort((a, b) => a.x - b.x || a.y - b.y);
}

export function pointDistanceSquared(from: Point, to: Point): number {
    return (to.x - from.x) ** 2 + (to.y - from.y) ** 2;
}

export function sortPointsByDistanceSquared<T extends Point>(
    origin: Point,
    points: T[],
): T[] {
    return [...points].sort(
        (a, b) =>
            pointDistanceSquared(origin, a) - pointDistanceSquared(origin, b),
    );
}
