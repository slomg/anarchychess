import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import {
    createFakeBoardPieces,
    createFakeMove,
} from "@/lib/testUtils/fakers/chessboardFakers";
import PositionHistory from "../positionHistory";
import { PositionId } from "../position";
import BoardPieces from "../boardPieces";
import { MoveKey } from "../types";

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
            expect(history.mainPlyCount).toBe(0);
            expect(history.totalPlyCount).toBe(0);
            expect(history.viewingPosition).toBeNull();
            expect([...history].length).toBe(0);
        });
    });

    describe("isViewingLatestPosition", () => {
        it("should return true when viewingPosition is the tail", () => {
            history.addNextPosition(createFakePositionProps());
            history.addNextPosition(createFakePositionProps());

            expect(history.isViewingLatestPosition).toBe(true);
        });

        it("should return false when viewingPosition is not the tail", () => {
            history.addNextPosition(createFakePositionProps());
            history.addNextPosition(createFakePositionProps());

            history.goToStart();
            expect(history.isViewingLatestPosition).toBe(false);
        });

        it("should return true when history is empty", () => {
            expect(history.isViewingLatestPosition).toBe(true);
        });
    });

    describe("goToPosition", () => {
        it("should return null if positionId does not exist", () => {
            expect(
                history.goToPosition("nonexistent-id" as PositionId),
            ).toEqual({ success: false, isOneStepForward: false });
        });

        it("should set the viewing position to the correct position", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            history.addNextPosition(createFakePositionProps());
            history.addNextPosition(createFakePositionProps());

            const result = history.goToPosition(pos1.positionId);

            expect(result.success).toBe(true);
            expect(result.isOneStepForward).toBe(false);
            expect(history.viewingPosition).toBe(pos1);
        });

        it("should detect one step forward when viewingPosition is null", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            history.addNextPosition(createFakePositionProps());
            history.goToStart();

            const result = history.goToPosition(pos1.positionId);

            expect(result.isOneStepForward).toBe(true);
        });

        it("should correctly detect one step forward along the main line", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());

            history.goToPosition(pos1.positionId);
            const result = history.goToPosition(pos2.positionId);
            expect(result.isOneStepForward).toBe(true);
        });

        it("should correctly detect one step forward along a sub variation", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            history.addNextPosition(createFakePositionProps());
            history.goToPosition(pos1.positionId);
            const pos1Variation = history.addNextPosition(
                createFakePositionProps(),
            );
            history.goToPosition(pos1.positionId);

            const result = history.goToPosition(pos1Variation.positionId);
            expect(result.isOneStepForward).toBe(true);
        });

        it("should return false for isOneStepForward when jumping multiple steps forward", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            history.addNextPosition(createFakePositionProps());
            const pos3 = history.addNextPosition(createFakePositionProps());

            history.goToPosition(pos1.positionId);
            const result = history.goToPosition(pos3.positionId);

            expect(result.isOneStepForward).toBe(false);
        });

        it("should return false for isOneStepForward when jumping backward", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());

            history.goToPosition(pos2.positionId);
            const result = history.goToPosition(pos1.positionId);

            expect(result.isOneStepForward).toBe(false);
        });
    });

    describe("goToStart", () => {
        it("should set viewingPosition to null", () => {
            history.addNextPosition(createFakePositionProps());
            history.addNextPosition(createFakePositionProps());

            expect(history.goToStart()).toBe(true);
            expect(history.viewingPosition).toBeNull();
        });

        it("should return false if we're already at the end", () => {
            history.addNextPosition(createFakePositionProps());
            history.addNextPosition(createFakePositionProps());

            expect(history.goToStart()).toBe(true);
            expect(history.goToStart()).toBe(false);
        });
    });

    describe("goToEnd", () => {
        it("should return isOneStepForward true when moving exactly one step to tail", () => {
            history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());
            const pos3 = history.addNextPosition(createFakePositionProps());

            history.goToPosition(pos2.positionId);

            const result = history.goToEnd();

            expect(result.success).toBe(true);
            expect(result.isOneStepForward).toBe(true);
            expect(history.viewingPosition).toBe(pos3);
        });

        it("should do nothing if history is empty", () => {
            history.goToStart();
            expect(history.viewingPosition).toBeNull();
        });

        it("should return false if already at the tail", () => {
            history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());

            history.goToEnd();
            const result = history.goToEnd();
            expect(result.success).toBe(false);
            expect(result.isOneStepForward).toBe(false);
            expect(history.viewingPosition).toBe(pos2);
        });

        it("should return isOneStepForward false when jumping multiple moves ahead", () => {
            history.addNextPosition(createFakePositionProps());
            history.addNextPosition(createFakePositionProps());
            const pos3 = history.addNextPosition(createFakePositionProps());
            history.goToStart();

            const result = history.goToEnd();

            expect(result.success).toBe(true);
            expect(result.isOneStepForward).toBe(false);
            expect(history.viewingPosition).toBe(pos3);
        });
    });

    describe("stepBackward", () => {
        it("should move viewingPosition backward along mainline", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());
            const pos3 = history.addNextPosition(createFakePositionProps());

            history.goToEnd();
            expect(history.viewingPosition).toBe(pos3);

            expect(history.stepBackward()).toBe(true);
            expect(history.viewingPosition).toBe(pos2);

            expect(history.stepBackward()).toBe(true);
            expect(history.viewingPosition).toBe(pos1);

            expect(history.stepBackward()).toBe(true);
            expect(history.viewingPosition).toBe(null);
        });

        it("should return false if history is empty", () => {
            expect(history.stepBackward()).toBe(false);
        });
    });

    describe("stepForward", () => {
        it("should move viewingPosition forward along mainline", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());
            const pos3 = history.addNextPosition(createFakePositionProps());

            history.goToPosition(pos1.positionId);
            expect(history.viewingPosition).toBe(pos1);

            expect(history.stepForward()).toBe(true);
            expect(history.viewingPosition).toBe(pos2);

            expect(history.stepForward()).toBe(true);
            expect(history.viewingPosition).toBe(pos3);

            // cannot go past tail
            expect(history.stepForward()).toBe(false);
            expect(history.viewingPosition).toBe(pos3);
        });

        it("should go to head if viewingPosition is null", () => {
            const head = history.addNextPosition(createFakePositionProps());
            history.goToStart();

            expect(history.stepForward()).toBe(true);
            expect(history.viewingPosition).toBe(head);
        });

        it("should return null if history is empty", () => {
            expect(history.stepForward()).toBe(false);
        });
    });

    describe("addNextPosition", () => {
        it("should create the first position and set it as head, tail, and viewing position", () => {
            const props = createFakePositionProps();
            const pos = history.addNextPosition(props);

            expect(pos).toEqual(expect.objectContaining(props));
            expect(history.mainPlyCount).toBe(1);
            expect(history.totalPlyCount).toBe(1);
            expect(history.viewingPosition).toBe(pos);
        });

        it("should append to the main line for next positions", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());

            expect(history.mainPlyCount).toBe(2);
            expect(history.totalPlyCount).toBe(2);
            expect([...history]).toEqual([pos1, pos2]);
            expect(history.viewingPosition).toBe(pos2);
        });

        it("should return the existing head variation if move key is the same as head when viewing root", () => {
            const props = createFakePositionProps();
            const pos = history.addNextPosition(props);

            history.goToStart();
            const duplicatePos = history.addNextPosition(props);

            expect(duplicatePos).toBe(pos);
            expect(history.viewingPosition).toBe(pos);
            expect([...history]).toEqual([pos]);
        });

        it("should add a new head variation if move key does not exist", () => {
            const head = history.addNextPosition(
                createFakePositionProps({
                    move: createFakeMove({ moveKey: "move1" as MoveKey }),
                }),
            );

            history.goToStart();
            const newProps = createFakePositionProps({
                move: createFakeMove({ moveKey: "move2" as MoveKey }),
            });
            const headVariation = history.addNextPosition(newProps);

            expect(headVariation).not.toBe(head);
            expect(history.viewingPosition).toBe(headVariation);
            expect(history.mainPlyCount).toBe(1);
            expect(history.totalPlyCount).toBe(2);

            const retrieved = history.goToPosition(headVariation.positionId);
            expect(retrieved.success).toBe(true);
            expect(history.viewingPosition).toBe(headVariation);
            expect(history.rootSubVariationByKey).toEqual(
                new Map([[headVariation.move.moveKey, headVariation]]),
            );

            history.goToStart();
            expect(history.stepForward()).toBe(true);
            expect(history.viewingPosition).toBe(head);
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
            expect(history.mainPlyCount).toBe(2);
            expect(history.totalPlyCount).toBe(3);
        });

        it("should return the existing variation if move key already exists as main variation", () => {
            const pos = history.addNextPosition(createFakePositionProps());

            const variation1 = history.addNextPosition(
                createFakePositionProps({
                    move: createFakeMove({ moveKey: "move1" as MoveKey }),
                }),
            );

            history.goToPosition(pos.positionId);
            const variation2 = history.addNextPosition(
                createFakePositionProps({
                    move: createFakeMove({ moveKey: "move1" as MoveKey }),
                }),
            );

            expect(variation1).toBe(variation2);
        });

        it("should return the existing variation if move key already exists as sub variation", () => {
            const pos1 = history.addNextPosition(createFakePositionProps());
            const pos2 = history.addNextPosition(createFakePositionProps());

            history.goToPosition(pos1.positionId);
            const variation1 = history.addNextPosition(
                createFakePositionProps({
                    move: createFakeMove({ moveKey: "move1" as MoveKey }),
                }),
            );

            history.goToPosition(pos1.positionId);
            const variation2 = history.addNextPosition(
                createFakePositionProps({
                    move: createFakeMove({ moveKey: "move1" as MoveKey }),
                }),
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
