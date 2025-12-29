import {
    createFakeBoardPieces,
    createFakeMove,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { Position, RootPosition } from "../position";
import PositionHistory from "../positionHistory";
import { PositionId } from "../types";

describe("PositionHistory", () => {
    let root: RootPosition;
    let history: PositionHistory;

    beforeEach(() => {
        root = new RootPosition(createFakeBoardPieces());
        history = new PositionHistory(root);
    });

    it("should store the root position", () => {
        expect(history.root).toBe(root);
    });

    it("should register the root position by default", () => {
        expect(history.getByPositionId(root.positionId)).toBe(root);
    });

    it("should register new positions", () => {
        const position = new Position(
            createFakeBoardPieces(),
            createFakeMove(),
            "e4",
        );

        history.registerPosition(position);

        expect(history.getByPositionId(position.positionId)).toBe(position);
    });

    it("should return undefined for unknown position IDs", () => {
        const unknownId = "non-existent-id" as PositionId;
        expect(history.getByPositionId(unknownId)).toBeUndefined();
    });
});
