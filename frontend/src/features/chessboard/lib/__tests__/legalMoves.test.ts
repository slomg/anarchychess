import { createFakeMove } from "@/lib/testUtils/fakers/chessboardFakers";
import { logicalPoint, pointToStr } from "@/features/point/pointUtils";
import { ForcedMovePriority } from "@/lib/apiClient";
import { IntermediateSquare } from "../types";
import LegalMoves from "../legalMoves";

describe("LegalMoves", () => {
    let legalMoves: LegalMoves;

    beforeEach(() => {
        legalMoves = new LegalMoves();
    });

    describe("constructor", () => {
        it("should initialize empty byOrigin map", () => {
            expect(legalMoves.byOrigin.size).toBe(0);
            expect(legalMoves.hasForcedMoves).toBe(false);
            expect(legalMoves.emphasizedSquares).toEqual([]);
        });

        it("should add moves if provided", () => {
            const move1 = createFakeMove({
                from: logicalPoint({ x: 1, y: 1 }),
            });
            const move2 = createFakeMove({
                from: logicalPoint({ x: 2, y: 1 }),
            });

            const legalMoves = new LegalMoves([move1, move2]);

            expect([...legalMoves.getFromOrigin(move1.from)]).toHaveLength(1);
            expect([...legalMoves.getFromOrigin(move2.from)]).toHaveLength(1);
            expect(legalMoves.hasForcedMoves).toBe(false);
            expect(legalMoves.emphasizedSquares).toEqual([]);
        });
    });

    describe("addMove", () => {
        it("should add a move to byOrigin", () => {
            const move = createFakeMove();

            legalMoves.addMove(move);

            const node = legalMoves.getDirectNode(move.from, move.to);
            expect(node).not.toBeNullable();
            expect(node?.terminalMoves).toContain(move);
            expect(legalMoves.byOrigin.has(pointToStr(move.from))).toBe(true);
            expect(legalMoves.hasMovesDirectlyFromTo(move.from, move.to)).toBe(
                true,
            );
        });

        it("should mark hasForcedMoves if move is forced", () => {
            const move = createFakeMove({
                forcedPriority: ForcedMovePriority.EN_PASSANT,
            });
            legalMoves.addMove(move);

            expect(legalMoves.hasForcedMoves).toBe(true);
        });

        it("should add emphasized squares if move has emphasizeSquare", () => {
            const emphasizedMove = createFakeMove({ emphasizeSquare: true });
            legalMoves.addMove(emphasizedMove);

            expect(legalMoves.emphasizedSquares).toContain(emphasizedMove.from);
        });

        it("should handle adding multiple moves with intermediates", () => {
            const move1 = createFakeMove({
                from: logicalPoint({ x: 1, y: 1 }),
                to: logicalPoint({ x: 2, y: 2 }),
            });
            legalMoves.addMove(move1);

            let node = legalMoves.getDirectNode(move1.from, move1.to);
            expect(node).not.toBeNullable();
            expect(node?.terminalMoves).toEqual([move1]);
            expect(node?.nextIntermediates.size).toBe(0);

            const move2 = createFakeMove({
                from: logicalPoint({ x: 1, y: 1 }),
                to: logicalPoint({ x: 4, y: 4 }),
                intermediates: [
                    {
                        position: logicalPoint({ x: 3, y: 3 }),
                        isCapture: false,
                    },
                    {
                        position: logicalPoint({ x: 2, y: 3 }),
                        isCapture: false,
                    },
                ],
            });
            legalMoves.addMove(move2);

            node = legalMoves.getDirectNode(move1.from, move1.to);
            expect(node).not.toBeNullable();
            expect(node?.terminalMoves).toEqual([move1]);

            const firstIntermediate = legalMoves.getDirectNode(
                move2.from,
                logicalPoint({
                    x: 3,
                    y: 3,
                }),
            );
            expect(firstIntermediate).not.toBeNullable();
            expect(firstIntermediate?.terminalMoves.length).toBe(0);

            const secondIntermediate = firstIntermediate?.nextIntermediates.get(
                pointToStr({ x: 2, y: 3 }),
            );
            expect(secondIntermediate).not.toBeNullable();
            expect(secondIntermediate?.terminalMoves.length).toBe(0);

            const destination = secondIntermediate?.nextIntermediates.get(
                pointToStr(move2.to),
            );
            expect(destination).not.toBeNullable();
            expect(destination?.terminalMoves).toEqual([move2]);
        });

        it("should add moves for each trigger in without the main destination", () => {
            const triggerPoint1 = logicalPoint({ x: 3, y: 3 });
            const triggerPoint2 = logicalPoint({ x: 4, y: 4 });
            const mainDestination = logicalPoint({ x: 5, y: 5 });

            const moveWithTriggers = createFakeMove({
                to: mainDestination,
                triggers: [triggerPoint1, triggerPoint2],
            });

            legalMoves.addMove(moveWithTriggers);

            // original destination node
            const mainNode = legalMoves.getDirectNode(
                moveWithTriggers.from,
                mainDestination,
            );
            expect(mainNode).toBeNullable();

            // trigger nodes
            const triggerNode1 = legalMoves.getDirectNode(
                moveWithTriggers.from,
                triggerPoint1,
            );
            expect(triggerNode1).not.toBeNullable();
            expect(triggerNode1?.terminalMoves).toContain(moveWithTriggers);

            const triggerNode2 = legalMoves.getDirectNode(
                moveWithTriggers.from,
                triggerPoint2,
            );
            expect(triggerNode2).not.toBeNullable();
            expect(triggerNode2?.terminalMoves).toContain(moveWithTriggers);
        });

        it("should add moves with one intermediate for main destination and each trigger", () => {
            const intermediate: IntermediateSquare = {
                position: logicalPoint({ x: 2, y: 2 }),
                isCapture: false,
            };
            const mainDestination = logicalPoint({ x: 5, y: 5 });
            const triggerPoint1 = logicalPoint({ x: 3, y: 3 });
            const triggerPoint2 = logicalPoint({ x: 4, y: 4 });

            const move = createFakeMove({
                to: mainDestination,
                intermediates: [intermediate],
                triggers: [triggerPoint1, triggerPoint2],
            });

            legalMoves.addMove(move);

            const intermediateNode = legalMoves.getDirectNode(
                move.from,
                intermediate.position,
            );
            expect(intermediateNode).not.toBeNullable();

            const intermediateTerminalNode =
                intermediateNode?.nextIntermediates.get(
                    pointToStr(mainDestination),
                );
            expect(intermediateTerminalNode).toBeNullable();

            // trigger nodes
            const triggerIntermediateNode1 = legalMoves
                .getDirectNode(move.from, intermediate.position)
                ?.nextIntermediates.get(pointToStr(triggerPoint1));
            expect(triggerIntermediateNode1).not.toBeNullable();
            expect(triggerIntermediateNode1?.terminalMoves).toContain(move);

            const triggerIntermediateNode2 = legalMoves
                .getDirectNode(move.from, intermediate.position)
                ?.nextIntermediates.get(pointToStr(triggerPoint2));
            expect(triggerIntermediateNode2).not.toBeNullable();
            expect(triggerIntermediateNode2?.terminalMoves).toContain(move);
        });
    });

    describe("getDirectNode", () => {
        it("should return null if no move exists", () => {
            expect(
                legalMoves.getDirectNode(
                    logicalPoint({ x: 0, y: 0 }),
                    logicalPoint({ x: 1, y: 1 }),
                ),
            ).toBeNull();
        });

        it("should return the node if it exists", () => {
            const move = createFakeMove();
            legalMoves.addMove(move);

            const node = legalMoves.getDirectNode(move.from, move.to);

            expect(node).not.toBeNullable();
            expect(node?.from).toEqual(move.from);
            expect(node?.at).toEqual(move.to);
            expect(node?.terminalMoves).toEqual([move]);
        });
    });

    describe("hasMovesDirectlyFromTo", () => {
        it("should return true if a move exists", () => {
            const move = createFakeMove();

            legalMoves.addMove(move);
            expect(legalMoves.hasMovesDirectlyFromTo(move.from, move.to)).toBe(
                true,
            );
        });

        it("should return false if no move exists", () => {
            const move = createFakeMove({ to: logicalPoint({ x: 2, y: 2 }) });

            expect(
                legalMoves.hasMovesDirectlyFromTo(
                    move.from,
                    logicalPoint({ x: 3, y: 3 }),
                ),
            ).toBe(false);
        });
    });

    describe("getFromOrigin", () => {
        it("should return an empty iterator if no moves exist", () => {
            const result = [
                ...legalMoves.getFromOrigin(logicalPoint({ x: 0, y: 0 })),
            ];
            expect(result).toEqual([]);
        });

        it("should return all moves from a given origin", () => {
            const move1 = createFakeMove({ to: logicalPoint({ x: 2, y: 2 }) });
            const move2 = createFakeMove({
                from: move1.from,
                to: logicalPoint({ x: 3, y: 3 }),
            });
            legalMoves.addMove(move1);
            legalMoves.addMove(move2);

            const movesFromOrigin = [...legalMoves.getFromOrigin(move1.from)];
            expect(movesFromOrigin).toHaveLength(2);
            expect(movesFromOrigin.map((n) => n.at)).toEqual([
                move1.to,
                move2.to,
            ]);
        });
    });
});
