import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import processRootAnalysis from "../rootAnalysisPositionProcessor";
import { GameColor, RootAnalysisPosition } from "@/lib/apiClient";
import mockSequentialUUID from "@/lib/testUtils/mocks/mockUuids";
import constants from "@/lib/constants";
import { createFakeMovePath } from "@/lib/testUtils/fakers/movePathFaker";

describe("processRootAnalysis", () => {
    it("should create a chessboard store with correct initial state", () => {
        mockSequentialUUID();

        const position: RootAnalysisPosition = {
            fen: constants.INITIAL_FEN,
            moveOptions: {
                legalMoves: [createFakeMovePath()],
                hasForcedMoves: true,
            },
        };

        const store = processRootAnalysis(position);
        const state = store.getState();

        const expectedLegalMoves = decodeMovePathIntoLegalMoves({
            paths: position.moveOptions.legalMoves,
            boardWidth: constants.BOARD_WIDTH,
            hasForcedMoves: position.moveOptions.hasForcedMoves,
        });

        expect(state.pieces).toEqual(constants.DEFAULT_CHESS_BOARD);

        expect(state.boardDimensions).toEqual({
            width: constants.BOARD_WIDTH,
            height: constants.BOARD_HEIGHT,
        });

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
            moveOptions: {
                legalMoves: [],
                hasForcedMoves: false,
            },
        };

        const store = processRootAnalysis(position);
        const state = store.getState();

        expect(state.positionHistory.rootPieces).toEqual(
            constants.DEFAULT_CHESS_BOARD,
        );
        expect(state.positionHistory.totalPlyCount).toEqual(0);
    });
});
