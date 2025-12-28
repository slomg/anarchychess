import { logicalPoint, pointToStr } from "@/features/point/pointUtils";
import { createFakeMove } from "@/lib/testUtils/fakers/chessboardFakers";
import { Move } from "../types";
import LegalMoves from "../legalMoves";
import { StrPoint } from "@/features/point/types";

describe("legalMoves", () => {
    describe("constructor", () => {
        it("should create an empty LegalMoves instance when no arguments are provided", () => {
            const legalMoves = new LegalMoves();
            expect(legalMoves.size).toBe(0);
            expect(legalMoves.hasForcedMoves).toBe(false);
        });

        it("should create a LegalMoves instance from a Map", () => {
            const move = createFakeMove();
            const legalMovesMap = new Map<StrPoint, Move[]>([
                [pointToStr(move.from), [move]],
            ]);
            const legalMoves = new LegalMoves(legalMovesMap, true);

            expect(legalMoves.size).toBe(1);
            expect(legalMoves.hasForcedMoves).toBe(true);
            const movesFromOrigin = legalMoves.get(move.from);
            expect(movesFromOrigin).toEqual([move]);
        });

        it("should create a LegalMoves instance from an array of entries", () => {
            const move = createFakeMove();
            const legalMovesArray: [StrPoint, Move[]][] = [
                [pointToStr(move.from), [move]],
            ];

            const legalMoves = new LegalMoves(legalMovesArray, false);

            expect(legalMoves.size).toBe(1);
            expect(legalMoves.hasForcedMoves).toBe(false);
            const movesFromOrigin = legalMoves.get(move.from);
            expect(movesFromOrigin).toEqual([move]);
        });
    });

    describe("hasMovesFromTo", () => {
        it("should return false if there are no moves from the given 'from' position", () => {
            const move = createFakeMove({
                from: logicalPoint({ x: 0, y: 0 }),
                to: logicalPoint({ x: 2, y: 2 }),
            });

            const legalMoves = new LegalMoves([
                [pointToStr(move.from), [move]],
            ]);

            const result = legalMoves.hasMovesFromTo(
                logicalPoint({ x: 1, y: 1 }),
                move.to,
            );
            expect(result).toBe(false);
        });

        it("should return false if there are moves from 'from' but none to the 'to' position", () => {
            const move = createFakeMove({
                from: logicalPoint({ x: 0, y: 0 }),
                to: logicalPoint({ x: 2, y: 2 }),
            });

            const legalMoves = new LegalMoves([
                [pointToStr(move.from), [move]],
            ]);

            const result = legalMoves.hasMovesFromTo(
                move.from,
                logicalPoint({ x: 1, y: 1 }),
            );
            expect(result).toBe(false);
        });

        it("should return true if there is at least one move from 'from' to 'to'", () => {
            const move1 = createFakeMove({
                from: logicalPoint({ x: 0, y: 0 }),
                to: logicalPoint({ x: 1, y: 1 }),
            });
            const move2 = createFakeMove({
                from: logicalPoint({ x: 0, y: 0 }),
                to: logicalPoint({ x: 2, y: 2 }),
            });

            const legalMoves = new LegalMoves([
                [pointToStr(move1.from), [move1, move2]],
            ]);

            const result = legalMoves.hasMovesFromTo(
                logicalPoint({ x: 0, y: 0 }),
                logicalPoint({ x: 1, y: 1 }),
            );
            expect(result).toBe(true);
        });
    });

    describe("iterator", () => {
        it("should iterate over all move arrays in the LegalMoves instance", () => {
            const move1 = createFakeMove();
            const move2 = createFakeMove();

            const legalMoves = new LegalMoves([
                [pointToStr(move1.from), [move1]],
                [pointToStr(move2.from), [move2]],
            ]);

            const movesArray = Array.from(legalMoves);
            expect(movesArray).toEqual([[move1], [move2]]);
        });
    });
});
