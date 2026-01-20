import { StoreApi } from "zustand";

import {
    createFakeMove,
    createFakeBoardPieces,
} from "@/lib/testUtils/fakers/chessboardFakers";

import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { logicalPoint, sortPoints } from "@/features/point/pointUtils";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";
import { LogicalPoint } from "@/features/point/types";
import LegalMoves from "../../lib/legalMoves";
import { PieceID } from "../../lib/types";

describe("IntermediateSlice", () => {
    let store: StoreApi<ChessboardStore>;

    const animatePieceMock = vi.fn();
    const pieceId: PieceID = "test piece" as PieceID;
    const pieces = createFakeBoardPieces(1);

    beforeEach(() => {
        store = createChessboardStore();
        store.setState({ animatePiece: animatePieceMock });
        vi.useFakeTimers({ shouldAdvanceTime: true });
    });

    function expectNextIntermediates(...expected: LogicalPoint[]) {
        const actual = store.getState().nextIntermediates;
        expect(sortPoints(actual)).toEqual(sortPoints(expected));
    }

    it("should return terminal moves if ther are no intermediates", async () => {
        const move1 = createFakeMove({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 2, y: 2 }),
        });
        const move2 = createFakeMove({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 3, y: 3 }),
        });
        const legalMoves = new LegalMoves([move1, move1, move2]);

        const result = await store
            .getState()
            .disambiguateIntermediates(
                move1.to,
                legalMoves.getDirectNode(move1.from, move1.to)!,
                pieceId,
                pieces,
            );

        expect(result).toEqual([move1, move1]);
        expect(animatePieceMock).not.toHaveBeenCalled();
    });

    it("should return terminalMoves if we resolve intermediates to dest", async () => {
        const move1 = createFakeMove({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 1, y: 1 }),
        });
        const intermediateMove1 = createFakeMove({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 2, y: 2 }),
            intermediates: [
                { position: logicalPoint({ x: 1, y: 1 }), isCapture: false },
            ],
        });
        const intermediateMove2 = createFakeMove({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 4, y: 4 }),
            intermediates: [
                { position: logicalPoint({ x: 1, y: 1 }), isCapture: false },
                { position: logicalPoint({ x: 3, y: 3 }), isCapture: false },
            ],
        });
        const legalMoves = new LegalMoves([
            move1,
            intermediateMove1,
            intermediateMove2,
        ]);

        const promise = store
            .getState()
            .disambiguateIntermediates(
                move1.to,
                legalMoves.getDirectNode(move1.from, move1.to)!,
                pieceId,
                pieces,
            );

        expectNextIntermediates(
            logicalPoint({ x: 1, y: 1 }),
            logicalPoint({ x: 2, y: 2 }),
            logicalPoint({ x: 3, y: 3 }),
        );
        const resolve = store.getState().resolveNextIntermediate!;
        resolve(logicalPoint({ x: 1, y: 1 }));

        const result = await promise;
        expect(result).toEqual([move1]);
    });

    it("should loop until terminal moves are found", async () => {
        const move1 = createFakeMove({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 1, y: 1 }),
        });
        const intermediateMove1 = createFakeMove({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 2, y: 2 }),
            intermediates: [
                { position: logicalPoint({ x: 1, y: 1 }), isCapture: false },
            ],
        });
        const intermediateMove2 = createFakeMove({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 3, y: 3 }),
            intermediates: [
                { position: logicalPoint({ x: 1, y: 1 }), isCapture: false },
                { position: logicalPoint({ x: 2, y: 2 }), isCapture: false },
            ],
        });
        const legalMoves = new LegalMoves([
            move1,
            intermediateMove1,
            intermediateMove2,
        ]);

        const promise = store
            .getState()
            .disambiguateIntermediates(
                move1.to,
                legalMoves.getDirectNode(move1.from, move1.to)!,
                pieceId,
                pieces,
            );

        store.getState().resolveNextIntermediate!(logicalPoint({ x: 2, y: 2 }));

        await flushMicrotasks();

        expectNextIntermediates(
            logicalPoint({ x: 2, y: 2 }),
            logicalPoint({ x: 3, y: 3 }),
        );

        store.getState().resolveNextIntermediate!(logicalPoint({ x: 3, y: 3 }));

        const result = await promise;
        expect(result).toEqual([intermediateMove2]);
    });

    it("should break when resolving is cancelled", async () => {
        const move = createFakeMove({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 1, y: 1 }),
        });
        const intermediateMove = createFakeMove({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 2, y: 2 }),
            intermediates: [
                { position: logicalPoint({ x: 1, y: 1 }), isCapture: false },
            ],
        });
        const legalMoves = new LegalMoves([move, intermediateMove]);

        const promise = store
            .getState()
            .disambiguateIntermediates(
                move.to,
                legalMoves.getDirectNode(move.from, move.to)!,
                pieceId,
                pieces,
            );

        store.getState().resolveNextIntermediate!(null);

        const result = await promise;
        expect(result).toEqual([]);
    });

    it("should animate piece on each intermediate", async () => {
        const move = createFakeMove({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 1, y: 1 }),
        });
        const intermediateMove = createFakeMove({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 3, y: 3 }),
            intermediates: [
                { position: logicalPoint({ x: 1, y: 1 }), isCapture: false },
                { position: logicalPoint({ x: 2, y: 2 }), isCapture: false },
            ],
        });
        const legalMoves = new LegalMoves([move, intermediateMove]);

        const promise = store
            .getState()
            .disambiguateIntermediates(
                move.to,
                legalMoves.getDirectNode(move.from, move.to)!,
                pieceId,
                pieces,
            );

        expect(animatePieceMock).toHaveBeenCalledWith(
            pieceId,
            logicalPoint({ x: 1, y: 1 }),
            pieces,
        );

        store.getState().resolveNextIntermediate!(logicalPoint({ x: 2, y: 2 }));
        await flushMicrotasks();

        expect(animatePieceMock).toHaveBeenCalledWith(
            pieceId,
            logicalPoint({ x: 2, y: 2 }),
            pieces,
        );

        store.getState().resolveNextIntermediate!(logicalPoint({ x: 3, y: 3 }));
        const result = await promise;
        expect(result).toEqual([intermediateMove]);
    });

    it("should clean up state after finishing", async () => {
        const move = createFakeMove({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 2, y: 2 }),
            intermediates: [
                { position: logicalPoint({ x: 1, y: 1 }), isCapture: false },
            ],
        });
        const legalMoves = new LegalMoves([move]);
        const promise = store
            .getState()
            .disambiguateIntermediates(
                move.to,
                legalMoves.getDirectNode(
                    move.from,
                    move.intermediates[0].position,
                )!,
                pieceId,
                pieces,
            );

        expect(store.getState().nextIntermediates.length).toBeGreaterThan(0);
        store.getState().resolveNextIntermediate!(logicalPoint({ x: 2, y: 2 }));

        await promise;
        expect(store.getState().nextIntermediates).toEqual([]);
        expect(store.getState().resolveNextIntermediate).toBeNull();
    });
});
