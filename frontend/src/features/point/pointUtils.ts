
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

export function pointEquals(a?: Point, b?: Point): boolean {
    if (a === undefined && b === undefined) return true;
    else if (a === undefined || b === undefined) return false;

    return a.x === b.x && a.y === b.y;
}

export function pointArraysEqual(a: Point[], b: Point[]): boolean {
    if (a.length !== b.length) return false;
    for (let i = 0; i < a.length; i++) {
        if (!pointEquals(a[i], b[i])) return false;
    }
    return true;
}

export function pointWithinArray(point: Point, arr: Point[]): boolean {
    return arr.some((p) => pointEquals(p, point));
}

export function pointArrayStartsWith(arr: Point[], prefix: Point[]): boolean {
    if (prefix.length > arr.length) return false;
    for (let i = 0; i < prefix.length; i++) {
        if (!pointEquals(arr[i], prefix[i])) return false;
    }
    return true;
}

export function sortPoints<T extends Point>(points: T[]): T[] {
    return [...points].sort((a, b) => a.x - b.x || a.y - b.y);
}

export function pointDistanceSquared(from: Point, to: Point): number {
    return (to.x - from.x) ** 2 + (to.y - from.y) ** 2;
}
