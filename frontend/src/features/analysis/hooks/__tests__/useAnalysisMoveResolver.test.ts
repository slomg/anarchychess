import { renderHook } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import {
    createFakeBoardPieces,
    createFakeMove,
} from "@/lib/testUtils/fakers/chessboardFakers";
import {
    addAnalysisMove,
    AnalysisMoveArgs,
} from "../../lib/handleAnalysisMove";

import useAnalysisMoveResolver from "../useAnalysisMoveResolver";

vi.mock("@/features/analysis/lib/handleAnalysisMove");

describe("useAnalysisMoveResolver", () => {
    let chessboardStore: StoreApi<ChessboardStore>;

    const addAnalysisMoveMock = vi.mocked(addAnalysisMove);

    beforeEach(() => {
        chessboardStore = createChessboardStore();
    });

    it("should call handleAnalysisMove correctly", async () => {
        const move = createFakeMove();
        const prevPieces = createFakeBoardPieces();

        renderHook(() => useAnalysisMoveResolver(chessboardStore));

        await chessboardStore.getState().pieceMovementEvent.emit({
            move,
            prevPieces,
            animationPromise: new Promise(() => {}),
        });

        expect(addAnalysisMoveMock).toHaveBeenCalledExactlyOnceWith<
            [AnalysisMoveArgs]
        >({
            chessboardStore,
            prevPieces,
            move,
        });
    });
});
