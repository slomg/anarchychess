import { StoreApi } from "zustand";

import {
    createFakePiece,
    createRandomPoint,
} from "@/lib/testUtils/fakers/chessboardFakers";

import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { logicalPoint, screenPoint } from "@/features/point/pointUtils";
import mockSequentialUUID from "@/lib/testUtils/mocks/mockUuids";
import createDefaultChessboard from "../../lib/defaultBoard";
import PositionHistory from "../../lib/positionHistory";
import { GameColor, PieceType } from "@/lib/apiClient";
import BoardPieces from "../../lib/boardPieces";
import { MoveBounds } from "../../lib/types";

describe("SetupModeSlice", () => {
    let store: StoreApi<ChessboardStore>;
    let lastMove: MoveBounds;

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

        lastMove = {
            from: createRandomPoint(),
            to: createRandomPoint(),
        };
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
                lastMove,
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

        it("should remove piece if destination is outside board", () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });

            const pieces = BoardPieces.fromPieces(piece);
            const positionHistory = new PositionHistory({ pieces });

            store.setState({
                pieces,
                positionHistory,
                selectedPieceId: piece.id,
                lastMove,
            });

            store
                .getState()
                .makeSetupModeMove(screenPoint({ x: 9999, y: 9999 }));

            const expectedPieces = new BoardPieces(pieces);
            expectedPieces.remove(piece.id);

            expect(store.getState().pieces).toEqual(expectedPieces);
            expect(store.getState().positionHistory.root.pieces).toEqual(
                expectedPieces,
            );
            expect(store.getState().lastMove).toBeNull();
        });
    });

    describe("addSetupModePiece", () => {
        it("should add a piece and override root", () => {
            const pieces = new BoardPieces();

            store.setState({
                pieces: new BoardPieces(pieces),
                positionHistory: new PositionHistory({ pieces }),
                lastMove,
            });

            mockSequentialUUID();
            store
                .getState()
                .addSetupModePiece(
                    PieceType.ROOK,
                    GameColor.WHITE,
                    screenPoint({ x: 20, y: 20 }),
                );

            const dest = logicalPoint({ x: 2, y: 7 });
            const expectedPieces = new BoardPieces(pieces);
            expectedPieces.add({
                id: "0",
                type: PieceType.ROOK,
                color: GameColor.WHITE,
                position: dest,
                stunnedForTurns: 0,
                hasMoved: false,
            });

            expect(store.getState().positionHistory.root.pieces).toEqual(
                expectedPieces,
            );
            expect(store.getState().pieces).toEqual(expectedPieces);
            expect(store.getState().lastMove).toBeNull();
        });
    });

    describe("clearSetupModeBoard", () => {
        it("should clear all pieces and override root", () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const pieces = BoardPieces.fromPieces(piece);
            store.setState({
                pieces: new BoardPieces(pieces),
                positionHistory: new PositionHistory({ pieces }),
                lastMove,
            });

            store.getState().clearSetupModeBoard();

            const expectedPieces = new BoardPieces();
            expect(store.getState().positionHistory.root.pieces).toEqual(
                expectedPieces,
            );
            expect(store.getState().pieces).toEqual(expectedPieces);
            expect(store.getState().lastMove).toBeNull();
        });
    });

    describe("resetSetupModeBoard", () => {
        it("should reset pieces to default chessboard and override root", () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const pieces = BoardPieces.fromPieces(piece);
            store.setState({
                pieces: new BoardPieces(pieces),
                positionHistory: new PositionHistory({ pieces }),
                lastMove,
            });

            store.getState().resetSetupModeBoard();

            const expectedPieces = createDefaultChessboard();
            expect(store.getState().positionHistory.root.pieces).toEqual(
                expectedPieces,
            );
            expect(store.getState().pieces).toEqual(expectedPieces);
            expect(store.getState().lastMove).toBeNull();
        });
    });

    describe("setSetupModeSideToMove", () => {
        it("should set side to move", () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const pieces = BoardPieces.fromPieces(piece);
            const positionHistory = new PositionHistory({ pieces });

            store.setState({
                pieces,
                positionHistory,
                lastMove,
            });

            store.getState().setSetupModeSideToMove(GameColor.BLACK);

            expect(store.getState().positionHistory.root.pieces).toEqual(
                pieces,
            );
            expect(store.getState().positionHistory.root.sideToMove).toBe(
                GameColor.BLACK,
            );
            expect(store.getState().lastMove).toBeNull();
        });
    });
});
