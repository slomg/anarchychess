import { it, expect, describe } from "vitest";

import {
    pointToStr,
    idxToLogicalPoint,
    logicalPoint,
    viewPoint,
    viewToWorld,
    pointEquals,
    pointArraysEqual,
    sortPoints,
    pointDistanceSquared,
    sortPointsByDistanceSquared,
    logicalToAlgebraic,
    algebraicToLogical,
} from "../pointUtils";

import { Point } from "../types";

describe("pointToStr", () => {
    it("should convert a point to a string", () => {
        const point = { x: 3, y: 5 };
        expect(pointToStr(point)).toBe("3,5");
    });
});

describe("idxToLogicalPoint", () => {
    it("should convert an index to a logical point", () => {
        const index = 16;
        const expectedPoint = logicalPoint({ x: 6, y: 1 });

        const result = idxToLogicalPoint(index);
        expect(result).toEqual(expectedPoint);
    });
});

describe("viewToWorld", () => {
    it("should convert a viewPoint to world coordinates", () => {
        const view = viewPoint({ x: 6, y: 9 });

        const result = viewToWorld(view);

        expect(result.x).toBeCloseTo(1.1625);
        expect(result.y).toBeCloseTo(-3.4875);
    });
});

describe("logicalToAlgebraic", () => {
    it.each([
        [logicalPoint({ x: 0, y: 0 }), "a1"],
        [logicalPoint({ x: 1, y: 1 }), "b2"],
        [logicalPoint({ x: 5, y: 7 }), "f8"],
        [logicalPoint({ x: 25, y: 7 }), "z8"],
    ])("should convert point to algebraic string", (point, expected) => {
        expect(logicalToAlgebraic(point)).toBe(expected);
    });
});

describe("algebraicToLogical", () => {
    it.each([
        ["a1", logicalPoint({ x: 0, y: 0 })],
        ["b2", logicalPoint({ x: 1, y: 1 })],
        ["f8", logicalPoint({ x: 5, y: 7 })],
        ["z8", logicalPoint({ x: 25, y: 7 })],
    ])("should convert algebraic to point", (algebraic, expected) => {
        expect(algebraicToLogical(algebraic)).toEqual(expected);
    });
});

describe("pointEquals", () => {
    it.each([
        [{ x: 1, y: 2 }, { x: 1, y: 2 }, true],
        [{ x: 1, y: 2 }, { x: 2, y: 1 }, false],
        [null, null, true],
        [{ x: 2, y: 1 }, null, false],
    ])("should correctly compare points", (a, b, equals) => {
        expect(pointEquals(a, b)).toBe(equals);
    });
});

describe("pointArraysEqual", () => {
    it.each([
        [
            [
                { x: 1, y: 2 },
                { x: 3, y: 4 },
            ],
            [
                { x: 1, y: 2 },
                { x: 3, y: 4 },
            ],
            true,
        ],
        [
            [
                { x: 1, y: 2 },
                { x: 3, y: 4 },
            ],
            [
                { x: 1, y: 2 },
                { x: 6, y: 9 },
            ],
            false,
        ],
    ])("should correctly compare arrays of points", (a, b, equals) => {
        expect(pointArraysEqual(a, b)).toBe(equals);
    });
});

describe("sortPoints", () => {
    it("should sort points by x then y", () => {
        const unsortedPoints = [
            { x: 2, y: 1 },
            { x: 1, y: 2 },
            { x: 1, y: 1 },
        ];

        const result = sortPoints(unsortedPoints);

        expect(result).toEqual([
            { x: 1, y: 1 },
            { x: 1, y: 2 },
            { x: 2, y: 1 },
        ]);
    });
});

describe("pointDistanceSquared", () => {
    it("should calculate squared distance between points", () => {
        const pointStart = { x: 1, y: 2 };
        const pointEnd = { x: 4, y: 6 };
        const expectedDistance = 25;

        expect(pointDistanceSquared(pointStart, pointEnd)).toBe(
            expectedDistance,
        );
    });
});

describe("sortPointsByDistanceSquared", () => {
    it("should sort points by distance squared from origin", () => {
        const origin: Point = { x: 0, y: 0 };

        const points: Point[] = [
            { x: 3, y: 4 }, // 25
            { x: 1, y: 1 }, // 2
            { x: 2, y: 2 }, // 8
        ];

        const result = sortPointsByDistanceSquared(origin, points);

        expect(result).toEqual([
            { x: 1, y: 1 },
            { x: 2, y: 2 },
            { x: 3, y: 4 },
        ]);
    });
});
