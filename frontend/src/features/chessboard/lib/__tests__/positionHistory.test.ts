import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import { createFakeBoardPieces } from "@/lib/testUtils/fakers/chessboardFakers";
import PositionHistory, { PositionId } from "../positionHistory";
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
            expect(history.viewingPosition).toBeNull();
            expect([...history].length).toBe(0);
        });
    });

    describe("goToPosition", () => {
        it("should return null if positionId does not exist", () => {
            expect(
                history.goToPosition("nonexistent-id" as PositionId),
            ).toBeNull();
        });

        it("should set the viewing position to the correct position", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            history.addNextPosition(createFakePositionProps());
            history.addNextPosition(createFakePositionProps());

            const result = history.goToPosition(pos1.positionId);

            expect(result).not.toBeNull();
            expect(result?.position).toBe(pos1);
            expect(result?.isOneStepForward).toBe(false);
            expect(history.viewingPosition).toBe(pos1);
        });

        it("should detect one step forward when viewingPosition is null", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            history.addNextPosition(createFakePositionProps());
            history.goToStart();

            const result = history.goToPosition(pos1.positionId);

            expect(result!.isOneStepForward).toBe(true);
        });

        it("should correctly detect one step forward along the main line", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());

            history.goToPosition(pos1.positionId);
            const result = history.goToPosition(pos2.positionId);
            expect(result!.isOneStepForward).toBe(true);
        });

        it("should correctly detect one step forward along a sub variation", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            history.addNextPosition(createFakePositionProps());
            history.goToPosition(pos1.positionId);
            const pos1Variation = history.addNextPosition(
                createFakePositionProps({ san: "c4" }),
            );
            history.goToPosition(pos1.positionId);

            const result = history.goToPosition(pos1Variation.positionId);
            expect(result!.isOneStepForward).toBe(true);
        });

        it("should return false for isOneStepForward when jumping multiple steps forward", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            history.addNextPosition(createFakePositionProps());
            const pos3 = history.addNextPosition(createFakePositionProps());

            history.goToPosition(pos1.positionId);
            const result = history.goToPosition(pos3.positionId);

            expect(result!.isOneStepForward).toBe(false);
        });

        it("should return false for isOneStepForward when jumping backward", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());

            history.goToPosition(pos2.positionId);
            const result = history.goToPosition(pos1.positionId);

            expect(result!.isOneStepForward).toBe(false);
        });
    });

    describe("goToStart", () => {
        it("should set viewingPosition to null", () => {
            history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());

            history.goToEnd();
            expect(history.viewingPosition).toBe(pos2);

            history.goToStart();
            expect(history.viewingPosition).toBeNull();
        });
    });

    describe("goToEnd", () => {
        it("should set viewingPosition to tail", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());

            history.goToStart();
            expect(history.viewingPosition).toBe(pos1);

            history.goToEnd();
            expect(history.viewingPosition).toBe(pos2);
        });

        it("should do nothing if history is empty", () => {
            history.goToStart();
            expect(history.viewingPosition).toBeNull();
        });
    });

    describe("stepBackward", () => {
        it("should move viewingPosition backward along mainline", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());
            const pos3 = history.addNextPosition(createFakePositionProps());

            history.goToEnd();
            expect(history.viewingPosition).toBe(pos3);

            expect(history.stepBackward()).toBe(pos2);
            expect(history.viewingPosition).toBe(pos2);

            expect(history.stepBackward()).toBe(pos1);
            expect(history.viewingPosition).toBe(pos1);

            // cannot go past head
            expect(history.stepBackward()).toBeNull();
            expect(history.viewingPosition).toBe(pos1);
        });

        it("should return null if history is empty", () => {
            expect(history.stepBackward()).toBeNull();
        });
    });

    describe("stepForward", () => {
        it("should move viewingPosition forward along mainline", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());
            const pos3 = history.addNextPosition(createFakePositionProps());

            history.goToStart();
            expect(history.viewingPosition).toBe(pos1);

            expect(history.stepForward()).toBe(pos2);
            expect(history.viewingPosition).toBe(pos2);

            expect(history.stepForward()).toBe(pos3);
            expect(history.viewingPosition).toBe(pos3);

            // cannot go past tail
            expect(history.stepForward()).toBeNull();
            expect(history.viewingPosition).toBe(pos3);
        });

        it("should return null if history is empty", () => {
            expect(history.stepForward()).toBeNull();
        });
    });

    describe("addNextPosition", () => {
        it("should create the first position and set it as head, tail, and viewing position", () => {
            const props = createFakePositionProps();
            const pos = history.addNextPosition(props);

            expect(pos.pieces).toBe(props.pieces);
            expect(pos.move).toBe(props.move);
            expect(pos.san).toBe(props.san);
            expect(history.plyCount).toBe(1);
            expect(history.viewingPosition).toBe(pos);
        });

        it("should append to the main line for subsequent positions", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());

            expect(history.plyCount).toBe(2);
            expect([...history]).toEqual([pos1, pos2]);
            expect(history.viewingPosition).toBe(pos2);
        });

        it("should add a variation when viewing a non tail position", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());

            // go back to first position to add a variation
            history.goToPosition(pos1.positionId);
            const pos1Variation = history.addNextPosition(
                createFakePositionProps(),
            );

            expect(pos1Variation).toBeDefined();
            expect(pos1.variations).toEqual([pos2, pos1Variation]);
            expect(history.plyCount).toBe(2); // only main line counted
        });

        it("should return the existing variation if SAN already exists as main variation", () => {
            const pos = history.addNextPosition(createFakePositionProps());

            const variation1 = history.addNextPosition(
                createFakePositionProps({ san: "d4" }),
            );

            history.goToPosition(pos.positionId);
            const variation2 = history.addNextPosition(
                createFakePositionProps({ san: "d4" }),
            );

            expect(variation1).toBe(variation2);
        });

        it("should return the existing variation if SAN already exists as sub variation", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());

            history.goToPosition(pos1.positionId);
            const variation1 = history.addNextPosition(
                createFakePositionProps({ san: "c4" }),
            );

            history.goToPosition(pos1.positionId);
            const variation2 = history.addNextPosition(
                createFakePositionProps({ san: "c4" }),
            );

            expect(variation1).toBe(variation2);
            expect(pos2).not.toBe(variation1);
        });
    });

    describe("iterator", () => {
        it("should iterate over the main line only", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());

            // go back to first position to add a variation
            history.goToPosition(pos1.positionId);
            history.addNextPosition(createFakePositionProps());

            const positions = [...history];
            expect(positions).toEqual([pos1, pos2]);
        });
    });
});
