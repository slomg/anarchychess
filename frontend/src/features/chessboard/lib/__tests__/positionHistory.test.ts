import {
    createFakeBoardPieces,
    createFakeMove,
} from "@/lib/testUtils/fakers/chessboardFakers";

import PositionHistory from "../positionHistory";
import BoardPieces from "../boardPieces";
import { PositionId } from "../types";

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
            expect(history.viewingPosition).toBeNull();
        });
    });

    describe("goToPosition", () => {
        it("should return false if positionId does not exist", () => {
            expect(history.goToPosition("nonexistent-id" as PositionId)).toBe(
                false,
            );
        });

        it("should set the viewing position to the correct position after it is added", () => {
            const pieces = createFakeBoardPieces();
            const move = createFakeMove();
            const pos = history.addNextPosition(pieces, move, "e4");

            expect(history.goToPosition(pos.positionId)).toBe(true);
            expect(history.viewingPosition).toBe(pos);
        });
    });

    describe("addNextPosition", () => {
        it("should create the first position and set it as head, tail, and viewing position", () => {
            const pieces = createFakeBoardPieces();
            const move = createFakeMove();
            const pos = history.addNextPosition(pieces, move, "e4");

            expect(pos.pieces).toBe(pieces);
            expect(pos.move).toBe(move);
            expect(pos.san).toBe("e4");
            expect(history.plyCount).toBe(1);
            expect(history.viewingPosition).toBe(pos);
        });

        it("should append to the main line for subsequent positions", () => {
            const pieces1 = createFakeBoardPieces();
            const pieces2 = createFakeBoardPieces();
            const move1 = createFakeMove();
            const move2 = createFakeMove();

            const pos1 = history.addNextPosition(pieces1, move1, "e4");
            const pos2 = history.addNextPosition(pieces2, move2, "d4");

            expect(history.plyCount).toBe(2);
            expect([...history]).toEqual([pos1, pos2]);
            expect(history.viewingPosition).toBe(pos2);
        });

        it("should add a variation when viewing a non tail position", () => {
            const pieces1 = createFakeBoardPieces();
            const pieces2 = createFakeBoardPieces();
            const pieces3 = createFakeBoardPieces();
            const move1 = createFakeMove();
            const move2 = createFakeMove();
            const move3 = createFakeMove();

            const pos1 = history.addNextPosition(pieces1, move1, "e4");
            const pos2 = history.addNextPosition(pieces2, move2, "d4");

            // go back to first position to add a variation
            history.goToPosition(pos1.positionId);
            const pos1Variation = history.addNextPosition(pieces3, move3, "c4");

            expect(pos1Variation).toBeDefined();
            expect(pos1.variations).toEqual([pos2, pos1Variation]);
            expect(history.plyCount).toBe(2); // only main line counted
        });

        it("should return the existing variation if SAN already exists as main variation", () => {
            const pieces1 = createFakeBoardPieces();
            const pieces2 = createFakeBoardPieces();
            const move1 = createFakeMove();

            const pos = history.addNextPosition(pieces1, move1, "e4");

            const variation1 = history.addNextPosition(pieces2, move1, "d4");
            history.goToPosition(pos.positionId);
            const variation2 = history.addNextPosition(pieces2, move1, "d4");

            expect(variation1).toBe(variation2);
        });

        it("should return the existing variation if SAN already exists as sub variation", () => {
            const pieces1 = createFakeBoardPieces();
            const pieces2 = createFakeBoardPieces();
            const pieces3 = createFakeBoardPieces();
            const move1 = createFakeMove();

            const pos1 = history.addNextPosition(pieces1, move1, "e4");
            const pos2 = history.addNextPosition(pieces2, move1, "d4");

            history.goToPosition(pos1.positionId);
            const variation1 = history.addNextPosition(pieces3, move1, "c4");
            history.goToPosition(pos1.positionId);
            const variation2 = history.addNextPosition(pieces3, move1, "c4");

            expect(variation1).toBe(variation2);
            expect(pos2).not.toBe(variation1);
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

            const pos1 = history.addNextPosition(pieces1, move1, "e4");
            const pos2 = history.addNextPosition(pieces2, move2, "d4");

            // go back to first position to add a variation
            history.goToPosition(pos1.positionId);
            history.addNextPosition(pieces3, move3, "c4");

            const positions = [...history];
            expect(positions).toEqual([pos1, pos2]);
        });
    });
});
