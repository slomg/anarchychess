import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import createDefaultChessboard from "@/features/chessboard/lib/defaultBoard";
import { createFakeMovePath } from "@/lib/testUtils/fakers/movePathFaker";
import processRootAnalysis from "../rootAnalysisPositionProcessor";
import { GameColor, RootAnalysisPosition } from "@/lib/apiClient";
import mockSequentialUUID from "@/lib/testUtils/mocks/mockUuids";
import constants from "@/lib/constants";

describe("processRootAnalysis", () => {
    it("should create a chessboard store with correct initial state", () => {
        mockSequentialUUID();

        const position: RootAnalysisPosition = {
            fen: constants.INITIAL_FEN,
            legalMoves: [createFakeMovePath()],
        };

        const store = processRootAnalysis(position);
        const state = store.getState();

        const expectedLegalMoves = decodeMovePathIntoLegalMoves(
            position.legalMoves,
        );

        expect(state.pieces).toEqual(createDefaultChessboard());

        expect(state.viewingFrom).toBe(GameColor.WHITE);
        expect(state.allowHistoryChanges).toBe(true);

        expect(state.positionHistory.totalPlyCount).toBe(0);
        expect(state.legalMovesByPosition.size).toBe(1);
        expect(
            state.legalMovesByPosition.get(
                state.positionHistory.viewingPosition?.positionId,
            ),
        ).toEqual(expectedLegalMoves);
    });

    it("should initialize position history from the provided FEN", () => {
        mockSequentialUUID();

        const position: RootAnalysisPosition = {
            fen: constants.INITIAL_FEN,
            legalMoves: [],
        };

        const store = processRootAnalysis(position);
        const state = store.getState();

        expect(state.positionHistory.root.pieces).toEqual(
            createDefaultChessboard(),
        );
        expect(state.positionHistory.totalPlyCount).toEqual(0);
    });
});
