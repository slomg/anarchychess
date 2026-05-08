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
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import { decodeMovePathIntoLegalMoves } from "../../lib/moveDecoder";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { getNextLegalMoves } from "@/lib/apiClient";

vi.mock("@/lib/apiClient/definition");

describe("useEnsureLegalMovesForViewedPosition", () => {
    let chessboardStore: StoreApi<ChessboardStore>;
    let expectedLegalMoves: LegalMoves;

    const getNextLegalMovesMock = vi.mocked(getNextLegalMoves);

    beforeEach(() => {
        chessboardStore = createChessboardStore();

        const legalMoves = [createFakeMovePath()];
        getNextLegalMovesMock.mockResolvedValue({
            error: undefined,
            data: legalMoves,
            response: new Response(),
        });

        expectedLegalMoves = decodeMovePathIntoLegalMoves(legalMoves);
    });

    it("should fetch and add legal moves when they do not exist for the viewed position", async () => {
        const position = createFakePositionProps();
        chessboardStore.setState({
            allowHistoryChanges: true,
            legalMovesByPosition: new Map(),
        });

        renderHook(() => useEnsureLegalMovesForViewedPosition(chessboardStore));

        await act(() =>
            chessboardStore.getState().addPosition(createFakePositionProps()),
        );

        expect(getNextLegalMovesMock).toHaveBeenCalledWith({
            query: { fen: position.fen },
        });

        expect(
            chessboardStore.getState().getViewedPositionLegalMoves(),
        ).toEqual(expectedLegalMoves);
    });

    it("should not fetch legal moves if they already exist for the viewed position", async () => {
        chessboardStore.setState({ allowHistoryChanges: false });
        const prevLegalMoves = createFakeLegalMoves();
        chessboardStore.getState().setLatestLegalMoves(prevLegalMoves);

        renderHook(() => useEnsureLegalMovesForViewedPosition(chessboardStore));

        act(() => chessboardStore.getState().setAllowHistoryChanges(true));

        expect(getNextLegalMovesMock).not.toHaveBeenCalled();
        expect(
            chessboardStore.getState().getViewedPositionLegalMoves(),
        ).toEqual(prevLegalMoves);
    });

    it("should fetch using root fen if no viewing position exists", async () => {
        chessboardStore.setState({
            allowHistoryChanges: false,
            positionHistory: new PositionHistory({
                pieces: createFakeBoardPieces(),
                fen: "test fen",
            }),
            legalMovesByPosition: new Map(),
        });

        renderHook(() => useEnsureLegalMovesForViewedPosition(chessboardStore));

        act(() => chessboardStore.getState().setAllowHistoryChanges(true));

        expect(getNextLegalMovesMock).toHaveBeenCalledWith({
            query: { fen: "test fen" },
        });
    });

    it("should not fetch legal moves when history changes are not allowed", async () => {
        const position = createFakePositionProps();

        chessboardStore.setState({
            allowHistoryChanges: false,
            legalMovesByPosition: new Map(),
        });

        renderHook(() => useEnsureLegalMovesForViewedPosition(chessboardStore));

        await act(() => chessboardStore.getState().addPosition(position));

        expect(getNextLegalMovesMock).not.toHaveBeenCalled();
        expect(
            chessboardStore.getState().getViewedPositionLegalMoves(),
        ).toEqual(new LegalMoves());
    });
});
