import { StoreApi } from "zustand";

import { createNFakePositionHistory } from "@/lib/testUtils/fakers/positionHistoryFaker";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import { createFakeBoardPieces } from "@/lib/testUtils/fakers/chessboardFakers";
import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import PositionHistory from "../../lib/positionHistory";
import { PositionId } from "../../lib/position";
import { mock } from "vitest-mock-extended";
import { createFakePosition } from "@/lib/testUtils/fakers/positionFaker";

describe("HistorySlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    describe("goToPosition", () => {
        const applyMoveAnimatedMock = vi.fn();
        const updatePiecesFromPositionMock = vi.fn();

        beforeEach(() => {
            store.setState({
                applyMoveAnimated: applyMoveAnimatedMock,
                updatePiecesFromPosition: updatePiecesFromPositionMock,
            });
        });

        it("should not do anything if goToPosition fails", async () => {
            const positionHistory = createNFakePositionHistory(2);
            store.setState({ positionHistory });

            await store
                .getState()
                .goToPosition("non-existent-id" as PositionId);

            expect(applyMoveAnimatedMock).not.toHaveBeenCalled();
            expect(updatePiecesFromPositionMock).not.toHaveBeenCalled();
        });

        it("should animate the move if stepping exactly one position forward", async () => {
            const positionHistory = new PositionHistory(
                createFakeBoardPieces(),
            );
            const pos1 = positionHistory.addNextPosition(
                createFakePositionProps(),
            );
            const pos2 = positionHistory.addNextPosition(
                createFakePositionProps(),
            );

            positionHistory.goToPosition(pos1.positionId);
            store.setState({ positionHistory });

            await store.getState().goToPosition(pos2.positionId);

            expect(applyMoveAnimatedMock).toHaveBeenCalledExactlyOnceWith(
                pos2.move,
            );
            expect(updatePiecesFromPositionMock).not.toHaveBeenCalled();
        });

        it("should update pieces from position when jumping backward", async () => {
            const positionHistory = new PositionHistory(
                createFakeBoardPieces(),
            );
            const pos1 = positionHistory.addNextPosition(
                createFakePositionProps(),
            );
            const pos2 = positionHistory.addNextPosition(
                createFakePositionProps(),
            );

            positionHistory.goToPosition(pos2.positionId);
            store.setState({ positionHistory });

            await store.getState().goToPosition(pos1.positionId);

            expect(
                updatePiecesFromPositionMock,
            ).toHaveBeenCalledExactlyOnceWith(pos1);
            expect(applyMoveAnimatedMock).not.toHaveBeenCalled();
        });

        it("should update pieces from position when jumping multiple steps forward", async () => {
            const positionHistory = new PositionHistory(
                createFakeBoardPieces(),
            );
            const pos1 = positionHistory.addNextPosition(
                createFakePositionProps(),
            );
            positionHistory.addNextPosition(createFakePositionProps());
            const pos3 = positionHistory.addNextPosition(
                createFakePositionProps(),
            );

            positionHistory.goToPosition(pos1.positionId);
            store.setState({ positionHistory });

            await store.getState().goToPosition(pos3.positionId);

            expect(
                updatePiecesFromPositionMock,
            ).toHaveBeenCalledExactlyOnceWith(pos3);
            expect(applyMoveAnimatedMock).not.toHaveBeenCalled();
        });
    });

    describe("stepPositionForward", () => {
        const applyMoveAnimatedMock = vi.fn();

        beforeEach(() => {
            store.setState({
                applyMoveAnimated: applyMoveAnimatedMock,
            });
        });

        it("should not do anything if we're at the final position", async () => {
            store.setState({ positionHistory: createNFakePositionHistory(3) });

            await store.getState().stepPositionForward();

            expect(applyMoveAnimatedMock).not.toHaveBeenCalled();
        });

        it("should animate the move if we're not at the final position", async () => {
            const positionHistory = new PositionHistory(
                createFakeBoardPieces(),
            );
            const pos1 = positionHistory.addNextPosition(
                createFakePositionProps(),
            );
            const pos2 = positionHistory.addNextPosition(
                createFakePositionProps(),
            );
            positionHistory.goToPosition(pos1.positionId);
            store.setState({ positionHistory });

            await store.getState().stepPositionForward();

            expect(applyMoveAnimatedMock).toHaveBeenCalledExactlyOnceWith(
                pos2.move,
            );
        });
    });

    describe("stepPositionBackward", () => {
        const updatePiecesFromPositionMock = vi.fn();
        const updatePiecesMock = vi.fn();

        beforeEach(() => {
            store.setState({
                updatePiecesFromPosition: updatePiecesFromPositionMock,
                updatePieces: updatePiecesMock,
            });
        });

        it("should not do anything if we're already on root position", async () => {
            const positionHistory = createNFakePositionHistory(3);
            positionHistory.goToStart();
            store.setState({ positionHistory });

            await store.getState().stepPositionBackward();

            expect(updatePiecesFromPositionMock).not.toHaveBeenCalled();
            expect(updatePiecesMock).not.toHaveBeenCalled();
        });

        it("should update pieces if we reached root position", async () => {
            const positionHistory = new PositionHistory(
                createFakeBoardPieces(),
            );
            const pos1 = positionHistory.addNextPosition(
                createFakePositionProps(),
            );
            positionHistory.addNextPosition(createFakePositionProps());
            positionHistory.goToPosition(pos1.positionId);
            store.setState({ positionHistory });

            await store.getState().stepPositionBackward();

            expect(updatePiecesFromPositionMock).not.toHaveBeenCalled();
            expect(updatePiecesMock).toHaveBeenCalledExactlyOnceWith(
                positionHistory.rootPieces,
            );
        });

        it("should update pieces from position if we aren't at root position yet", async () => {
            const positionHistory = new PositionHistory(
                createFakeBoardPieces(),
            );
            const pos1 = positionHistory.addNextPosition(
                createFakePositionProps(),
            );
            positionHistory.addNextPosition(createFakePositionProps());
            store.setState({ positionHistory });

            await store.getState().stepPositionBackward();

            expect(
                updatePiecesFromPositionMock,
            ).toHaveBeenCalledExactlyOnceWith(pos1);
            expect(updatePiecesMock).not.toHaveBeenCalled();
        });
    });

    describe("goToStartPosition", () => {
        it("should update pieces with the root pieces", async () => {
            const updatePiecesMock = vi.fn();
            const positionHistory = createNFakePositionHistory(3);
            store.setState({
                positionHistory,
                updatePieces: updatePiecesMock,
            });

            await store.getState().goToStartPosition();

            expect(updatePiecesMock).toHaveBeenCalledExactlyOnceWith(
                positionHistory.rootPieces,
            );
        });
    });

    describe("goToLatestPosition", () => {
        it("should update pieces from position with correct position", async () => {
            const updatePiecesFromPositionMock = vi.fn();

            const positionHistory = createNFakePositionHistory(3);
            const expectedPosition = positionHistory.viewingPosition;
            positionHistory.goToStart();

            store.setState({
                positionHistory,
                updatePiecesFromPosition: updatePiecesFromPositionMock,
            });

            await store.getState().goToLatestPosition();

            expect(
                updatePiecesFromPositionMock,
            ).toHaveBeenCalledExactlyOnceWith(expectedPosition);
        });
    });

    describe("addPosition", () => {
        it("should add the new position to positionHistory", async () => {
            const positionHistoryMock = mock<PositionHistory>();
            const newPosition = createFakePosition();
            const newPositionProps = createFakePositionProps();
            positionHistoryMock.addNextPosition.mockReturnValue(newPosition);
            store.setState({ positionHistory: positionHistoryMock });

            const result = store.getState().addPosition(newPositionProps);

            expect(result).toBe(newPosition);
            expect(
                positionHistoryMock.addNextPosition,
            ).toHaveBeenCalledExactlyOnceWith(newPositionProps);
        });
    });

    describe("addSidelinePosition", () => {
        it("should add the new position as a slideline to positionHistory", async () => {
            const positionHistoryMock = mock<PositionHistory>();
            const newPosition = createFakePosition();
            const newPositionProps = createFakePositionProps();
            positionHistoryMock.addNextSidelinePosition.mockReturnValue(
                newPosition,
            );
            store.setState({ positionHistory: positionHistoryMock });

            const result = store
                .getState()
                .addSidelinePosition(newPositionProps);

            expect(result).toBe(newPosition);
            expect(
                positionHistoryMock.addNextSidelinePosition,
            ).toHaveBeenCalledExactlyOnceWith(newPositionProps);
        });
    });
});
