import {
    createFakeMove,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";

import { TransientBoardEffectType } from "../../stores/boardEffectsSlice";
import { simulateMove, simulateMoveAnimated } from "../simulateMove";
import { AnimationStep, IntermediateSquare } from "../types";
import { PieceType, SpecialMoveType } from "@/lib/apiClient";
import { logicalPoint } from "@/features/point/pointUtils";
import BoardPieces from "../boardPieces";

describe("simulateMove", () => {
    it("should return a new BoardPieces instance that reflects the result of playMove", () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const pieces = BoardPieces.fromPieces(piece);

        const move = createFakeMove({
            from: piece.position,
            to: logicalPoint({ x: 2, y: 2 }),
        });

        const result = simulateMove(pieces, move);

        expect(result.newPieces).not.toBe(pieces);
        expect(result.newPieces.getByPosition(move.to)?.id).toEqual(piece.id);

        // base is unchanged
        expect(pieces.getByPosition(piece.position)?.id).toEqual(piece.id);
        expect(pieces.getByPosition(move.to)).toBeUndefined();

        const expectedPieces = new BoardPieces(pieces);
        expectedPieces.playMove(move);
        const expectedResult: AnimationStep = {
            newPieces: expectedPieces,
            movedPieceIds: [piece.id],
            moveBounds: { from: move.from, to: move.to },
            isCapture: false,
            isPromotion: false,
            specialType: SpecialMoveType.NONE,
        };
        expect(result).toEqual(expectedResult);
    });

    it("should set initialSpawnPositions only when spawning pieces", () => {
        const newPiece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });

        const piece = createFakePiece({
            position: logicalPoint({ x: 4, y: 4 }),
        });
        const pieces = BoardPieces.fromPieces(piece);

        const move = createFakeMove({
            from: piece.position,
            to: logicalPoint({ x: 5, y: 5 }),
            pieceSpawns: [newPiece],
        });

        const result = simulateMove(pieces, move);

        const expectedInitialSpawnPositions = new BoardPieces(pieces);
        expectedInitialSpawnPositions.addAt(newPiece, move.from);
        expect(result.initialSpawnPositions).toEqual(
            expectedInitialSpawnPositions,
        );

        expect(result.movedPieceIds).toEqual([piece.id, newPiece.id]);
    });

    it("should correctly set isPromotion", () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 5, y: 5 }),
        });
        const pieces = BoardPieces.fromPieces(piece);

        const move = createFakeMove({
            from: piece.position,
            to: logicalPoint({ x: 5, y: 6 }),
            promotesTo: PieceType.QUEEN,
        });

        const result = simulateMove(pieces, move);
        expect(result.isPromotion).toEqual(true);
    });

    it("should set isCapture to true when there are captures", () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const capturedPiece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const pieces = BoardPieces.fromPieces(piece, capturedPiece);

        const move = createFakeMove({
            from: piece.position,
            to: capturedPiece.position,
            captures: [capturedPiece.position],
        });

        const result = simulateMove(pieces, move);

        expect(result.isCapture).toEqual(true);
    });

    it("should set isCapture to false when captures are already represented in intermediates", () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const capturedPiece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const pieces = BoardPieces.fromPieces(piece, capturedPiece);

        const move = createFakeMove({
            from: piece.position,
            to: capturedPiece.position,
            captures: [capturedPiece.position],
            intermediates: [
                { position: capturedPiece.position, isCapture: true },
            ],
        });

        const result = simulateMove(pieces, move);

        expect(result.isCapture).toEqual(false);
    });

    it("should not set isCapture to true if captures come from overtime", () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const overtimePiece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const pieces = BoardPieces.fromPieces(piece, overtimePiece);
        const move = createFakeMove({
            from: piece.position,
            to: overtimePiece.position,
            overtimeRemovals: [overtimePiece.position],
        });

        const result = simulateMove(pieces, move);

        expect(result.isCapture).toEqual(false);
    });

    it("should forward specialMoveType", () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 7, y: 7 }),
        });
        const pieces = BoardPieces.fromPieces(piece);

        const move = createFakeMove({
            from: piece.position,
            to: logicalPoint({ x: 7, y: 6 }),
            specialType: SpecialMoveType.EN_PASSANT,
        });

        const result = simulateMove(pieces, move);

        expect(result.specialType).toEqual(move.specialType);
    });

    it("should correctly return fadedPieces for overtime removals", () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const overtimePiece1 = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const overtimePiece2 = createFakePiece({
            position: logicalPoint({ x: 2, y: 2 }),
        });
        const capturePiece = createFakePiece({
            position: logicalPoint({ x: 3, y: 3 }),
        });

        const pieces = BoardPieces.fromPieces(
            piece,
            overtimePiece1,
            overtimePiece2,
        );

        const move = createFakeMove({
            from: piece.position,
            to: logicalPoint({ x: 3, y: 3 }),
            overtimeRemovals: [
                overtimePiece1.position,
                overtimePiece2.position,
            ],
            captures: [capturePiece.position],
        });

        const result = simulateMove(pieces, move);

        expect(result.fadedPieces).toEqual(
            new Map([
                [overtimePiece1.id, overtimePiece1],
                [overtimePiece2.id, overtimePiece2],
            ]),
        );
        expect(result.newPieces.getByPosition(move.to)?.id).toEqual(piece.id);
    });
});

