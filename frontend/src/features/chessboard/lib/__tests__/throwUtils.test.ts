import {
    clampPointIdx,
    clampSideIdx,
    getThrowData,
    isOnThrowLine,
    ThrowData,
    ThrowLane,
} from "../throwUtils";

import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";
import { logicalPoint, offset } from "@/features/point/pointUtils";
import { PendingThrow } from "../../stores/throwSlice";
import { GameColor } from "@/lib/apiClient";

describe("getThrowData", () => {
    it("should return null when pendingThrow is null", () => {
        expect(getThrowData(null)).toBeNull();
    });

    it("should build forward throw data for white piece", () => {
        const pendingThrow: PendingThrow = {
            piece: createFakePiece({
                color: GameColor.WHITE,
                position: logicalPoint({ x: 5, y: 5 }),
            }),
            throwerOrigin: logicalPoint({ x: 5, y: 4 }),
            points: [
                logicalPoint({ x: 5, y: 6 }),
                logicalPoint({ x: 5, y: 7 }),
            ],
            resolve: vi.fn(),
        };

        const result = getThrowData(pendingThrow)!;

        expect(result.direction).toEqual(offset({ x: 0, y: 1 }));
        expect(result.lanes.length).toBe(1);
        expect(result.lanes[0].points).toEqual([
            logicalPoint({ x: 5, y: 6 }),
            logicalPoint({ x: 5, y: 7 }),
        ]);
    });

    it("should build left throw data", () => {
        const pendingThrow: PendingThrow = {
            piece: createFakePiece({
                color: GameColor.WHITE,
                position: logicalPoint({ x: 5, y: 5 }),
            }),
            throwerOrigin: logicalPoint({ x: 6, y: 5 }),
            points: [
                logicalPoint({ x: 4, y: 6 }),
                logicalPoint({ x: 3, y: 7 }),
            ],
            resolve: vi.fn(),
        };

        const result = getThrowData(pendingThrow)!;

        expect(result.direction).toEqual(offset({ x: -1, y: 1 }));
        expect(result.lanes.length).toBe(1);
    });

    it("should build right throw data for white piece", () => {
        const pendingThrow: PendingThrow = {
            piece: createFakePiece({
                color: GameColor.WHITE,
                position: logicalPoint({ x: 5, y: 5 }),
            }),
            throwerOrigin: logicalPoint({ x: 4, y: 5 }),
            points: [
                logicalPoint({ x: 6, y: 6 }),
                logicalPoint({ x: 7, y: 7 }),
            ],
            resolve: vi.fn(),
        };

        const result = getThrowData(pendingThrow)!;

        expect(result.direction).toEqual(offset({ x: 1, y: 1 }));
        expect(result.lanes.length).toBe(1);
    });

    it("should reverse lanes for black piece", () => {
        const pendingThrow: PendingThrow = {
            piece: createFakePiece({
                color: GameColor.BLACK,
                position: logicalPoint({ x: 5, y: 5 }),
            }),
            throwerOrigin: logicalPoint({ x: 5, y: 6 }),
            points: [
                logicalPoint({ x: 5, y: 4 }),
                logicalPoint({ x: 5, y: 3 }),
            ],
            resolve: vi.fn(),
        };

        const result = getThrowData(pendingThrow)!;

        expect(result.direction).toEqual(offset({ x: 0, y: -1 }));
        expect(result.lanes.length).toBe(1);
    });
});

describe("isOnThrowLine", () => {
    it("should return true for collinear point", () => {
        const throwData: ThrowData = {
            direction: offset({ x: 1, y: 1 }),
            lanes: [
                {
                    origin: logicalPoint({ x: 0, y: 0 }),
                    points: [],
                },
            ],
        };

        const point = logicalPoint({ x: 2, y: 2 });

        expect(isOnThrowLine(point, 0, throwData)).toBe(true);
    });

    it("should return false for non-collinear point", () => {
        const throwData: ThrowData = {
            direction: offset({ x: 1, y: 1 }),
            lanes: [
                {
                    origin: logicalPoint({ x: 0, y: 0 }),
                    points: [],
                },
            ],
        };

        const point = logicalPoint({ x: 2, y: 3 });

        expect(isOnThrowLine(point, 0, throwData)).toBe(false);
    });

    it("should return false for invalid sideIdx", () => {
        const throwData: ThrowData = {
            direction: offset({ x: 1, y: 1 }),
            lanes: [],
        };

        const point = logicalPoint({ x: 1, y: 1 });

        expect(isOnThrowLine(point, 0, throwData)).toBe(false);
    });
});

describe("clampPointIdx", () => {
    it.each([
        [5, 3, 2],
        [-1, 3, 0],
        [1, 3, 1],
    ])("should clamp point index", (inputIdx, length, expectedIdx) => {
        const lane: ThrowLane = {
            origin: logicalPoint({ x: 0, y: 0 }),
            points: new Array(length).fill(logicalPoint({ x: 0, y: 0 })),
        };

        expect(clampPointIdx(inputIdx, lane)).toBe(expectedIdx);
    });
});

describe("clampSideIdx", () => {
    it.each([
        [5, 3, 2],
        [-1, 3, 0],
        [1, 3, 1],
    ])("should clamp side index", (inputIdx, length, expectedIdx) => {
        const throwData: ThrowData = {
            direction: offset({ x: 0, y: 1 }),
            lanes: new Array(length).fill({
                origin: logicalPoint({ x: 0, y: 0 }),
                points: [],
            }),
        };

        expect(clampSideIdx(inputIdx, throwData)).toBe(expectedIdx);
    });
});
