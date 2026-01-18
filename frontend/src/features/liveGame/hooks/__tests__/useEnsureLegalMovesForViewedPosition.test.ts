import { renderHook } from "@testing-library/react";
import { StoreApi } from "zustand";
import { act } from "react";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import {
    createFakeBoardPieces,
    createFakeLegalMoves,
} from "@/lib/testUtils/fakers/chessboardFakers";

import useEnsureLegalMovesForViewedPosition from "../useEnsureLegalMovesForViewedPosition";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import { createFakeMovePath } from "@/lib/testUtils/fakers/movePathFaker";
import { decodeMovePathIntoLegalMoves } from "../../lib/moveDecoder";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { getNextLegalMoves } from "@/lib/apiClient";
import constants from "@/lib/constants";

import PositionHistory from "@/features/chessboard/lib/positionHistory";

vi.mock("@/lib/apiClient/definition");

describe("useEnsureLegalMovesForViewedPosition", () => {
    let chessboardStore: StoreApi<ChessboardStore>;
    let expectedLegalMoves: LegalMoves;
    const initialFen = "initial fen";

    const getNextLegalMovesMock = vi.mocked(getNextLegalMoves);
    const reselectPieceMock = vi.fn();

    beforeEach(() => {
        chessboardStore = createChessboardStore();
        chessboardStore.setState({ reselectPiece: reselectPieceMock });

        const legalMoves = [createFakeMovePath()];
        getNextLegalMovesMock.mockResolvedValue({
            error: undefined,
            data: legalMoves,
            response: new Response(),
        });

        expectedLegalMoves = decodeMovePathIntoLegalMoves({
            paths: legalMoves,
            boardWidth: constants.BOARD_WIDTH,
        });
    });

    it("should fetch and add legal moves when they do not exist for the viewed position", async () => {
        const position = createFakePositionProps();
        chessboardStore.setState({
            allowHistoryChanges: true,
            legalMovesByPosition: new Map(),
        });

        renderHook(() =>
            useEnsureLegalMovesForViewedPosition(initialFen, chessboardStore),
        );

        await act(() =>
            chessboardStore.getState().addPosition(createFakePositionProps()),
        );

        expect(getNextLegalMovesMock).toHaveBeenCalledWith({
            query: { fen: position.fen },
        });

        expect(
            chessboardStore.getState().getViewedPositionLegalMoves(),
        ).toEqual(expectedLegalMoves);
        expect(reselectPieceMock).toHaveBeenCalledOnce();
    });

    it("should not fetch legal moves if they already exist for the viewed position", async () => {
        chessboardStore.setState({ allowHistoryChanges: false });
        const prevLegalMoves = createFakeLegalMoves();
        chessboardStore.getState().setLatestLegalMoves(prevLegalMoves);

        renderHook(() =>
            useEnsureLegalMovesForViewedPosition(initialFen, chessboardStore),
        );

        act(() => chessboardStore.getState().setAllowHistoryChanges(true));

        expect(getNextLegalMovesMock).not.toHaveBeenCalled();
        expect(reselectPieceMock).not.toHaveBeenCalledOnce();
        expect(
            chessboardStore.getState().getViewedPositionLegalMoves(),
        ).toEqual(prevLegalMoves);
    });

    it("should fetch using initialFen if no viewing position exists", async () => {
        chessboardStore.setState({
            allowHistoryChanges: false,
            positionHistory: new PositionHistory(createFakeBoardPieces()),
            legalMovesByPosition: new Map(),
        });

        renderHook(() =>
            useEnsureLegalMovesForViewedPosition(initialFen, chessboardStore),
        );

        act(() => chessboardStore.getState().setAllowHistoryChanges(true));

        expect(getNextLegalMovesMock).toHaveBeenCalledWith({
            query: { fen: initialFen },
        });
    });

    it("should not fetch legal moves when history changes are not allowed", async () => {
        const position = createFakePositionProps();

        chessboardStore.setState({
            allowHistoryChanges: false,
            legalMovesByPosition: new Map(),
        });

        renderHook(() =>
            useEnsureLegalMovesForViewedPosition(initialFen, chessboardStore),
        );

        await act(() => chessboardStore.getState().addPosition(position));

        expect(getNextLegalMovesMock).not.toHaveBeenCalled();
        expect(reselectPieceMock).not.toHaveBeenCalledOnce();
        expect(
            chessboardStore.getState().getViewedPositionLegalMoves().size,
        ).toBe(0);
    });
});
