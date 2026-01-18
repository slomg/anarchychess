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

import useAnalysisMoveResolver from "../useAnalysisMoveResolver";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import {
    addAnalysisMove,
    AnalysisMoveArgs,
} from "../../lib/handleAnalysisMove";
import { RootAnalysisPosition } from "@/lib/apiClient";
import constants from "@/lib/constants";

vi.mock("@/features/analysis/lib/handleAnalysisMove");

describe("useAnalysisMoveResolver", () => {
    let chessboardStore: StoreApi<ChessboardStore>;
    let rootPosition: RootAnalysisPosition;
    let prevPieces: BoardPieces;

    const addAnalysisMoveMock = vi.mocked(addAnalysisMove);

    beforeEach(() => {
        chessboardStore = createChessboardStore();

        prevPieces = createFakeBoardPieces();
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

        renderHook(() =>
            useAnalysisMoveResolver(rootPosition, chessboardStore),
        );

        await chessboardStore
            .getState()
            .pieceMovementEvent.emit(move, prevPieces);

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
