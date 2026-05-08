import { StoreApi } from "zustand";

import {
    createFakePiece,
    createRandomPoint,
} from "@/lib/testUtils/fakers/chessboardFakers";

import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { logicalPoint, screenPoint } from "@/features/point/pointUtils";
import PositionHistory from "../../lib/positionHistory";
import BoardPieces from "../../lib/boardPieces";

describe("SetupModeSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
        store.setState({
            boardRect: {
                left: 0,
                top: 0,
                width: 100,
                height: 100,
            } as DOMRect,
        });
    });

    describe("setSetupMode", () => {
        it("should set isSetupMode", () => {
            store.setState({ isSetupMode: false });
            const setSetupMode = store.getState().setSetupMode;

            setSetupMode(true);
            expect(store.getState().isSetupMode).toBe(true);

            setSetupMode(false);
            expect(store.getState().isSetupMode).toBe(false);
        });

        it("should discard all prompts", () => {
            const discardAllPromptsMock = vi.fn();
            store.setState({ discardAllPrompts: discardAllPromptsMock });

            store.getState().setSetupMode(false);

            expect(discardAllPromptsMock).toHaveBeenCalledOnce();
        });
    });

    describe("makeSetupModeMove", () => {
        it("should do nothing if no piece is selected", () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 2, y: 7 }),
            });
            const pieces = BoardPieces.fromPieces(piece);
            const positionHistory = new PositionHistory({ pieces });

            store.setState({
                pieces,
                positionHistory,
                selectedPieceId: undefined,
            });

            store.getState().makeSetupModeMove(screenPoint({ x: 20, y: 20 }));

            expect(store.getState().positionHistory).toBe(positionHistory);
            expect(store.getState().pieces).toBe(pieces);
        });

        it("should do nothing if selected piece is missing", () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 2, y: 7 }),
            });
            const pieces = BoardPieces.fromPieces(piece);
            const positionHistory = new PositionHistory({ pieces });

            store.setState({
                pieces,
                positionHistory,
                selectedPieceId: "missing",
            });

            store.getState().makeSetupModeMove(screenPoint({ x: 20, y: 20 }));

            expect(store.getState().positionHistory).toBe(positionHistory);
            expect(store.getState().pieces).toBe(pieces);
        });

        it("should do nothing if moving to same position", () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 2, y: 7 }),
            });
            const pieces = BoardPieces.fromPieces(piece);
            const positionHistory = new PositionHistory({ pieces });

            store.setState({
                pieces,
                positionHistory,
                selectedPieceId: piece.id,
            });

            store.getState().makeSetupModeMove(screenPoint({ x: 20, y: 20 }));

            expect(store.getState().positionHistory).toBe(positionHistory);
            expect(store.getState().pieces).toBe(pieces);
        });

        it("should move piece and override root", () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const pieces = BoardPieces.fromPieces(piece);

            store.setState({
                pieces: new BoardPieces(pieces),
                positionHistory: new PositionHistory({ pieces }),
                selectedPieceId: piece.id,
                lastMove: {
                    from: createRandomPoint(),
                    to: createRandomPoint(),
                },
            });

            const to = logicalPoint({ x: 2, y: 7 });

            store.getState().makeSetupModeMove(screenPoint({ x: 20, y: 20 }));

            const expectedPieces = new BoardPieces(pieces);
            expectedPieces.movePiece(piece.id, to);

            expect(store.getState().positionHistory.root.pieces).toEqual(
                expectedPieces,
            );
            expect(store.getState().pieces).toEqual(expectedPieces);
            expect(store.getState().lastMove).toBeNull();
        });
    });
});
