import { renderHook } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import { RootAnalysisPosition } from "@/lib/apiClient";

import constants from "@/lib/constants";
import handleAnalysisMove from "../../lib/handleAnalysisMove";
import useAnalysisMoveResolver from "../useAnalysisMoveResolver";
import { createFakeMove } from "@/lib/testUtils/fakers/chessboardFakers";

vi.mock("@/features/analysis/lib/handleAnalysisMove");

describe("useAnalysisMoveResolver", () => {
    let chessboardStore: StoreApi<ChessboardStore>;
    let rootPosition: RootAnalysisPosition;

    const handleAnalysisMoveMock = vi.mocked(handleAnalysisMove);

    beforeEach(() => {
        chessboardStore = createChessboardStore();
        rootPosition = {
            fen: constants.INITIAL_FEN,
            moveOptions: {
                legalMoves: [
                    { fromIdx: 0, toIdx: 1, moveKey: "0" },
                    { fromIdx: 2, toIdx: 3, moveKey: "1" },
                ],
                hasForcedMoves: true,
            },
        };
    });

    it("should call handleAnalysisMove correctly", async () => {
        const move = createFakeMove();

        renderHook(() =>
            useAnalysisMoveResolver(rootPosition, chessboardStore),
        );

        await chessboardStore.getState().pieceMovementEvent.emit(move);

        expect(handleAnalysisMoveMock).toHaveBeenCalledExactlyOnceWith(
            chessboardStore,
            rootPosition.fen,
            move,
        );
    });
});
