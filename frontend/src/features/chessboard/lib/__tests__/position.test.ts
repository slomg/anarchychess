import {
    createFakeBoardPieces,
    createFakeMove,
} from "@/lib/testUtils/fakers/chessboardFakers";

import PositionHistory from "../positionHistory";
import { Position } from "../position";
import BoardPieces from "../boardPieces";
import { Move } from "../types";

describe("Position", () => {
    let position: Position;
    let history: PositionHistory;

    let pieces: BoardPieces;
    let move: Move;
    let san: string;

    beforeEach(() => {
        pieces = createFakeBoardPieces();
        move = createFakeMove();
        san = "e4";

        position = new Position(pieces, move, san);
        history = new PositionHistory(position);
        history.registerPosition(position);
    });

    it("should store the given pieces", () => {
        expect(position.pieces).toEqual(pieces);
    });

    it("should store the given move", () => {
        expect(position.move).toEqual(move);
    });

    it("should store the given SAN", () => {
        expect(position.san).toBe(san);
    });

    it("should generate a unique positionId", () => {
        expect(new Position(pieces, move, san).positionId).not.toBe(
            new Position(pieces, move, san).positionId,
        );
    });

    describe("setNext", () => {
        it("should add a main branch position", () => {
            const main = new Position(
                createFakeBoardPieces(),
                createFakeMove(),
                "e5",
            );
            position.setNext(main, history);

            expect(position.next).toBe(main);
            expect(history.getByPositionId(main.positionId)).toBe(main);
        });

        it("should add a sub branch if its san is different from the main branch", () => {
            const main = new Position(
                createFakeBoardPieces(),
                createFakeMove(),
                "e5",
            );
            const sub = new Position(
                createFakeBoardPieces(),
                createFakeMove(),
                "d5",
            );

            position.setNext(main, history);
            position.setNext(main, history);
            position.setNext(sub, history);

            expect(position.next).toBe(main);
            expect(position.subBranches).toContain(sub);
            expect(history.getByPositionId(sub.positionId)).toBe(sub);
        });
    });

    describe("iterator", () => {
        it("should iterate over the main branch only", () => {
            const main1 = new Position(
                createFakeBoardPieces(),
                createFakeMove(),
                "e5",
            );
            const main2 = new Position(
                createFakeBoardPieces(),
                createFakeMove(),
                "Nf3",
            );
            main1.setNext(main2, history);

            position.setNext(main1, history);

            expect([...position]).toEqual([position, main1, main2]);
        });
    });

    describe("subBranches", () => {
        it("should return all sub branches", () => {
            const main = new Position(
                createFakeBoardPieces(),
                createFakeMove(),
                "e5",
            );
            const sub1 = new Position(
                createFakeBoardPieces(),
                createFakeMove(),
                "d5",
            );
            const sub2 = new Position(
                createFakeBoardPieces(),
                createFakeMove(),
                "c5",
            );

            position.setNext(main, history);
            position.setNext(sub1, history);
            position.setNext(sub2, history);

            expect(position.subBranches).toEqual([sub1, sub2]);
        });
    });
});
