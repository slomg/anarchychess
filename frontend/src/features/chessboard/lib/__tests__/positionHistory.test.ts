import {
    createFakeBoardPieces,
    createFakeMove,
} from "@/lib/testUtils/fakers/chessboardFakers";

import PositionHistory, { Position } from "../positionHistory";
import { PositionId } from "../types";
import BoardPieces from "../boardPieces";

describe("PositionHistory", () => {
    let rootPieces: BoardPieces;
    let history: PositionHistory;

    beforeEach(() => {
        rootPieces = createFakeBoardPieces();
        history = new PositionHistory(rootPieces);
    });
    describe("constructor", () => {
        it("should initialize with the given root pieces", () => {
            expect(history.rootPieces).toBe(rootPieces);
            expect(history.plyCount).toBe(0);
        });
    });

    describe("getByPositionId", () => {
        it("should return undefined if positionId does not exist", () => {
            expect(
                history.getByPositionId("nonexistent-id" as PositionId),
            ).toBeUndefined();
        });

        it("should return the correct position after it is added", () => {
            const pieces = createFakeBoardPieces();
            const move = createFakeMove();
            const pos = history.createMainPosition(pieces, move, "e4");

            expect(history.getByPositionId(pos.positionId)).toBe(pos);
        });
    });

    describe("createMainPosition", () => {
        it("should create the first position and set head and tail", () => {
            const pieces = createFakeBoardPieces();
            const move = createFakeMove();
            const pos = history.createMainPosition(pieces, move, "e4");

            expect(pos.pieces).toBe(pieces);
            expect(pos.move).toBe(move);
            expect(pos.san).toBe("e4");
            expect(history.plyCount).toBe(1);
        });

        it("should append to the main line for subsequent positions", () => {
            const pieces1 = createFakeBoardPieces();
            const pieces2 = createFakeBoardPieces();
            const move1 = createFakeMove();
            const move2 = createFakeMove();

            const pos1 = history.createMainPosition(pieces1, move1, "e4");
            const pos2 = history.createMainPosition(pieces2, move2, "d4");

            expect(history.plyCount).toBe(2);
            expect([...history]).toEqual([pos1, pos2]);
        });
    });

    describe("addVariationToPosition", () => {
        it("should add a variation to a non tail position", () => {
            const pieces1 = createFakeBoardPieces();
            const pieces2 = createFakeBoardPieces();
            const pieces3 = createFakeBoardPieces();
            const move1 = createFakeMove();
            const move2 = createFakeMove();
            const move3 = createFakeMove();

            const pos1 = history.createMainPosition(pieces1, move1, "e4");
            const pos2 = history.createMainPosition(pieces2, move2, "d4");

            const pos1Variation = history.addVariationToPosition(
                pos1,
                pieces3,
                move3,
                "c4",
            );

            expect(pos1Variation).toBeDefined();
            expect(pos1.variations).toEqual([pos2, pos1Variation]);
            expect(history.plyCount).toBe(2); // plyCount only counts main line
        });

        it("should add a variation to the tail and update the tail", () => {
            const pieces1 = createFakeBoardPieces();
            const pieces2 = createFakeBoardPieces();
            const pieces3 = createFakeBoardPieces();
            const move1 = createFakeMove();
            const move2 = createFakeMove();
            const move3 = createFakeMove();

            const pos1 = history.createMainPosition(pieces1, move1, "e4");
            const pos2 = history.createMainPosition(pieces2, move2, "d4");

            // pos2 is tail which means it cannot possibly have a main variation
            // this new position will now be set as the main variation of pos2, and become tail
            const pos2Variation = history.addVariationToPosition(
                pos2,
                pieces3,
                move3,
                "c4",
            );

            expect(history.plyCount).toBe(3);
            expect([...history]).toEqual([pos1, pos2, pos2Variation]);
        });

        it("should return undefined if parent position is not found", () => {
            const fakePosition: Position = {
                positionId: "fake-id" as PositionId,
                pieces: createFakeBoardPieces(),
                move: createFakeMove(),
                san: "e4",
                variations: [],
            };

            const result = history.addVariationToPosition(
                fakePosition,
                createFakeBoardPieces(),
                createFakeMove(),
                "d4",
            );
            expect(result).toBeUndefined();
        });

        it("should return the existing variation if SAN already exists", () => {
            const pieces1 = createFakeBoardPieces();
            const pieces2 = createFakeBoardPieces();
            const move1 = createFakeMove();

            const pos = history.createMainPosition(pieces1, move1, "e4");
            const variation1 = history.addVariationToPosition(
                pos,
                pieces2,
                move1,
                "d4",
            );
            const variation2 = history.addVariationToPosition(
                pos,
                pieces2,
                move1,
                "d4",
            );

            expect(variation1).toBe(variation2);
        });
    });

    describe("iterator", () => {
        it("should iterate over the main line only", () => {
            const pieces1 = createFakeBoardPieces();
            const pieces2 = createFakeBoardPieces();
            const pieces3 = createFakeBoardPieces();
            const move1 = createFakeMove();
            const move2 = createFakeMove();
            const move3 = createFakeMove();

            const pos1 = history.createMainPosition(pieces1, move1, "e4");
            const pos2 = history.createMainPosition(pieces2, move2, "d4");
            history.addVariationToPosition(pos1, pieces3, move3, "c4");

            const positions = [...history];
            expect(positions).toEqual([pos1, pos2]);
        });
    });
});
