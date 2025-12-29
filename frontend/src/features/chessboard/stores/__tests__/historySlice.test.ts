import { StoreApi } from "zustand";

import {
    createFakeMove,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";
import {
    createFakePosition,
    createFakeStartingPosition,
} from "@/lib/testUtils/fakers/positionFaker";

import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { AnimationStep, PositionHistory } from "../../lib/types";
import { PieceType, SpecialMoveType } from "@/lib/apiClient";
import { logicalPoint } from "@/features/point/pointUtils";
import BoardPieces from "../../lib/boardPieces";

describe("HistorySlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    describe("teleportToPosition", () => {
        let positionHistory: PositionHistory;
        const applyMoveAnimatedMock = vi.fn();
        const applyHistoryPositionMock = vi.fn();

        beforeEach(() => {
            positionHistory = [
                createFakeStartingPosition(),
                createFakePosition({ move: createFakeMove() }),
                createFakePosition({ move: createFakeMove() }),
            ];
            store.setState({
                positionHistory,
                viewingPlyIdx: 0,
                applyMoveAnimated: applyMoveAnimatedMock,
                applyHistoryPosition: applyHistoryPositionMock,
            });
        });

        it("should do nothing if plyIdx is out of bounds", async () => {
            await store.getState().teleportToPosition(-1);
            await store.getState().teleportToPosition(positionHistory.length);

            expect(store.getState().viewingPlyIdx).toBe(0);
            expect(applyMoveAnimatedMock).not.toHaveBeenCalled();
            expect(applyHistoryPositionMock).not.toHaveBeenCalled();
        });

        it("should hide legal moves", async () => {
            const hideLegalMovesMock = vi.fn();
            store.setState({ hideLegalMoves: hideLegalMovesMock });

            await store.getState().teleportToPosition(1);

            expect(hideLegalMovesMock).toHaveBeenCalledOnce();
        });

        it("should call applyMoveAnimated for exactly one step forward", async () => {
            await store.getState().teleportToPosition(1);

            expect(store.getState().viewingPlyIdx).toBe(1);
            expect(applyMoveAnimatedMock).toHaveBeenCalledWith(
                positionHistory[1].move,
            );
            expect(applyHistoryPositionMock).not.toHaveBeenCalled();
        });

        it("should set moveFromPreviousViewedPosition to next move when moving backward", async () => {
            store.setState({ viewingPlyIdx: 2 });

            await store.getState().teleportToPosition(1);

            expect(store.getState().viewingPlyIdx).toBe(1);
            expect(applyHistoryPositionMock).toHaveBeenCalledWith({
                moveFromPreviousViewedPosition: positionHistory[2].move, // next position's move
                position: positionHistory[1],
            });
        });

        it("should set moveFromPreviousViewedPosition to position.move when jumping forward more than one step", async () => {
            store.setState({ viewingPlyIdx: 0 });

            await store.getState().teleportToPosition(2);

            expect(store.getState().viewingPlyIdx).toBe(2);
            expect(applyHistoryPositionMock).toHaveBeenCalledWith({
                moveFromPreviousViewedPosition: positionHistory[2].move,
                position: positionHistory[2],
            });
        });

        it("should not call applyMoveAnimated if jumping more than one step or moving backward", async () => {
            store.setState({ viewingPlyIdx: 0 });

            await store.getState().teleportToPosition(2);
            await store.getState().teleportToPosition(1);

            expect(applyMoveAnimatedMock).not.toHaveBeenCalled();
            expect(applyHistoryPositionMock).toHaveBeenCalledTimes(2);
        });
    });

    describe("applyHistoryPosition", () => {
        it("should set pieces with playAnimation", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const newPos = logicalPoint({ x: 1, y: 1 });

            const playAnimationMock = vi.fn();
            store.setState({
                playAnimation: playAnimationMock,
                pieces: BoardPieces.fromPieces(piece),
            });

            const position = createFakePosition({
                pieces: BoardPieces.fromPieces({ ...piece, position: newPos }),
            });

            await store.getState().applyHistoryPosition({ position });

            expect(playAnimationMock).toHaveBeenCalledExactlyOnceWith({
                newPieces: position.pieces,
                movedPieceIds: [piece.id],
                isCapture: false,
                isPromotion: false,
            });
        });

        it("should set isCapture to true if moveFromPreviousViewedPosition is a capture", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const newPos = logicalPoint({ x: 1, y: 1 });
            const playAnimationMock = vi.fn();

            store.setState({
                playAnimation: playAnimationMock,
                pieces: BoardPieces.fromPieces(piece),
            });

            const position = createFakePosition({
                pieces: BoardPieces.fromPieces({
                    ...piece,
                    position: newPos,
                }),
            });
            const moveFromPreviousViewedPosition = createFakeMove({
                captures: [logicalPoint({ x: 1, y: 1 })],
            });

            await store.getState().applyHistoryPosition({
                position,
                moveFromPreviousViewedPosition,
            });

            expect(playAnimationMock).toHaveBeenCalledExactlyOnceWith<
                [AnimationStep]
            >({
                newPieces: position.pieces,
                movedPieceIds: [piece.id],
                isCapture: true,
                isPromotion: false,
                specialType: null,
            });
        });

        it("should pass specialType to playAnimation when present", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const newPos = logicalPoint({ x: 2, y: 2 });
            const playAnimationMock = vi.fn();

            store.setState({
                playAnimation: playAnimationMock,
                pieces: BoardPieces.fromPieces(piece),
            });

            const position = createFakePosition({
                pieces: BoardPieces.fromPieces({
                    ...piece,
                    position: newPos,
                }),
            });
            const moveFromPreviousViewedPosition = createFakeMove({
                specialType: SpecialMoveType.KNOOKLEAR_FUSION,
            });

            await store.getState().applyHistoryPosition({
                position,
                moveFromPreviousViewedPosition,
            });

            expect(playAnimationMock).toHaveBeenCalledExactlyOnceWith<
                [AnimationStep]
            >({
                newPieces: position.pieces,
                movedPieceIds: [piece.id],
                isCapture: false,
                isPromotion: false,
                specialType: SpecialMoveType.KNOOKLEAR_FUSION,
            });
        });

        it("should set isPromotion=true when promotesTo is defined", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const newPos = logicalPoint({ x: 0, y: 7 });
            const playAnimationMock = vi.fn();

            store.setState({
                playAnimation: playAnimationMock,
                pieces: BoardPieces.fromPieces(piece),
            });

            const position = createFakePosition({
                pieces: BoardPieces.fromPieces({
                    ...piece,
                    position: newPos,
                }),
            });
            const moveFromPreviousViewedPosition = createFakeMove({
                promotesTo: PieceType.QUEEN,
            });

            await store.getState().applyHistoryPosition({
                position,
                moveFromPreviousViewedPosition,
            });

            expect(playAnimationMock).toHaveBeenCalledExactlyOnceWith<
                [AnimationStep]
            >({
                newPieces: position.pieces,
                movedPieceIds: [piece.id],
                isCapture: false,
                isPromotion: true,
                specialType: null,
            });
        });

        it("should set moveBounds when moveThatProducedPosition is defined", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const newPos = logicalPoint({ x: 0, y: 4 });
            const playAnimationMock = vi.fn();

            store.setState({
                playAnimation: playAnimationMock,
                pieces: BoardPieces.fromPieces(piece),
            });

            const position = createFakePosition({
                pieces: BoardPieces.fromPieces({
                    ...piece,
                    position: newPos,
                }),
                move: createFakeMove({
                    from: piece.position,
                    to: newPos,
                }),
            });

            await store.getState().applyHistoryPosition({
                position,
                moveFromPreviousViewedPosition: createFakeMove(),
            });

            expect(playAnimationMock).toHaveBeenCalledExactlyOnceWith<
                [AnimationStep]
            >({
                newPieces: position.pieces,
                movedPieceIds: [piece.id],
                moveBounds: { from: piece.position, to: newPos },
                isCapture: false,
                isPromotion: false,
                specialType: null,
            });
        });
    });

    describe("shiftMoveViewBy", () => {
        const teleportMock = vi.fn();
        beforeEach(() => {
            store.setState({
                viewingPlyIdx: 1,
                teleportToPosition: teleportMock,
            });
        });

        it("should call teleportToPosition with correct new index", async () => {
            await store.getState().shiftMoveViewBy(2);
            expect(teleportMock).toHaveBeenCalledWith(3);

            await store.getState().shiftMoveViewBy(-1);
            expect(teleportMock).toHaveBeenCalledWith(0);
        });

        it("should correctly shift backward and forward multiple times", async () => {
            await store.getState().shiftMoveViewBy(1);
            await store.getState().shiftMoveViewBy(-2);

            expect(teleportMock).toHaveBeenNthCalledWith(1, 2);
            expect(teleportMock).toHaveBeenNthCalledWith(2, -1);
        });
    });

    describe("teleportToLatestPosition", () => {
        it("should call teleportToPosition with the last index", async () => {
            const teleportMock = vi.fn();
            const positionHistory: PositionHistory = [
                createFakeStartingPosition(),
                createFakePosition(),
                createFakePosition(),
            ];
            store.setState({
                positionHistory,
                teleportToPosition: teleportMock,
            });
            const historyLength = positionHistory.length;

            await store.getState().teleportToLatestPosition();

            expect(teleportMock).toHaveBeenCalledWith(historyLength - 1);
        });

        it("should throw if positionHistory is empty", async () => {
            store.setState({
                positionHistory: [] as unknown as PositionHistory,
                teleportToPosition: vi.fn(),
            });

            await expect(
                store.getState().teleportToLatestPosition(),
            ).rejects.toThrow("positionHistory is empty");
        });
    });

    describe("addPosition", () => {
        it("should call teleportToLatestPosition before adding the new position", () => {
            const teleportMock = vi.fn();
            store.setState({ teleportToLatestPosition: teleportMock });

            const newPosition = createFakePosition();
            store.getState().addPosition(newPosition);

            expect(teleportMock).toHaveBeenCalled();
        });

        it("should add the new position to positionHistory", () => {
            const newPosition = createFakePosition();
            const previousLength = store.getState().positionHistory.length;

            store.getState().addPosition(newPosition);

            const history = store.getState().positionHistory;
            expect(history.length).toBe(previousLength + 1);
            expect(history.at(-1)).toBe(newPosition);
        });

        it("should update viewingPlyIdx to the last index", () => {
            const newPosition = createFakePosition();
            store.getState().addPosition(newPosition);

            const historyLength = store.getState().positionHistory.length;
            expect(store.getState().viewingPlyIdx).toBe(historyLength - 1);
        });
    });
});
