import { StoreApi } from "zustand";

import { createNFakePositionHistory } from "@/lib/testUtils/fakers/positionHistoryFaker";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import {
    createFakeBoardPieces,
    createFakeLegalMoves,
    createRandomPoint,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import PositionHistory from "../../lib/positionHistory";
import { PositionId } from "../../lib/position";
import { mock } from "vitest-mock-extended";
import { createFakePosition } from "@/lib/testUtils/fakers/positionFaker";
import LegalMoves from "../../lib/legalMoves";

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
            const unhighlightLegalMovesMock = vi.fn();
            const newPosition = createFakePosition();
            const newPositionProps = createFakePositionProps();
            const legalMoves = createFakeLegalMoves();

            positionHistoryMock.addNextPosition.mockReturnValue(newPosition);
            store.setState({
                positionHistory: positionHistoryMock,
                legalMovesByPosition: new Map(),
                unhighlightLegalMoves: unhighlightLegalMovesMock,
            });

            const result = store
                .getState()
                .addPosition(newPositionProps, legalMoves);

            expect(result).toBe(newPosition);
            expect(
                positionHistoryMock.addNextPosition,
            ).toHaveBeenCalledExactlyOnceWith(newPositionProps);
            expect(unhighlightLegalMovesMock).toHaveBeenCalledOnce();
            expect(store.getState().legalMovesByPosition).toEqual(
                new Map([[result.positionId, legalMoves]]),
            );
        });
    });

    describe("addSidelinePosition", () => {
        it("should add the new position as a slideline to positionHistory", async () => {
            const positionHistoryMock = mock<PositionHistory>();
            const unhighlightLegalMovesMock = vi.fn();
            const newPosition = createFakePosition();
            const newPositionProps = createFakePositionProps();
            const legalMoves = createFakeLegalMoves();
            positionHistoryMock.addNextSidelinePosition.mockReturnValue(
                newPosition,
            );
            store.setState({
                positionHistory: positionHistoryMock,
                legalMovesByPosition: new Map(),
                unhighlightLegalMoves: unhighlightLegalMovesMock,
            });

            const result = store
                .getState()
                .addSidelinePosition(newPositionProps, legalMoves);

            expect(result).toBe(newPosition);
            expect(
                positionHistoryMock.addNextSidelinePosition,
            ).toHaveBeenCalledExactlyOnceWith(newPositionProps);
            expect(unhighlightLegalMovesMock).toHaveBeenCalledOnce();
            expect(store.getState().legalMovesByPosition).toEqual(
                new Map([[result.positionId, legalMoves]]),
            );
        });
    });

    describe("getViewedPositionLegalMoves", () => {
        it("should return empty LegalMoves when hideLegalMoves is true", () => {
            const legalMoves = createFakeLegalMoves();
            const positionHistory = createNFakePositionHistory(1);

            store.setState({
                hideLegalMoves: true,
                positionHistory,
                legalMovesByPosition: new Map([
                    [positionHistory.viewingPosition?.positionId, legalMoves],
                ]),
            });

            const result = store.getState().getViewedPositionLegalMoves();
            expect(result).toEqual(new LegalMoves());
        });

        it("should return empty LegalMoves when viewing a non latest position and history changes are not allowed", () => {
            const legalMoves = createFakeLegalMoves();
            const positionHistory = createNFakePositionHistory(2);
            positionHistory.stepBackward();

            store.setState({
                allowHistoryChanges: false,
                positionHistory,
                legalMovesByPosition: new Map([
                    [positionHistory.viewingPosition?.positionId, legalMoves],
                ]),
            });

            const result = store.getState().getViewedPositionLegalMoves();
            expect(result).toEqual(new LegalMoves());
        });

        it("should return legal moves when viewing a non latest position and history changes are allowed", () => {
            const legalMoves = createFakeLegalMoves();
            const positionHistory = createNFakePositionHistory(2);
            positionHistory.stepBackward();

            store.setState({
                allowHistoryChanges: true,
                positionHistory,
                legalMovesByPosition: new Map([
                    [positionHistory.viewingPosition?.positionId, legalMoves],
                ]),
            });

            const result = store.getState().getViewedPositionLegalMoves();
            expect(result).toEqual(legalMoves);
        });

        it("should return legal moves for the current viewing position id", () => {
            const legalMoves = createFakeLegalMoves();
            const positionHistory = createNFakePositionHistory(1);

            store.setState({
                positionHistory,
                legalMovesByPosition: new Map([
                    [positionHistory.viewingPosition?.positionId, legalMoves],
                ]),
            });

            const result = store.getState().getViewedPositionLegalMoves();
            expect(result).toEqual(legalMoves);
        });

        it("should return empty LegalMoves when no legal moves exist for the viewing position", () => {
            const positionHistory = createNFakePositionHistory(1);

            store.setState({
                positionHistory,
                legalMovesByPosition: new Map(),
            });

            const result = store.getState().getViewedPositionLegalMoves();
            expect(result).toEqual(new LegalMoves());
        });

        it("should handle undefined viewing position id", () => {
            const legalMoves = createFakeLegalMoves();
            const positionHistory = createNFakePositionHistory(1);
            positionHistory.goToStart();

            store.setState({
                allowHistoryChanges: true,
                positionHistory,
                legalMovesByPosition: new Map([[undefined, legalMoves]]),
            });

            const result = store.getState().getViewedPositionLegalMoves();
            expect(result).toEqual(legalMoves);
        });
    });

    describe("addLegalMovesForPosition", () => {
        it("should store legal moves for a given position id and clear highlighted moves", () => {
            const legalMoves = createFakeLegalMoves();
            const positionId = "3" as PositionId;

            const legalMovesByPosition = new Map<PositionId, LegalMoves>([
                ["1" as PositionId, createFakeLegalMoves()],
            ]);
            store.setState({
                highlightedLegalMoves: [
                    createRandomPoint(),
                    createRandomPoint(),
                ],
                legalMovesByPosition,
            });

            store.getState().addLegalMovesForPosition(legalMoves, positionId);

            const state = store.getState();
            const expectedlegalMovesByPosition = new Map(legalMovesByPosition);
            expectedlegalMovesByPosition.set(positionId, legalMoves);
            expect(state.legalMovesByPosition).toEqual(
                expectedlegalMovesByPosition,
            );
            expect(state.highlightedLegalMoves).toHaveLength(0);
        });
    });

    describe("setLatestLegalMoves", () => {
        it("should store legal moves at the current viewing position id", () => {
            const legalMoves = createFakeLegalMoves();
            const positionHistory = createNFakePositionHistory(2);

            store.setState({
                highlightedLegalMoves: [
                    createRandomPoint(),
                    createRandomPoint(),
                    createRandomPoint(),
                ],
                positionHistory,
            });

            store.getState().setLatestLegalMoves(legalMoves);

            const state = store.getState();
            expect(
                state.legalMovesByPosition.get(
                    positionHistory.viewingPosition?.positionId,
                ),
            ).toEqual(legalMoves);
            expect(state.highlightedLegalMoves).toHaveLength(0);
        });
    });

    describe("hasLegalMovesForPosition", () => {
        it("should return false when no legal moves exist for the position id", () => {
            const result = store
                .getState()
                .hasLegalMovesForPosition("test position id" as PositionId);
            expect(result).toBe(false);
        });

        it("should return true when legal moves exist for the position id", () => {
            const { addPosition, addLegalMovesForPosition } = store.getState();
            const positionId = addPosition(
                createFakePositionProps(),
            ).positionId;
            addPosition(createFakePositionProps());
            addLegalMovesForPosition(createFakeLegalMoves(), positionId);

            const result = store
                .getState()
                .hasLegalMovesForPosition(positionId);
            expect(result).toBe(true);
        });

        it("should correctly handle undefined position id", () => {
            const { goToStartPosition, setLatestLegalMoves } = store.getState();
            goToStartPosition();
            setLatestLegalMoves(createFakeLegalMoves());

            const result = store.getState().hasLegalMovesForPosition(undefined);
            expect(result).toBe(true);
        });
    });

    describe("setAllowHistoryChanges", () => {
        it("should update allowHistoryChanges", () => {
            store.setState({ allowHistoryChanges: false });

            store.getState().setAllowHistoryChanges(true);

            expect(store.getState().allowHistoryChanges).toBe(true);
        });
    });
});