describe("simulateMoveAnimated", () => {
    it("should return intermediate positions and final move", () => {
        const movingPiece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const pieces = BoardPieces.fromPieces(movingPiece);

        const intermediates: IntermediateSquare[] = [
            { position: logicalPoint({ x: 1, y: 1 }), isCapture: false },
            { position: logicalPoint({ x: 2, y: 2 }), isCapture: true },
        ];
        const move = createFakeMove({
            from: movingPiece.position,
            to: logicalPoint({ x: 3, y: 3 }),
            intermediates,
            captures: [],
        });

        const resultSteps = simulateMoveAnimated(pieces, move);

        const expected1 = new BoardPieces(pieces);
        expected1.movePiece(movingPiece.id, intermediates[0].position);
        expect(resultSteps[0].newPieces).toEqual(expected1);
        expect(resultSteps[0].isCapture).toBe(intermediates[0].isCapture);

        const expected2 = new BoardPieces(pieces);
        expected2.movePiece(movingPiece.id, intermediates[1].position);
        expect(resultSteps[1].newPieces).toEqual(expected2);
        expect(resultSteps[1].isCapture).toBe(intermediates[1].isCapture);

        const expectedFinal = new BoardPieces(pieces);
        expectedFinal.movePiece(movingPiece.id, move.to);
        expect(resultSteps[2].newPieces).toEqual(expectedFinal);

        resultSteps.forEach((r) => {
            expect(r.movedPieceIds).toEqual([movingPiece.id]);
        });
    });

    it("should include removedPieces from the final move", () => {
        const movingPiece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const capturePiece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const pieces = BoardPieces.fromPieces(movingPiece, capturePiece);

        const intermediates: IntermediateSquare[] = [
            { position: logicalPoint({ x: 0, y: 1 }), isCapture: false },
            { position: logicalPoint({ x: 1, y: 0 }), isCapture: false },
        ];
        const move = createFakeMove({
            from: movingPiece.position,
            to: capturePiece.position,
            intermediates,
            captures: [capturePiece.position],
        });

        const resultSteps = simulateMoveAnimated(pieces, move);

        const expectedFadedPieces = new Map([[capturePiece.id, capturePiece]]);
        expect(
            resultSteps[0].newPieces.getById(capturePiece.id),
        ).not.toBeDefined();
        expect(resultSteps[0].fadedPieces).toEqual(expectedFadedPieces);

        expect(
            resultSteps[1].newPieces.getById(capturePiece.id),
        ).not.toBeDefined();
        expect(resultSteps[1].fadedPieces).toEqual(expectedFadedPieces);

        expect(
            resultSteps[2].newPieces.getById(capturePiece.id),
        ).not.toBeDefined();
        expect(resultSteps[2].fadedPieces).toBeUndefined();
    });

    it("should skip intermediate steps when skipAlreadyPlayedLocally is true", () => {
        const movingPiece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const pieces = BoardPieces.fromPieces(movingPiece);

        const intermediates: IntermediateSquare[] = [
            { position: logicalPoint({ x: 1, y: 0 }), isCapture: false },
            { position: logicalPoint({ x: 2, y: 0 }), isCapture: false },
        ];

        const move = createFakeMove({
            from: movingPiece.position,
            to: logicalPoint({ x: 3, y: 0 }),
            intermediates,
        });

        const resultSteps = simulateMoveAnimated(pieces, move, {
            skipAlreadyPlayedLocally: true,
        });

        expect(resultSteps).toHaveLength(1);

        const finalStep = resultSteps[0];
        expect(finalStep.newPieces.getByPosition(move.to)?.id).toEqual(
            movingPiece.id,
        );
        expect(finalStep.movedPieceIds).toEqual([movingPiece.id]);
    });

    it("should simulate throw moves correctly", () => {
        const movingPiece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const pieces = BoardPieces.fromPieces(movingPiece);

        const move = createFakeMove({
            from: movingPiece.position,
            to: logicalPoint({ x: 4, y: 4 }),
            specialType: SpecialMoveType.THROW,
        });

        const resultSteps = simulateMoveAnimated(pieces, move);

        expect(resultSteps).toHaveLength(2);

        const expectedThrowPieces = new BoardPieces(pieces);
        expectedThrowPieces.removeFrom(move.from);
        expect(resultSteps[0]).toEqual<AnimationStep>({
            newPieces: expectedThrowPieces,
            movedPieceIds: [],
            boardEffect: {
                type: TransientBoardEffectType.PAWN_THROW,
                from: move.from,
                to: move.to,
                color: movingPiece.color,
            },
            disableStepDelay: true,
        });

        const expectedFinalPieces = new BoardPieces(pieces);
        expectedFinalPieces.movePiece(movingPiece.id, move.to);
        expect(resultSteps[1].newPieces).toEqual(expectedFinalPieces);
        expect(resultSteps[1].movedPieceIds).toEqual([movingPiece.id]);
        expect(resultSteps[1].boardEffect).toBeUndefined();
    });
});
