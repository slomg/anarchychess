import { renderHook } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import { createFakeMove } from "@/lib/testUtils/fakers/chessboardFakers";
import useAnalysisMoveResolver from "../useAnalysisMoveResolver";
import { addAnalysisMove } from "../../lib/handleAnalysisMove";
import { RootAnalysisPosition } from "@/lib/apiClient";
import constants from "@/lib/constants";

vi.mock("@/features/analysis/lib/handleAnalysisMove");

describe("useAnalysisMoveResolver", () => {
    let chessboardStore: StoreApi<ChessboardStore>;
    let rootPosition: RootAnalysisPosition;

    const addAnalysisMoveMock = vi.mocked(addAnalysisMove);

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

        expect(addAnalysisMoveMock).toHaveBeenCalledExactlyOnceWith(
            chessboardStore,
            rootPosition.fen,
            move,
        );
    });
});
