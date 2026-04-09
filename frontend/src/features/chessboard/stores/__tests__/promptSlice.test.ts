import { StoreApi } from "zustand";
import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { PromotionRequest } from "../promotionSlice";
import {
    createFakePiece,
    createRandomPoint,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { PieceType } from "@/lib/apiClient";
import { PendingIntermediate } from "../intermediateSlice";
import { PendingThrow } from "../throwSlice";

describe("PromptSlice", () => {
    let store: StoreApi<ChessboardStore>;
    const resolvePromotionMock = vi.fn();
    const resolveNextIntermediateMock = vi.fn();

    let pendingThrow: PendingThrow;

    beforeEach(() => {
        store = createChessboardStore();

        pendingThrow = {
            piece: createFakePiece(),
            points: [createRandomPoint(), createRandomPoint()],
            throwerOrigin: createRandomPoint(),
            resolve: vi.fn(),
        };
        store.setState({
            resolvePromotion: resolvePromotionMock,
            resolveNextIntermediate: resolveNextIntermediateMock,
            pendingThrow,
        });
    });

    describe("discardAllPrompts", () => {
        it("should resolve all prompts", () => {
            store.getState().discardAllPrompts();

            expect(resolvePromotionMock).toHaveBeenCalledExactlyOnceWith(null);
            expect(resolveNextIntermediateMock).toHaveBeenCalledExactlyOnceWith(
                null,
            );
            expect(pendingThrow.resolve).toHaveBeenCalledExactlyOnceWith(null);
        });
    });

    describe("discardPromptsForPiece", () => {
        it("should not prompts if they have the right piece", () => {
            const piece = createFakePiece();
            const pendingPromotion: PromotionRequest = {
                at: createRandomPoint(),
                pieces: [PieceType.KNOOK],
                piece,
            };
            const pendingIntermediate: PendingIntermediate = {
                nextOptions: [createRandomPoint(), createRandomPoint()],
                pieceId: piece.id,
            };
            pendingThrow.piece = piece;
            store.setState({
                pendingPromotion,
                pendingIntermediate,
                pendingThrow,
            });

            store.getState().discardPromptsForPiece(piece.id);

            expect(resolvePromotionMock).toHaveBeenCalledExactlyOnceWith(null);
            expect(resolveNextIntermediateMock).toHaveBeenCalledExactlyOnceWith(
                null,
            );
            expect(pendingThrow.resolve).toHaveBeenCalledExactlyOnceWith(null);
        });

        it("should not discard prompts if they have the wrong piece", () => {
            const pendingPromotion: PromotionRequest = {
                at: createRandomPoint(),
                pieces: [PieceType.KNOOK],
                piece: createFakePiece(),
            };
            const pendingIntermediate: PendingIntermediate = {
                nextOptions: [createRandomPoint(), createRandomPoint()],
                pieceId: "1",
            };
            store.setState({ pendingPromotion, pendingIntermediate });

            store.getState().discardPromptsForPiece("different piece");

            expect(resolvePromotionMock).not.toHaveBeenCalled();
            expect(resolveNextIntermediateMock).not.toHaveBeenCalled();
            expect(pendingThrow.resolve).not.toHaveBeenCalled();
        });
    });
});
