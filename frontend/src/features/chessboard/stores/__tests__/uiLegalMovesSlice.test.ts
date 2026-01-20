import { waitFor } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    createFakeMove,
    createFakePiece,
    createRandomPoint,
} from "@/lib/testUtils/fakers/chessboardFakers";

import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { IntermediateSquare, Piece } from "../../lib/types";
import { logicalPoint } from "@/features/point/pointUtils";
import BoardPieces from "../../lib/boardPieces";
import LegalMoves from "../../lib/legalMoves";
import { PieceType } from "@/lib/apiClient";

describe("UiLegalMovesSlice", () => {
    let store: StoreApi<ChessboardStore>;
    let piece: Piece;

    beforeEach(() => {
        store = createChessboardStore();
        piece = createFakePiece();
    });

    describe("getLegalMove", () => {
        it("should return null if no legal moves exist for the origin", async () => {
            const origin = logicalPoint({ x: 1, y: 2 });
            const dest = logicalPoint({ x: 3, y: 3 });
            const piece = createFakePiece({ position: origin });
            const pieces = BoardPieces.fromPieces(piece);

            store.setState({
                legalMovesByPosition: new Map(),
            });

            const result = await store
                .getState()
                .getLegalMove(dest, piece.id, pieces);
            expect(result).toBeNull();
        });

        it("should return null if no move matches the destination", async () => {
            const origin = logicalPoint({ x: 1, y: 1 });
            const dest = logicalPoint({ x: 5, y: 5 });
            const piece = createFakePiece({ position: origin });
            const pieces = BoardPieces.fromPieces(piece);

            const move = createFakeMove({
                from: origin,
                to: logicalPoint({ x: 2, y: 2 }),
            });
            const legalMoves = new LegalMoves([move]);
            const { setLatestLegalMoves, getLegalMove } = store.getState();
            setLatestLegalMoves(legalMoves);

            const result = await getLegalMove(dest, piece.id, pieces);
            expect(result).toBeNull();
        });

        it("should return the single matching move if only one matches", async () => {
            const origin = logicalPoint({ x: 2, y: 2 });
            const dest = logicalPoint({ x: 3, y: 3 });
            const piece = createFakePiece({ position: origin });
            const pieces = BoardPieces.fromPieces(piece);

            const move = createFakeMove({ from: origin, to: dest });
            const legalMoves = new LegalMoves([move]);
            const { setLatestLegalMoves, getLegalMove } = store.getState();
            setLatestLegalMoves(legalMoves);

            const result = await getLegalMove(dest, piece.id, pieces);
            expect(result).toEqual(move);
        });

        it("should return a move that matches via trigger", async () => {
            const origin = logicalPoint({ x: 4, y: 4 });
            const trigger = logicalPoint({ x: 6, y: 6 });
            const dest = logicalPoint({ x: 9, y: 9 });
            const piece = createFakePiece({ position: origin });
            const pieces = BoardPieces.fromPieces(piece);

            const triggerMove = createFakeMove({
                from: origin,
                to: dest,
                triggers: [trigger],
            });
            const regularMove = createFakeMove({ from: origin, to: dest });

            const legalMoves = new LegalMoves([triggerMove, regularMove]);

            const { setLatestLegalMoves, getLegalMove } = store.getState();
            setLatestLegalMoves(legalMoves);

            const result = await getLegalMove(trigger, piece.id, pieces);
            expect(result).toEqual(triggerMove);
        });

        it("should return a move that matches via trigger even if trigger wasn't used", async () => {
            const origin = logicalPoint({ x: 4, y: 4 });
            const trigger = logicalPoint({ x: 6, y: 6 });
            const dest = logicalPoint({ x: 9, y: 9 });
            const piece = createFakePiece({ position: origin });
            const pieces = BoardPieces.fromPieces(piece);

            const triggerMove = createFakeMove({
                from: origin,
                to: dest,
                triggers: [trigger],
            });
            const legalMoves = new LegalMoves([triggerMove]);
            const { setLatestLegalMoves, getLegalMove } = store.getState();
            setLatestLegalMoves(legalMoves);

            const result = await getLegalMove(dest, piece.id, pieces);
            expect(result).toEqual(triggerMove);
        });

        it("should return null if multiple moves match but promotion is cancelled", async () => {
            const origin = logicalPoint({ x: 0, y: 1 });
            const dest = logicalPoint({ x: 0, y: 7 });
            const piece = createFakePiece({ position: origin });
            const pieces = BoardPieces.fromPieces(piece);

            const queenMove = createFakeMove({
                from: origin,
                to: dest,
                promotesTo: PieceType.QUEEN,
            });
            const rookMove = createFakeMove({
                from: origin,
                to: dest,
                promotesTo: PieceType.ROOK,
            });

            const legalMoves = new LegalMoves([queenMove, rookMove]);

            const { setLatestLegalMoves, getLegalMove } = store.getState();
            store.setState({
                pendingPromotion: null,
                resolvePromotion: null,
            });
            setLatestLegalMoves(legalMoves);

            const promise = getLegalMove(dest, piece.id, pieces);

            await waitFor(() => {
                const state = store.getState();
                expect(state.resolvePromotion).not.toBeNull();
            });
            store.getState().resolvePromotion?.(null);

            const result = await promise;
            expect(result).toBeNull();
        });
    });

    describe("highlightLegalMoves", () => {
        it("should highlight unique points from 'to' and 'triggers'", () => {
            const piece = createFakePiece();
            const move1To = logicalPoint({ x: 3, y: 3 });
            const move2To = logicalPoint({ x: 4, y: 4 });
            const trigger1 = logicalPoint({ x: 5, y: 5 });
            const trigger2 = move1To; // same as move1To

            const move1 = createFakeMove({
                from: piece.position,
                to: move1To,
                triggers: [trigger1, trigger2],
            });
            const move2 = createFakeMove({
                from: piece.position,
                to: move2To,
                triggers: [],
            });

            const legalMoves = new LegalMoves([move1, move2]);

            const { setLatestLegalMoves, highlightLegalMoves } =
                store.getState();
            store.setState({ pieces: BoardPieces.fromPieces(piece) });
            setLatestLegalMoves(legalMoves);

            highlightLegalMoves(piece);

            const highlighted = store.getState().highlightedLegalMoves;
            expect(highlighted).toHaveLength(3);
            expect(highlighted).toEqual(
                expect.arrayContaining([move1To, move2To, trigger1]),
            );
        });

        it("should highlight the first intermediate instead of 'to'", () => {
            const intermediate: IntermediateSquare = {
                position: logicalPoint({ x: 1, y: 1 }),
                isCapture: false,
            };
            const destination = logicalPoint({ x: 2, y: 2 });
            const move = createFakeMove({
                from: piece.position,
                to: destination,
                intermediates: [intermediate],
            });

            const legalMoves = new LegalMoves([move]);

            const { setLatestLegalMoves, highlightLegalMoves } =
                store.getState();
            store.setState({ pieces: BoardPieces.fromPieces(piece) });
            setLatestLegalMoves(legalMoves);

            highlightLegalMoves(piece);

            const highlighted = store.getState().highlightedLegalMoves;
            expect(highlighted).toHaveLength(1);
            expect(highlighted[0]).toEqual(intermediate.position);
            expect(highlighted).not.toContainEqual(destination);
        });
    });

    describe("unhighlightLegalMoves", () => {
        it("should remove any highlighted legal moves", () => {
            store.setState({
                highlightedLegalMoves: [
                    createRandomPoint(),
                    createRandomPoint(),
                    createRandomPoint(),
                ],
            });

            store.getState().unhighlightLegalMoves();
            expect(store.getState().highlightedLegalMoves).toHaveLength(0);
        });
    });

    describe("setHideLegalMoves", () => {
        it("should update hideLegalMoves", () => {
            store.setState({ hideLegalMoves: false });

            store.getState().setHideLegalMoves(true);

            expect(store.getState().hideLegalMoves).toBe(true);
        });
    });
});
