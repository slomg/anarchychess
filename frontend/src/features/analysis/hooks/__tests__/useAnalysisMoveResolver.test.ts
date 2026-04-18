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
            legalMoves: [
                { fromIdx: 0, toIdx: 1, moveKey: "0" },
                { fromIdx: 2, toIdx: 3, moveKey: "1" },
            ],
        };
    });

    it("should call handleAnalysisMove correctly", async () => {
        const move = createFakeMove();
        const prevPieces = createFakeBoardPieces();

        renderHook(() =>
            useAnalysisMoveResolver(rootPosition, chessboardStore),
        );

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
            rootFen: rootPosition.fen,
            move,
        });
    });
});
