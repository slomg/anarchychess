import { waitFor } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    createFakeMove,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";

import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { PieceType, SpecialMoveType } from "@/lib/apiClient";
import { logicalPoint } from "@/features/point/pointUtils";
import BoardPieces from "../../lib/boardPieces";
import LegalMoves from "../../lib/legalMoves";

describe("UiLegalMovesSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
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

        it("should prompt for throw when multiple throw moves exist", async () => {
            const origin = logicalPoint({ x: 1, y: 1 });
            const dest = logicalPoint({ x: 4, y: 4 });
            const piece = createFakePiece({ position: origin });
            const pieces = BoardPieces.fromPieces(piece);

            const throwMove1 = createFakeMove({
                from: origin,
                to: dest,
                specialType: SpecialMoveType.THROW,
            });
            const throwMove2 = createFakeMove({
                from: origin,
                to: dest,
                specialType: SpecialMoveType.THROW,
            });

            store
                .getState()
                .setLatestLegalMoves(new LegalMoves([throwMove1, throwMove2]));

            const promise = store
                .getState()
                .getLegalMove(dest, piece.id, pieces);

            await waitFor(() =>
                expect(store.getState().pendingThrow).not.toBeNull(),
            );

            const { pendingThrow } = store.getState();
            expect(pendingThrow).not.toBeNull();
            pendingThrow?.resolve(pendingThrow!.points[0]);

            const result = await promise;
            expect(result).toEqual(throwMove1);
        });

        it("should return null if throw is cancelled when multiple throw moves exist", async () => {
            const origin = logicalPoint({ x: 0, y: 0 });
            const dest = logicalPoint({ x: 3, y: 3 });
            const piece = createFakePiece({ position: origin });
            const pieces = BoardPieces.fromPieces(piece);

            const throwMove1 = createFakeMove({
                from: origin,
                to: dest,
                specialType: SpecialMoveType.THROW,
            });
            const throwMove2 = createFakeMove({
                from: origin,
                to: dest,
                specialType: SpecialMoveType.THROW,
            });

            store
                .getState()
                .setLatestLegalMoves(new LegalMoves([throwMove1, throwMove2]));

            const promise = store
                .getState()
                .getLegalMove(dest, piece.id, pieces);

            await waitFor(() => {
                expect(store.getState().pendingThrow).not.toBeNull();
            });

            store.getState().pendingThrow?.resolve(null);

            const result = await promise;
            expect(result).toBeNull();
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
